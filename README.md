# AutoFeedSiqi

思齐 PT 站自动发种工具：Excel 导入 → PDF 截图 → 图床上传 → NFO/制种 → 思齐发布。

.NET 9 Blazor Server + BootstrapBlazor UI + MVC 控制器，EF Core + SQLite。

## 运行

```bash
dotnet run --project AutoFeedSiqi
```

Development 默认 http://localhost:5013。`data/feedsiqi.db` 相对进程工作目录解析。

## 部署

一键发布到 NAS（宿主 5021 → 容器 8080）：

```bash
bash scripts/deploy-nas.sh --yes
```

不传版本=自动发布（算版本+打 tag+推送+部署）；传已存在版本=仅部署。详见 `AGENTS.md`「部署与运行环境」节。

## 旧库升级

手工建的历史库首次启动若报"表已存在"，执行后重启：

```sql
INSERT INTO __EFMigrationsHistory (migration_id, product_version) VALUES ('<迁移ID>', '9.0.9');
```

迁移 ID 查 `AutoFeedSiqi/Migrations/` 目录名。
