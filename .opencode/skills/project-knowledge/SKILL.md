---
name: project-knowledge
description: AutoFeedSiqi 项目知识快速入口。Use when 需要查本项目架构/代码地图、发种流程、图床 token 机制、部署发布细节（deploy-nas.sh/镜像分层/多架构 push/容器数据路径）、或需要读 learnings（ENVIRONMENT/items 条目库）的项目经验、环境明细、未决任务时加载。不要用于任务收尾（用 task-wrapup）或通用学习记录（用 self-improvement）。
---

# 项目知识库（project-knowledge）

AGENTS.md 只保留铁律与高频命令，本 skill 收纳「参考型知识」：架构地图与 `learnings/` 条目库的读取路由（部署手册、环境明细统一在 `learnings/ENVIRONMENT.md`，本 skill 不重复维护）。遇到本文件列出的场景先加载本 skill，再按需用 Read / Grep 取细节。

## 知识路由（learnings）

- **`learnings/ENVIRONMENT.md`**：本项目 NAS 部署与数据环境清单——发布入口、容器端口 5021→8080、镜像分层、组装 update 易错点、G 盘部署基准、env 单级键写法、数据/日志位置、dev 双行配置说明。**本机通用环境在全局 `~/.config/opencode/learnings/ENVIRONMENT.md`（单一来源），本文件只留项目部署细节并指向它**
- **`learnings/items/`**：原子化条目库。一文件一条目：LRN=经验教训、TD=任务/观察项；frontmatter 含 id/type/category/status/priority/tags/aliases。定位：按 ID 直接 Read `items/LRN-YYYYMMDD-XXX.md`，或 `rg "关键词" learnings/items/`；活跃任务用 `rg -l "^status: \"(open|watching)\"" learnings/items/`。条目间关联是正文尾部 `## 相关条目` 小节的 wiki-link；AGENTS 中「机理见 LRN-xxx」的溯源都查这里（本库条目多自 Emby 库移植，溯源时注明出处）
- 引用频率高的 LRN：LRN-20260914-004（部署前台铁律）、LRN-20260914-005（nbgv 两段写法）、LRN-20260914-006（先提交再打 tag）

**写入职责分工**：新增经验/报错/功能诉求 → `self-improvement` skill（全局）；任务收尾扫尾 → `task-wrapup` skill（项目）。本 skill 只做读取路由，不写内容，避免双份维护。

## 架构地图

- `Program.cs`：入口。Razor Components + MVC 控制器；`builder.ConfigureLog()`（Serilog）；`app.Build()` 后建 `data` 目录、短命 context 执行 `Database.Migrate()`（无库建库建表）+ 空表种子一行默认 `Configuration`
- `Models/FeedPtSiqi.cs`：发种核心（静态类，经 `MainLayout.Configuration` 取配置）。`UploadExcelToDatabaseAsync`（Excel 入库）、`GetPDFPreviewsUrlsAsync`（PDF 截图→图床上传，token 为空重登/401 自动重登）、`CreateNfoAsync`、`SeedSiqiBackAsync`（发种回写）
- `Controllers/FeedSiqiController.cs`：发种流程 Webhook 端点
- `Components/Layout/MainLayout.razor.cs`：静态 `Configuration` 配置缓存（生产首行/dev 第二行）；`NoRenderShowEditDialog/OnEditAsync` 配置保存（换号自动清 token）
- `Models/CommonComponents/`：`DatabaseUtil`（静态 `PooledDbContextFactory`，连接串 `data/feedsiqi.db` + snake_case）、`ConfigureProvider`（Serilog 配置 + `Operating_System` 开关）、`Model/Configuration.cs`（发种全部配置项）、`EntityTypeConfiguration/`（DbContext + 设计时工厂 + 实体配置）
- 迁移：`AutoFeedSiqi/Migrations/` 提交版本控制（含初始 `Init`）；旧手工库升级预案见 AGENTS.md「数据库与升级」节；`data/` 为运行时数据（gitignore）
- 图床：Lsky（`IskyHost/IskyEmail/IskyPassword/IskyToken`，token 机制见 AGENTS.md「代码规范」节）；PDF 截图经 `PDFtoImage`（SkiaSharp.NativeAssets.Linux.NoDependencies，容器免装系统库）

## 部署与运行环境手册

- 部署发布与环境明细统一见 `learnings/ENVIRONMENT.md`（**单一来源**，勿在别处另存副本）；发布入口命令保留在 AGENTS「部署与运行环境」节
