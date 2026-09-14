#!/usr/bin/env bash
# NAS 端部署脚本（由 Windows 侧 scp 上来并由 deploy-nas.sh 远程触发）
# 用法: bash nas-deploy.sh <版本如 v1.2.1>
set -euo pipefail
VER="${1:?缺少版本参数}"
APP_PATH=/vol1/1000/docker/autofeedsiqi
DIR="$APP_PATH/Dockerfile"
# 带时间戳的日志输出（仅替换面向终端的 echo；管道/读文件的数据类 echo 保持原样）
log() { printf '[%s] %s\n' "$(date +%F_%T)" "$*"; }
# 出海代理端口：dockerd 已配 proxies（daemon.json）走此代理；buildx imagetools 等客户端直连 registry 时也用它
BUILDX_PROXY=http://192.168.0.118:10811

cd "$DIR"
log "==> 解包 update.tar.gz"
rm -rf update.update_old
[ -d update ] && mv update update.update_old
tar xzf update.tar.gz -C .
rm -rf update.update_old

# 预热基础镜像：tag 与 Dockerfile 的 FROM 保持一致（9.0.19 为精确版本 tag，升 .NET patch 时两处同步换）
# 单机一个 tag 只能指一个平台镜像（overlay2 经典 store），后拉平台会覆盖 tag，
# 故先拉 arm64 并打专属缓存 tag（保住不被下方 docker image prune 误删），再拉 amd64 使 9.0.19 归本机平台
ASPNET="mcr.microsoft.com/dotnet/aspnet:9.0.19"
docker pull --platform linux/arm64 "$ASPNET" >/dev/null 2>&1 || log "  [警告] arm64 aspnet 预热失败"
docker tag "$ASPNET" mcr.microsoft.com/dotnet/aspnet:9.0.19-arm64 >/dev/null 2>&1 || true
docker pull --platform linux/amd64 "$ASPNET" >/dev/null 2>&1 || log "  [警告] amd64 aspnet 预热失败"

# 发布链路说明（与 AutoFeedEmby 相同，已验证）：
#   docker-container buildkit 容器的 env 代理对 registry oauth 请求无效（直连 auth.docker.io 超时），
#   故改为：dockerd 内置 buildkit（docker driver）分架构单平台构建 + 单平台 push（dockerd 已配 proxies 出海）
#          + buildx imagetools 客户端经代理合并 multi-arch manifest。
#   tag 策略：Hub 只保留 latest / vX.Y.Z（multi-arch）+ latest-amd64/latest-arm64（滚动单平台），
#             版本级单平台 tag（vX.Y.Z-amd64/-arm64）不再推 Hub。

log "==> 本地 amd64 镜像（供 compose 用）"
docker build --platform linux/amd64 \
  -t "yxlonger/autofeedsiqi:${VER}-amd64" -t "yxlonger/autofeedsiqi:latest-amd64" .

log "==> arm64 单平台镜像（dockerd 内置 buildkit，纯 COPY 无 RUN 无需 QEMU）"
docker build --platform linux/arm64 \
  -t "yxlonger/autofeedsiqi:${VER}-arm64" -t "yxlonger/autofeedsiqi:latest-arm64" .

log "==> docker compose 使用本地 amd64 镜像"
docker tag "yxlonger/autofeedsiqi:${VER}-amd64" "yxlonger/autofeedsiqi:latest" >/dev/null 2>&1 || true

# 先本地后远端：up 用本地新镜像（不依赖 Hub），后面 push/merge 失败不影响服务
log "==> docker compose up -d（容器切到本地新镜像，唯一一次重启）"
cd "$APP_PATH"
docker compose up -d
cd "$DIR"

log "==> 分架构 push（仅滚动单平台 tag；版本级单平台 tag 不上 Hub）"
pids=()
for t in "latest-amd64" "latest-arm64"; do
  # 两平台 push 并行（dockerd 可并发；子 shell 保证各自的警告独立打印）
  ( docker push "yxlonger/autofeedsiqi:$t" || log "[警告] push $t 失败" ) &
  pids+=("$!")
done
for p in "${pids[@]}"; do wait "$p" || true; done

log "==> 合并 multi-arch manifest（基于滚动单平台 tag，写 Hub 的 latest 与 vX.Y.Z）"
merge_manifest() {
  local name="$1" attempt
  for attempt in 1 2 3; do
    HTTP_PROXY="$BUILDX_PROXY" HTTPS_PROXY="$BUILDX_PROXY" \
      docker buildx imagetools create \
      -t "yxlonger/autofeedsiqi:${name}" \
      "yxlonger/autofeedsiqi:latest-amd64" "yxlonger/autofeedsiqi:latest-arm64" && return 0
    log "[警告] 合并 manifest ${name} 失败（第${attempt}次），2秒后重试"
    sleep 2
  done
  log "[警告] 合并 manifest ${name} 最终失败（已重试3次）"
  return 1
}
# 两个 manifest 都基于同一对滚动单平台 tag，无依赖，并行合并省一半时间
merge_manifest "${VER}" &
merge_manifest latest &
wait || true

# 清理策略：保留集 = 容器在用 ID + 本次部署构建的全部本地 tag 镜像
# （${VER}-amd64/-arm64 + latest-amd64/-arm64 + latest）。
# 全量保留 latest 滚动 tag 是为应对 push 失败重试：镜像仍在本地，无需重建即可补推。
# 对 docker images -a 全量扫描，以 Entrypoint 含 AutoFeedSiqi.dll 判定本仓库产物，
# 不在保留集的一律删除（含旧版本号 tag 镜像与已剥 tag 的 <none> 遗留，旧办法按 reference filter 扫不到它们）
log "==> 清理 autofeedsiqi 旧镜像（保留容器在用 + 本次部署全部 tag）"
CUR=$(docker inspect -f '{{.Image}}' autofeedsiqi 2>/dev/null | sed 's/^sha256://' || true)
KEEP=$( { docker images --no-trunc --filter "reference=yxlonger/autofeedsiqi:${VER}-*" --format '{{.ID}}'
          docker images --no-trunc --filter "reference=yxlonger/autofeedsiqi:latest-*" --format '{{.ID}}'
          docker images --no-trunc --filter "reference=yxlonger/autofeedsiqi:latest" --format '{{.ID}}'
          echo "$CUR"; } | sed 's/^sha256://' | sort -u )
deleted=0
# 单次批量 inspect 一次取全部镜像 Id+Entrypoint（替代逐个 docker inspect spawn，镜像增多时避免线性退化），
# awk 只保留 Entrypoint 含 AutoFeedSiqi.dll 的本仓库产物；<(...) 进程替换保证 deleted 在主 shell 累加
while IFS= read -r id; do
  id=${id#sha256:}
  grep -qx "$id" <<<"$KEEP" && continue
  log "  [删除] $id"
  docker rmi -f "$id" >/dev/null 2>&1 && deleted=$((deleted+1)) || log "  [警告] 删除 $id 失败（可能仍有容器在用）"
done < <({ docker inspect -f '{{.Id}}|{{.Config.Entrypoint}}' $(docker images -aq) 2>/dev/null || true; } \
  | awk -F'|' '$2 ~ /AutoFeedSiqi\.dll/ { sub(/^sha256:/, "", $1); print $1 }' | sort -u)
log "  共清理 $deleted 个旧镜像"

log "==> 清理 dangling 镜像与 docker build cache"
docker image prune -f >/dev/null 2>&1 || log "  [警告] dangling 镜像清理失败"
docker builder prune -f >/dev/null 2>&1 || log "  [警告] build cache 清理失败"

log "部署完成: ${VER}"
