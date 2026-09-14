# ENVIRONMENT.md

本项目的 NAS 部署与数据环境清单。**本机通用环境（工具链/代理/多 SDK/参考源码镜像库/playwright 便携版/G 盘挂载拓扑/NAS 通用信息）已在全局 `~/.config/opencode/learnings/ENVIRONMENT.md`（单一来源），先看那边**。本项目部署手册细节如下，基础设施变更时同步更新本文件。

## 部署与运行（NAS）

- ssh 别名 `nas`，Git Bash 直接 `ssh nas` 即连；实际连接地址/用户/凭证见全局 ENVIRONMENT.md「NAS」节。应用部署目录：`/vol1/1000/docker/autofeedsiqi/`
  （内含 Dockerfile（scp 自仓库 `scripts/Dockerfile`）、base/{amd64,arm64}、update/{amd64,arm64}、nas-deploy.sh，均与 scripts/deploy-nas.sh 协作）
- **发布入口**：`bash scripts/deploy-nas.sh vX.Y.Z [--rebuild-base]` 一键发布到 NAS（宿主 5021 → 容器 8080）；Dockerfile 随部署从仓库 `scripts/Dockerfile` scp 同步 NAS（勿手工改 NAS 那份）；输出 tee 落盘 `.deploy-logs/`（gitignore），`--rebuild-base` 后自动刷 `.deploy-cache/`（gitignore），怀疑 base 变化未重建时删该目录强制重算
- 容器：`autofeedsiqi`，宿主端口 **5021** → 容器内 8080
- 镜像分层：按架构双层——`base/{amd64,arm64}` **仅依赖层**（*.dll 依赖程序集 + libe_sqlite3.so + appsettings.json）+ `update/{amd64,arm64}` **应用层**（AutoFeedSiqi.* 应用本体 + wwwroot/ 前端资源 + 与 base 差异文件），Dockerfile 以 `ARG TARGETARCH` + `COPY base/${TARGETARCH}` + `COPY update/${TARGETARCH}` 选择；**base 不随常规发布变化**（依赖 dll 稳定 → Hub 层 digest 复用秒传），仅依赖升级/清理残留/首次部署才 `--rebuild-base`（重建时旧 base 备份为 `base.bak`，首部无旧 base 时跳过备份；脚本对 base/update 归属按 `is_base_file`/`skip_waste` 分类）
- 组装 update 易错点：`AutoFeedSiqi*.dll` 永远归 update——**勿用 `AutoFeedSiqi.*` 通配过滤**（会把应用本体一并扔掉）
- 部署脚本固定排除（永不进镜像）：apphost `AutoFeedSiqi`、`AutoFeedSiqi.pdb/.exe`、`web.config`、`appsettings.Development.json`。**`*.br/*.gz` 不可剔除**——.NET 9 MapStaticAssets 发布产物含预压缩资产，缺失则浏览器拿到 200+空 body、前端 JS/CSS 全失效
- NAS 侧代理、Docker Hub 发布链路（multi-arch manifest / 凭证 / push 约束）见全局 ENVIRONMENT.md「NAS」节

## 容器 env（autofeedsiqi.env，单级键）

- `TZ=Asia/Shanghai`
- `Operating_System=Linux`（`ConfigureProvider.IsLinuxOperatingSystem` 开关，当前无分支使用，预留）
- 注意与 AutoFeedEmby 的双下划线节式不同；Seq 在本项目未接线（读了不用），无需配置

## G 盘（clouddrive2 直连 NAS 文件）

- **部署增量 diff 基准**：`G:/fnOS/docker/autofeedsiqi/Dockerfile/base/{amd64,arm64}`（分架构对照；缺失某架构目录时该架构产物全量并入 update）
- 教训：向 clouddrive2 传输几千个小文件不稳定；NAS 侧文件传输一律本地打单包 → ssh tar 流式解包

## 数据文件位置

- NAS `autofeedsiqi` 容器真实数据：`G:\fnOS\docker\autofeedsiqi\data`（`feedsiqi.db`；本机可直读）
- NAS `autofeedsiqi` 容器日志：`G:\fnOS\docker\autofeedsiqi\logs`（对应容器内 `/app/logs/`，本机可直读；查问题直接 `rg -n "关键字" "G:/fnOS/docker/autofeedsiqi/logs"`，无需 ssh 进容器）
- **本机 dev 库**：仓库 `AutoFeedSiqi/data/feedsiqi.db`（gitignore），dev 端读第二行配置（`Skip(1)`），生产读首行
- 客户旧库搬迁：`feedsiqi.db` 拷入 NAS `./data` 后重启；DB 内 Windows 路径（`ExcelPath`/`AutoDataPath`）容器内失效，部署后进配置页重设
