#!/usr/bin/env bash
# AutoFeedSiqi 一键部署到 NAS（fnOS），参照 AutoFeedEmby scripts/deploy-nas.sh
# 用法:
#   bash scripts/deploy-nas.sh                           自动发布：算版本+打tag+push+push tag+核对 后部署（需交互确认；--yes 跳过确认）
#   bash scripts/deploy-nas.sh [已存在版本]             仅部署（指定已存在 git tag，不打 tag 不 push）
#   bash scripts/deploy-nas.sh [版本] --rebuild-base    一次性重建 NAS 干净基线（缩小 base 层）
#   bash scripts/deploy-nas.sh --rebuild-base           自动发布 + 重建 base
# 说明:
# ⚠ 必须前台单次调用：勿管道(| tail/tee/less)、勿后台(&/setsid/nohup)，否则 opencode bash 工具在调用返回时杀进程组，脚本在 1/5 publish 阶段中断（机理见 AutoFeedEmby learnings/items/LRN-20260816-013.md）。规范：bash scripts/deploy-nas.sh vX.Y.Z --yes（超时 ≥600000ms）；中断即幂等重跑同命令。
#   发布 = 打 tag：自动模式由脚本统一算 SimpleVersion、打 tag、推送代码与 tag、核对远端，
#   调用脚本即视为授权；升 minor 仍需人工 nbgv set-version 提交后再跑。
# 说明:
#   base/update 分层目标：**base 尽量长期不变**（只装依赖层），日常应用/前端改动只进 update，
#   Hub 按层 digest 去重实现 base 秒传、每版只推小 update。
#   文件归属:
#     base   = *.dll(依赖) + libe_sqlite3.so + appsettings.json（依赖升级/基础配置才变）
#     update = AutoFeedSiqi.* 应用本体 + wwwroot/ 前端 + 与 base 有差异的其它文件
#     排除   = apphost / pdb / web.config / appsettings.Development.json / tsconfig.json
#     （*.br/*.gz 保留：.NET 9 MapStaticAssets 按 Accept-Encoding 提供压缩响应，剔除会致前端 200+空 body）
#   RID_ARCHS: 「架构目录名(= buildx TARGETARCH) → .NET RID」映射。
#   Dockerfile 按 TARGETARCH 选择对应架构目录（scripts/Dockerfile 随部署 scp 上传 NAS）。
set -euo pipefail
cd "$(git rev-parse --show-toplevel)"

# 带时间戳的日志输出（仅替换面向终端的 echo；管道/读文件的数据类 echo 保持原样）
log() { printf '[%s] %s\n' "$(date +%F_%T)" "$*"; }
# 总耗时起点与单步耗时输出（传入步骤名与该步开始秒级时间戳）
START_TS=$(date +%s)
step_done() { log "    $1 用时 $(( $(date +%s) - $2 ))s"; }

# 解析参数：可选版本（已存在 tag=仅部署模式）+ --rebuild-base / --yes 标志（位置无关）
VER=""
REBUILD=0
AUTO_YES=0
for a in "$@"; do
  case "$a" in
    --rebuild-base) REBUILD=1 ;;
    --yes) AUTO_YES=1 ;;
    -*) log "未知参数: $a" >&2; exit 1 ;;
    *)
      if [ -z "$VER" ]; then VER="$a"; else log "重复版本参数: $a" >&2; exit 1; fi
      ;;
  esac
done

if [ -z "$VER" ]; then
  # ===== 自动模式：算版本 + 打 tag + push + push tag + 核对（调用即视为授权）=====
  # 1) 工作区必须干净：nbgv 对脏工作区会注入杂质，破坏版本纯净
  if ! git diff-index --quiet HEAD --; then
    log "错误: 工作区存在未提交改动，请先 git commit 再发布（避免版本号污染）" >&2
    exit 1
  fi
  # 2) 用 nbgv 计算当前 HEAD 语义版本（务必 SimpleVersion，杜绝 -g{hash} 杂质）
  VER="v$(nbgv get-version -v SimpleVersion)"
  TAG="$VER"
  # 3) 防重复 tag（已存在则交回人工决定，不自动覆盖）
  if git rev-parse -q --verify "refs/tags/$TAG" >/dev/null; then
    log "错误: tag '$TAG' 已存在，跳过自动打 tag（如需重部署请用: bash $0 $TAG）" >&2
    exit 1
  fi
  BRANCH="$(git rev-parse --abbrev-ref HEAD)"
  log "==> 自动发布: 将打 tag '$TAG' 并推送分支 '$BRANCH' -> origin"
  if [ "$AUTO_YES" != "1" ]; then
    printf '确认打 tag 并推送? [y/N] '
    read -r ANS
    case "$ANS" in y|Y) ;; *) log "已取消"; exit 1 ;; esac
  fi
  # 4) 打本地 tag
  git tag "$TAG" HEAD
  # 5) 推送代码（失败保留本地 tag，提示人工续推/回滚）
  log "    git push origin $BRANCH"
  if ! git push origin "$BRANCH"; then
    log "错误: 推送分支失败，本地 tag 已打，请手动 'git push origin $TAG' 续推或 'git tag -d $TAG' 回滚" >&2
    exit 1
  fi
  # 6) 推送 tag
  if ! git push origin "$TAG"; then
    log "错误: 推送 tag 失败，请手动 'git push origin $TAG'" >&2
    exit 1
  fi
  # 7) 核对：tag 已落远端
  if git ls-remote --tags origin | grep -q "refs/tags/$TAG\$"; then
    log "    核对通过: $TAG 已存在于 origin"
  else
    log "错误: 核对失败，$TAG 未出现在 origin（请检查网络/代理后手动推送）" >&2
    exit 1
  fi
else
  # ===== 指定 tag 模式：仅部署，不打 tag 不 push =====
  # 发布 = 打 tag：显式传 VER 时必须存在对应 git tag，杜绝镜像 tag 与 git 版本脱节
  TAG="v${VER#v}"
  if ! git rev-parse -q --verify "refs/tags/$TAG" >/dev/null; then
    log "错误: 未找到 git tag '$TAG'，发布前请先打 tag：git tag $TAG HEAD" >&2
    log "（镜像 tag 应与 git 提交高度对齐）" >&2
    exit 1
  fi
fi

# 部署日志：终端实时显示进度 + 落盘 .deploy-logs/（gitignore 排除，不进 git）
LOG_DIR=".deploy-logs"
mkdir -p "$LOG_DIR"
LOG_FILE="$LOG_DIR/deploy-${VER}-$(date +%Y%m%d-%H%M%S).log"
exec > >(tee -a "$LOG_FILE") 2>&1
log "==> 部署日志: $LOG_FILE"

NAS_HOST=nas
NAS_APP_PATH=/vol1/1000/docker/autofeedsiqi
# G 盘挂载的 NAS 依赖基线（clouddrive2），仅增量 diff 对照用；重建走 ssh tar 管道避免多小文件
BASE_PATH="G:/fnOS/docker/autofeedsiqi/Dockerfile/base"
# 架构目录名 → .NET RID；新增架构在此追加（需同时保证 Dockerfile 的 COPY base/${TARGETARCH} 有对应目录）
RID_ARCHS="amd64:linux-x64 arm64:linux-arm64"
# base md5 清单本地缓存（gitignore 排除）：base 长期不变，指纹校验一致时直接复用，避免每次逐文件读 G 盘算 md5
CACHE_DIR=".deploy-cache"
mkdir -p "$CACHE_DIR"

STAGING=$(mktemp -d)
trap 'rm -rf "$STAGING"' EXIT

log "==> 1/5 dotnet publish -c Release（framework-dependent，各架构独立产物，并行）"
S1=$(date +%s)
# 一次 restore 同时还原全部 RID target（SDK 命令行 -p:RuntimeIdentifiers 的 ';' 会被拆成多属性，
# 需经环境变量传入）；随后各架构 publish --no-restore 并行编译，避免并发 restore 争写 assets
RIDS=""
for entry in $RID_ARCHS; do
  rid="${entry##*:}"
  [ -n "$RIDS" ] && RIDS="$RIDS;"
  RIDS="$RIDS$rid"
done
RuntimeIdentifiers="$RIDS" dotnet restore AutoFeedSiqi/AutoFeedSiqi.csproj >/dev/null
pids=()
for entry in $RID_ARCHS; do
  arch="${entry%%:*}"; rid="${entry##*:}"
  log "    -r $rid -> $arch"
  # 各架构 publish 后台并行（中间目录按 RID 隔离），wait 串行等结果以保住 set -e 失败语义
  dotnet publish AutoFeedSiqi/AutoFeedSiqi.csproj -c Release -r "$rid" \
    --no-restore --self-contained false -o "$STAGING/publish-$rid" >/dev/null &
  pids+=("$!")
done
for p in "${pids[@]}"; do wait "$p" || exit 1; done
step_done "1/5 publish" "$S1"

UP="$STAGING/update"
mkdir -p "$UP"

# 判定产物中不该进镜像的冗余文件：
#   AutoFeedSiqi          apphost 宿主程序（容器用 dotnet AutoFeedSiqi.dll 启动）
#   AutoFeedSiqi.pdb/.exe 调试符号 / Windows 宿主
#   web.config            IIS 专用
#   appsettings.Development.json  开发配置（容器仅用 appsettings.json）
# 注意：*.br/*.gz **不能**剔除——.NET 9 MapStaticAssets 发布产物含预压缩资产，
# 运行时按 Accept-Encoding 提供压缩响应，剔除后浏览器带 br/gzip 请求拿到 200+空 body，前端 JS/CSS 全部失效。
skip_waste() {
  case "$1" in
    AutoFeedSiqi|AutoFeedSiqi.pdb|AutoFeedSiqi.exe|web.config) return 0 ;;
    appsettings.Development.json) return 0 ;;
  esac
  return 1
}

# 判断文件归属 base（依赖层）；其余非 skip_waste 文件都进 update
is_base_file() {
  case "$1" in
    AutoFeedSiqi*.dll) return 1 ;;  # 项目 dll 永远走 update
    *.dll|libe_sqlite3.so|appsettings.json) return 0 ;;
  esac
  return 1
}

# 批量生成 md5 清单：每行 "<md5> <相对路径>"（剥掉 md5sum 二进制文件的 * 前缀与 ./ 前缀）
# 一次 xargs 调用算完全部文件，替代逐文件 spawn 子进程
md5_manifest() {
  ( cd "$1" && find . -type f -print0 | xargs -0 md5sum \
    | awk '{ p=$2; sub(/^\*?\.\//, "", p); print $1, p }' )
}

# 目录指纹：文件数 + 总字节数（仅读元数据，G 盘约 0.1s），用于 base 未变时命中缓存清单
dir_fingerprint() {
  ( cd "$1" && find . -type f -printf '%s\n' | awk '{ n++; s+=$1 } END { print n, s }' )
}

# 取 base md5 清单：指纹一致直接读缓存；否则现场批量算一次并存档（首次/--rebuild-base 后触发）
base_manifest() {
  local arch="$1"
  local fp; fp="$(dir_fingerprint "$BASE_PATH/$arch")"
  local fp_cache="$CACHE_DIR/base-$arch.fp"
  local md5_cache="$CACHE_DIR/base-$arch.md5"
  if [ -f "$fp_cache" ] && [ -f "$md5_cache" ] && [ "$(cat "$fp_cache")" = "$fp" ]; then
    cat "$md5_cache"
  else
    md5_manifest "$BASE_PATH/$arch" | tee "$md5_cache"
    echo "$fp" > "$fp_cache"
  fi
}

# 从 src 按 stdin 提供的相对路径列表（每行一个）用 tar 一次性批量复制到 dst，
# 替代逐文件 mkdir/cp 启动子进程
tar_copy() {
  local src="$1" dst="$2"
  ( cd "$src" && tar cf - --files-from=- ) | tar xf - -C "$dst"
}

# 按架构对照对应 base 基线组装 update/$arch/（base 只含依赖；应用/前端/差异文件全进 update）
assemble_update() {
  local arch="$1" rid="$2"
  local dst="$UP/$arch"
  local src="$STAGING/publish-$rid"
  mkdir -p "$dst"
  # publish 与 base 各生成一次批量清单，按路径比对跳过 base 中内容相同的依赖文件
  local manifest; manifest="$(md5_manifest "$src")"
  local bman; bman=""
  if [ -d "$BASE_PATH/$arch" ]; then
    bman="$(base_manifest "$arch")"
  else
    log "    [警告] 无法读取 $arch 基线($BASE_PATH/$arch)，全部产物并入 update/$arch"
  fi
  local -A base_hash
  local h p
  while read -r h p; do base_hash["$p"]=$h; done <<< "$bman"
  # 先筛出待复制文件列表，再交给 tar_copy 一次性批量复制（base 已有且内容相同则跳过）
  local -a picks=()
  while read -r h p; do
    skip_waste "$p" && continue
    if [ -n "$bman" ] && [ "${base_hash[$p]+x}" ] && [ "${base_hash[$p]}" = "$h" ]; then
      continue  # base 已有且内容相同（仅依赖层可能命中）
    fi
    picks+=("$p")
  done <<< "$manifest"
  if [ ${#picks[@]} -gt 0 ]; then
    printf '%s\n' "${picks[@]}" | tar_copy "$src" "$dst"
  fi
  log "    $arch: diff 完成（复制 $(find "$dst" -type f | wc -l) 文件）"
  log "    $arch: update 体积 $(du -sh "$dst" | cut -f1)"
}

if [ "$REBUILD" = 1 ]; then
  log "==> 2/5 --rebuild-base：base 仅收依赖层（dll/libe_sqlite3/appsettings.json），应用/前端归 update"
  S2=$(date +%s)
  for entry in $RID_ARCHS; do
    arch="${entry%%:*}"; rid="${entry##*:}"
    rm -rf "$STAGING/cleanbase-$arch"
    mkdir -p "$STAGING/cleanbase-$arch"
    # 筛选依赖层文件清单后 tar 批量复制，避免逐文件 mkdir/cp 启动子进程
    base_files=()
    while read -r h rel; do
      skip_waste "$rel" && continue
      is_base_file "$rel" || continue
      base_files+=("$rel")
    done <<< "$(md5_manifest "$STAGING/publish-$rid")"
    if [ ${#base_files[@]} -gt 0 ]; then
      printf '%s\n' "${base_files[@]}" | tar_copy "$STAGING/publish-$rid" "$STAGING/cleanbase-$arch"
    fi
    log "    $arch 依赖基线大小: $(du -sh "$STAGING/cleanbase-$arch" | cut -f1)"
  done
  log "    [操作] 经 ssh 覆盖 NAS base（旧 base 备份为 base.bak）"
  ssh "$NAS_HOST" "cd $NAS_APP_PATH/Dockerfile && rm -rf base.bak && { [ -d base ] && mv base base.bak || true; } && mkdir -p base/amd64 base/arm64"
  for entry in $RID_ARCHS; do
    arch="${entry%%:*}"
    tar czf - -C "$STAGING/cleanbase-$arch" . | ssh "$NAS_HOST" "tar xzf - -C $NAS_APP_PATH/Dockerfile/base/$arch"
  done
  log "    base 重建完成，size=$(ssh "$NAS_HOST" "du -sh $NAS_APP_PATH/Dockerfile/base | cut -f1")"
  # 用本地 cleanbase 内容刷新 base md5 缓存（即刚上传的新 base），下次增量直接命中缓存
  for entry in $RID_ARCHS; do
    arch="${entry%%:*}"
    md5_manifest "$STAGING/cleanbase-$arch" > "$CACHE_DIR/base-$arch.md5"
    dir_fingerprint "$STAGING/cleanbase-$arch" > "$CACHE_DIR/base-$arch.fp"
  done
  # update = 应用层全量（AutoFeedSiqi.* + wwwroot/ + 非依赖差异），保证 base 重建后镜像完整
  for entry in $RID_ARCHS; do
    arch="${entry%%:*}"; rid="${entry##*:}"
    mkdir -p "$UP/$arch"
    # 筛选应用层文件清单后 tar 批量复制
    app_files=()
    while read -r h rel; do
      skip_waste "$rel" && continue
      is_base_file "$rel" && continue
      app_files+=("$rel")
    done <<< "$(md5_manifest "$STAGING/publish-$rid")"
    if [ ${#app_files[@]} -gt 0 ]; then
      printf '%s\n' "${app_files[@]}" | tar_copy "$STAGING/publish-$rid" "$UP/$arch"
    fi
    log "    $arch 应用层: $(du -sh "$UP/$arch" | cut -f1)"
  done
  step_done "2/5 rebuild-base" "$S2"
else
  log "==> 2/5 组装 update/<arch>/（应用本体 + 前端 + 与依赖 base 有差异的文件）"
  S2=$(date +%s)
  for entry in $RID_ARCHS; do
    arch="${entry%%:*}"; rid="${entry##*:}"
    assemble_update "$arch" "$rid"
  done
  step_done "2/5 组装 update" "$S2"
fi

log "==> 3/5 打包 update.tar.gz"
S3=$(date +%s)
tar czf "$STAGING/update.tar.gz" -C "$STAGING" update
step_done "3/5 打包" "$S3"

log "==> 4/5 scp 上传 NAS（update.tar.gz + nas-deploy.sh + Dockerfile）"
S4=$(date +%s)
scp "$STAGING/update.tar.gz" "$NAS_HOST:$NAS_APP_PATH/Dockerfile/"
scp scripts/nas-deploy.sh scripts/Dockerfile "$NAS_HOST:$NAS_APP_PATH/Dockerfile/"
step_done "4/5 scp 上传" "$S4"

log "==> 5/5 远程构建 + 推送 + 重启"
S5=$(date +%s)
ssh "$NAS_HOST" "bash $NAS_APP_PATH/Dockerfile/nas-deploy.sh $VER"
step_done "5/5 远程构建" "$S5"

log "部署完成: $VER（总用时 $(( $(date +%s) - START_TS ))s）"
