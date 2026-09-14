# AGENTS.md

## 会话启动加载（全局 AGENTS 第 0 节的项目侧部分）

进入本项目工作后，在全局知识源之外一并加载：

### 加载项目 Skill（2 个）

1. `project-knowledge` — 项目知识库（架构/部署/learnings 路由）
2. `task-wrapup` — 任务收尾扫尾

### 加载项目文档（2 个）

1. `learnings/ENVIRONMENT.md` — 项目 NAS 部署与数据环境
2. `AGENTS.md` — 项目 AGENTS（本文件）

## 项目概览

思齐 PT 站自动发种工具：Excel 导入 → PDF 截图 → 图床上传 → NFO/制种 → 思齐发布。.NET 9 Blazor Server + BootstrapBlazor UI + MVC 控制器，EF Core + SQLite。slnx 仅主项目 `AutoFeedSiqi`（无测试项目）。版本经 `version.json` 走 nbgv（两段写法）。

## 知识查询入口（先读这里）

参考型知识已下沉到 `project-knowledge` skill，**遇到这些问题先加载它**：

- 架构/代码定位、发种流程、图床 token 机制 → skill「架构地图」节
- 部署发布（deploy-nas.sh/镜像分层/多架构 push）、环境明细（NAS/代理/数据路径）→ `learnings/ENVIRONMENT.md`（环境与部署**单一来源**；发布入口命令见下方「部署与运行环境」节）
- 经验教训/未决问题 → `learnings/items/` 原子条目库（LRN=经验 TD=任务，按 ID 或关键词 rg 定位；路由细节见 skill「知识路由」节）

## 命令

- 运行：`dotnet run --project AutoFeedSiqi`（Development 默认 http://localhost:5013）；注意进程 cwd 即库路径基准，`data/feedsiqi.db` 相对 cwd 解析
- 构建：`dotnet build`（**勿依赖 `dotnet build -t:Compile` 做验证**——增量可能跳过改动文件，实踩：Program.cs 改动未进 dll；验证一律完整 build）
- 格式化：`dotnet format AutoFeedSiqi.slnx --include <本次改动.cs> --no-restore`（SDK 内置；**只对本次改动文件跑，禁全量**）；提交前用 `--verify-no-changes` 复核
- EF 工具：全局 `dotnet-ef` 9.0.9，与项目 EF Core 9.0.9 匹配；迁移命名大驼峰（如 `Init`）；迁移历史表 snake_case（列名 `migration_id`）；设计时工厂 `AutoFeedSiqiDbContextFactory` 已就绪

## 代码规范

- **中文注释**：新增逻辑配中文注释，解释"为什么"
- **Edit 工具使用纪律**：见全局 AGENTS.md 第 11 节——oldString 优先单行唯一不含换行符、多行须逐字复制且对齐 CRLF、禁带行号前缀、重复片段用 replaceAll；改完补回文件终换行（Edit 会吞掉，`git diff` 出现 `No newline` 即需补）
- **行尾规则**（`.gitattributes`）：`*.cs/*.razor/*.json/*.csproj/*.slnx` 强制 CRLF；`*.sh` 与 `Dockerfile` 强制 LF（NAS 端执行，CRLF 会坏 shebang）
- **EF 上下文**：静态 `DatabaseUtil.PooledDbContextFactory`，每次 `using var context = ...CreateDbContext()` 短命使用；`Update()` 只对分离/新对象必要；时间戳无全局 override，改实体时手动维护 `LastUpdatedAt`
- **配置缓存**：`MainLayout.Configuration` 为静态配置缓存，发种流程经 `FeedPtSiqi.Configuration` 引用；保存配置页（`NoRenderShowEditDialog/OnEditAsync`）后内存与 DB 保持一致，改保存逻辑时注意同步两者
- **图床 token 机制**：`IskyToken` 为空时才用 `IskyEmail+IskyPassword` 重登；换号保存自动清空旧 token（`OnEditAsync` 按 `Id` 查旧值对比）；上传遇 401 自动重登重试一次（`Upload/IsIskyUnauthorized`，仅一次防循环）；新建/重登 token 必须同步写库 + 同步内存

## 数据库与升级

- 启动 `Program.cs` 执行 `Database.Migrate()`（无库自动建库建表）+ 空表种子一行默认 `Configuration`
- **旧库升级预案**：手工建的历史库无 `__EFMigrationsHistory` 行时，首次启动报"表已存在"，执行 `INSERT INTO __EFMigrationsHistory (migration_id, product_version) VALUES ('<迁移ID>', '9.0.9');` 后重启（迁移 ID 查 `Migrations/` 目录名）
- `data/` 为运行时数据（gitignore），禁提交；dev 端第二行配置（`Skip(1)`）与生产首行区分，改保存逻辑勿破坏

## 任务收尾自动扫尾（task-wrapup）

- **每次任务 git 收尾前必须执行扫尾**（`task-wrapup` skill）：扫描本轮会话，未解决问题新建 `TD-*` 条目到 `learnings/items/`、有价值信息沉淀为 `LRN-*` 条目，再走 git 提交流程；完整判断标准与写入格式见 `.opencode/skills/task-wrapup/SKILL.md`

## Git 提交与版本管理

- 提交格式（Conventional Commits）与任务收尾 git 流程遵循全局 AGENTS.md 第 9 节（先拟定中文 commit message 供审核、通过后再 add/commit），不在项目内重复维护
- **发布=打 tag，tag 落远端才算备份完成**：本仓库发布由 `scripts/deploy-nas.sh` 统一负责算版本/打 tag/push/push tag/核对（自动模式内置，调用即授权），无需外部再查未推送 tag

### 版本号与打 tag

- 版本号由仓库根 `version.json` 的语义主段驱动，**patch 段自动 = git 提交高度**（每提交 +1；纯 `.md` 文档提交经 `pathFilters` 排除不计数）；**升版号前必须先查询 `version.json` 实际版号，勿凭记忆写死**
- **升 minor 一律 `nbgv set-version X.Y`（两段），勿写三段 `X.Y.0`**（高度落第四段、后续版本不可区分）；**升 minor 先提交 version.json 再取 `SimpleVersion` 打 tag**（未提交拿到假想高度 0 的 X.Y.0，tag 脱节）；勿用 `nbgv tag`。机理见 `learnings/items/` LRN-20260815-006 / LRN-20260820-004（自 Emby 库移植）
- **git 收尾必查版本号：`feat` 自动升 minor**——直接 `nbgv set-version X.Y` 随功能提交同 commit；**升版当天当版打 tag + 部署**
- **pathFilters 维护**：文档提交不计高度靠 `pathFilters`（目录前缀 + 根级精确文件名）。新增**根级 `.md`** 时须补一条；文档尽量放 `learnings/` 子树

## 部署与运行环境（核心约束；手册见 `learnings/ENVIRONMENT.md`）

- **发布入口**：`bash scripts/deploy-nas.sh` 一键发布到 NAS（宿主 5021 → 容器 8080，ssh 别名 `nas`）；**不传版本=自动发布**（算 SimpleVersion + 打 tag + push + push tag + 核对远端后部署），**传已存在版本=仅部署**；`--yes` 跳过确认、`--rebuild-base` 重建 base；**运行必须前台单次调用、不加管道、不后台化**，规范 `bash scripts/deploy-nas.sh --yes`（超时 ≥600000ms）；中断即幂等重跑显式版本；脚本内置 tee 落盘 `.deploy-logs/`
- **⚠️ 部署不断开铁律**：调用发出后**全程挂起等到「部署完成: vX.Y.Z」才结束本回合**；中途不得发其他 bash 调用、不得提前返回——bash 调用一返回就杀整个进程组，部署必掉。输出停滞只管等（5/5 远程构建常 2 分钟+）
- 客户旧库搬迁：`feedsiqi.db` 拷入 NAS `./data` 后重启；DB 内 Windows 路径（`ExcelPath`/`AutoDataPath`）容器内失效，部署后进配置页重设
