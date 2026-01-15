using AutoFeedSiqi.Components.Layout;
using AutoFeedSiqi.Models;
using AutoFeedSiqi.Models.CommonComponents;
using AutoFeedSiqi.Models.CommonComponents.Model;
using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Serilog;
using System.Diagnostics.CodeAnalysis;

namespace AutoFeedSiqi.Components.Pages
{
    public partial class FeedHistory
    {
        public static List<Feed> Feeds { get; set; } = [];
        public static Feed Feed { get; set; } = new();
        //public static Configuration Configuration { get; set; } = new();

        private bool IsAutoRefresh { get; set; }
        private void ToggleAuto() => IsAutoRefresh = !IsAutoRefresh;


        private static IEnumerable<int> PageItemsSource => [5, 10, 20, 40, 80, 100];

        private static readonly long MaxFileLength = 5 * 1024 * 1024;
        private CancellationTokenSource? _token;

        [Inject]
        [NotNull]
        private MessageService? MessageService { get; set; }

        [NotNull]
        private Message? Message { get; set; }

        protected override async Task OnInitializedAsync()
        {
            using var context = DatabaseUtil.PooledDbContextFactory.CreateDbContext();
            Feeds = [.. context.Feeds];
            Feeds.Reverse();
        }

        private Task<Feed> OnAddAsync() => Task.FromResult(new Feed() { Id = GenerateId() });

        private uint GenerateId()
        {
            var id = (uint)Feeds.Count;
            while (Feeds.Any(i => i.Id == id))
            {
                id++;
            }
            return id;
        }

        private Task<bool> OnDeleteAsync(IEnumerable<Feed> items)
        {
            items.ToList().ForEach(i => Feeds.Remove(i));
            using var context = DatabaseUtil.PooledDbContextFactory.CreateDbContext();
            context.Feeds.RemoveRange(items);
            context.SaveChanges();
            //删除对应的自动目录
            foreach (var feed in items)
            {
                var autoData = Path.Combine(MainLayout.Configuration.AutoDataPath, $"{feed.Title} {{seedid-{feed.Id}}}");
                if (Directory.Exists(autoData))
                {
                    Directory.Delete(autoData, true);
                }
            }
            return Task.FromResult(true);
        }

        private Task<QueryData<Feed>> OnQueryAsync(QueryPageOptions options)
        {
            IEnumerable<Feed> items = Feeds;

            // 过滤
            var isFiltered = false;
            if (!string.IsNullOrEmpty(options.SearchText))
            {
                // 使用 Linq 处理
                items = items.Where(i =>
                    (i.Id.ToString().Contains(options.SearchText, StringComparison.OrdinalIgnoreCase))
                 || (i.SiqiId.ToString().Contains(options.SearchText, StringComparison.OrdinalIgnoreCase))
                 || (i.Title.Contains(options.SearchText, StringComparison.OrdinalIgnoreCase))
                 || (i.Isbn.Contains(options.SearchText, StringComparison.OrdinalIgnoreCase))
                 || (i.Authors.Contains(options.SearchText, StringComparison.OrdinalIgnoreCase))
                 || (i.Press.Contains(options.SearchText, StringComparison.OrdinalIgnoreCase))
                 || (i.Date.ToString().Contains(options.SearchText, StringComparison.OrdinalIgnoreCase))
                 || (i.Summary.Contains(options.SearchText, StringComparison.OrdinalIgnoreCase))
                 || (i.DoubanId.ToString().Contains(options.SearchText, StringComparison.OrdinalIgnoreCase))
                 || (i.Type.ToDisplayName().Contains(options.SearchText, StringComparison.OrdinalIgnoreCase))
                 || (i.Sort.ToDisplayName().Contains(options.SearchText, StringComparison.OrdinalIgnoreCase))
                 || (i.Source.ToDisplayName().Contains(options.SearchText, StringComparison.OrdinalIgnoreCase))
                 || (i.Condition.ToDisplayName().Contains(options.SearchText, StringComparison.OrdinalIgnoreCase))
                 || (i.Color.ToDisplayName().Contains(options.SearchText, StringComparison.OrdinalIgnoreCase))
                 || (i.Extension.ToDisplayName().Contains(options.SearchText, StringComparison.OrdinalIgnoreCase))
                 || (i.Language.ToDisplayName().Contains(options.SearchText, StringComparison.OrdinalIgnoreCase))
                 || (i.Quality.ToDisplayName().Contains(options.SearchText, StringComparison.OrdinalIgnoreCase))
                 || (i.Team.ToDisplayName().Contains(options.SearchText, StringComparison.OrdinalIgnoreCase))
                 || (i.Path.Contains(options.SearchText, StringComparison.OrdinalIgnoreCase))
                 || (i.Task.ToDisplayName().Contains(options.SearchText, StringComparison.OrdinalIgnoreCase))
                 || (i.CreateAt.ToString().Contains(options.SearchText, StringComparison.OrdinalIgnoreCase))
                 || (i.LastUpdatedAt.ToString().Contains(options.SearchText, StringComparison.OrdinalIgnoreCase))
                 ).ToList();
                isFiltered = true;
            }

            var isSorted = false;

            //处理高级排序
            if (options.AdvancedSortList.Count != 0)
            {
                items = items.Sort(options.AdvancedSortList);
                isSorted = true;
            }

            // 使用 Sort 扩展排序方法进行排序
            if (!string.IsNullOrEmpty(options.SortName))
            {
                items = items.Sort(options.SortName, options.SortOrder);
                isSorted = true;
            }


            // 设置记录总数
            var total = items.Count();

            // 内存分页
            items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            return Task.FromResult(new QueryData<Feed>()
            {
                Items = items,
                TotalCount = total,
                IsSorted = isSorted,
                IsFiltered = isFiltered,
                IsSearch = isFiltered,
                IsAdvanceSearch = false
            });
        }

        private async Task<bool> OnSaveAsync(Feed feed, ItemChangedType changedType)
        {
            var message = string.Empty;
            var title = feed.Title;
            var path = feed.Path;
            var isHasError = false;
            if (string.IsNullOrEmpty(path))
            {
                message += $"“{title}”的文件夹路径不能为空\r\n";
            }
            //添加时，判断path是否已存在，或者修改时，判断path是否已存在且不是当前记录的path 
            if ((changedType == ItemChangedType.Add && Feeds.Any(i => i.Path == path)) ||
                (changedType == ItemChangedType.Update && Feeds.Any(i => i.Path == path && i.Id != feed.Id)))
            {
                Log.Error($"数据库已存在相同文件夹路径的记录！路径：{{{nameof(path)}}}", path);
                message += $"数据库已存在相同文件夹路径的记录！路径：{path}\r\n";
            }
            //判断path是否是一个合格的路径
            if (!Path.IsPathFullyQualified(path))
            {
                message += $"“{title}”的文件夹路径“{path}”不合法\r\n";
            }
            //判断path路径是否存在
            if (!Directory.Exists(path))
            {
                message += $"“{title}”的文件夹路径“{path}”不存在\r\n";
            }
            //判断path路径下是否存在pdf或者cbz、cbr文件
            try
            {
                if (feed.Extension == Extension.PDF)
                {
                    if (Directory.GetFiles(path, "*.pdf", SearchOption.AllDirectories).Length == 0)
                    {
                        message += $"“{title}”的文件夹路径“{path}”下不存在pdf文件\r\n";
                    }

                }
                else if (feed.Extension == Extension.CBZOrCBR)
                {
                    if (Directory.GetFiles(path, "*.cbz", SearchOption.AllDirectories).Length == 0
                        && Directory.GetFiles(path, "*.cbr", SearchOption.AllDirectories).Length == 0
                        && Directory.GetFiles(path, "*.zip", SearchOption.AllDirectories).Length == 0
                        && Directory.GetFiles(path, "*.rar", SearchOption.AllDirectories).Length == 0
                        )
                    {
                        message += $"“{title}”的文件夹路径“{path}”下不存在cbz、zip或者cbr、rar文件\r\n";
                    }
                }
            }
            catch
            {
                message += $"无法访问“{title}”的文件夹路径“{path}”，请检查权限\r\n";
            }

            if (!string.IsNullOrEmpty(message))
            {
                isHasError = true;
            }
            else
            {
                using var context = DatabaseUtil.PooledDbContextFactory.CreateDbContext();
                if (changedType == ItemChangedType.Add)
                {
                    feed.Id = Feeds.Max(i => i.Id) + 1;
                    Feeds.Reverse();
                    Feeds.Add(feed);
                    Feeds.Reverse();
                    context.Add(feed);
                }
                else
                {
                    var oldItem = Feeds.FirstOrDefault(i => i.Id == feed.Id);
                    if (oldItem != null)
                    {
                        oldItem.Title = feed.Title;
                        oldItem.Isbn = feed.Isbn;
                        oldItem.Authors = feed.Authors;
                        oldItem.Press = feed.Press;
                        oldItem.Date = feed.Date;
                        oldItem.Summary = feed.Summary;
                        oldItem.DoubanId = feed.DoubanId;
                        oldItem.Type = feed.Type;
                        oldItem.Sort = feed.Sort;
                        oldItem.Source = feed.Source;
                        oldItem.Condition = feed.Condition;
                        oldItem.Color = feed.Color;
                        oldItem.Extension = feed.Extension;
                        oldItem.Language = feed.Language;
                        oldItem.Quality = feed.Quality;
                        oldItem.Team = MainLayout.Configuration.SiqiUploaderToken == "d6k55a0lvancydsnnrgumxf7u8n56g52" ? Team.SQB : Team.Other;
                        oldItem.Path = feed.Path;
                        oldItem.LastUpdatedAt = DateTime.Now;
                    }
                    context.Update(feed);
                }
                context.SaveChanges();
            }

            if (isHasError)
            {
                await MessageService.Show(new()
                {
                    IsAutoHide = false,
                    Content = message,
                    Color = BootstrapBlazor.Components.Color.Danger,
                    Icon = "fa-solid fa-circle-info",
                    ShowDismiss = true,
                    OnDismiss = () =>
                    {
                        return Task.CompletedTask;
                    },
                    ShowBar = true,
                    ShowShadow = true,
                    ShowMode = MessageShowMode.Multiple,
                }, Message);
                return false;
            }
            else
            {
                return true;
            }
        }

        private async Task OnClickToUpload(UploadFile file)
        {
            var ext = Path.GetExtension(file.OriginFileName ?? string.Empty).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xls")
            {
                file.Code = 1;
                file.Error = "UploadsError";
                //await ToastService.Error("上传Excel", "文件不是Excel格式");
                await MessageService.Show(new()
                {
                    IsAutoHide = false,
                    Content = "上传文件不是Excel格式",
                    Color = BootstrapBlazor.Components.Color.Warning,
                    Icon = "fa-solid fa-circle-info",
                    ShowDismiss = true,
                    OnDismiss = () =>
                    {
                        return Task.CompletedTask;
                    },
                    ShowBar = true,
                    ShowShadow = true,
                    ShowMode = MessageShowMode.Multiple,
                }, Message);
            }
            else
            {
                await SaveToFile(file);
            }
        }

        private async Task OnClickToUploadPreviews(UploadFile file)
        {
            var ext = Path.GetExtension(file.OriginFileName ?? string.Empty).ToLowerInvariant();
            if (ext != ".png" && ext != ".jpeg" && ext != ".jpg")
            {
                file.Code = 1;
                file.Error = "UploadsError";
                //await ToastService.Error("上传Excel", "文件不是Excel格式");
                await MessageService.Show(new()
                {
                    IsAutoHide = false,
                    Content = "上传文件不是png|jpeg|jpg格式",
                    Color = BootstrapBlazor.Components.Color.Warning,
                    Icon = "fa-solid fa-circle-info",
                    ShowDismiss = true,
                    OnDismiss = () =>
                    {
                        return Task.CompletedTask;
                    },
                    ShowBar = true,
                    ShowShadow = true,
                    ShowMode = MessageShowMode.Multiple,
                }, Message);
            }
            else
            {
                await SaveToPreviewsFile(file);
            }
        }

        private async Task SaveToPreviewsFile(UploadFile file)
        {
            //获取程序根目录
            var webRootPath = Directory.GetCurrentDirectory();
            var uploaderFolder = Path.Combine(webRootPath, "previews");
            if (!Directory.Exists(uploaderFolder))
            {
                Directory.CreateDirectory(uploaderFolder);
            }
            file.FileName = $"{Path.GetFileNameWithoutExtension(file.OriginFileName)}-{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(file.OriginFileName)}";
            var fileName = Path.Combine(uploaderFolder, file.FileName);

            _token ??= new CancellationTokenSource();
            try
            {
                var ret = await file.SaveToFileAsync(fileName, MaxFileLength, token: _token.Token);

                if (!ret)
                {
                    var errorMessage = $"UploadsSaveFileError {file.OriginFileName}";
                    file.Code = 1;
                    file.Error = errorMessage;
                    await ToastService.Error("UploadFile", errorMessage);
                }
                else
                {
                    await MessageService.Show(new()
                    {
                        IsAutoHide = false,
                        Content = $"上传文件“{file.OriginFileName}”成功！",
                        Color = BootstrapBlazor.Components.Color.Success,
                        Icon = "fa-solid fa-circle-info",
                        ShowDismiss = true,
                        OnDismiss = () =>
                        {
                            return Task.CompletedTask;
                        },
                        ShowBar = true,
                        ShowShadow = true,
                        ShowMode = MessageShowMode.Multiple,
                    }, Message);
                }
            }
            catch (OperationCanceledException)
            {

            }
        }

        private async Task SaveToFile(UploadFile file)
        {
            //获取程序根目录
            var webRootPath = Directory.GetCurrentDirectory();
            var uploaderFolder = Path.Combine(webRootPath, "data", "excel");
            file.FileName = $"{Path.GetFileNameWithoutExtension(file.OriginFileName)}-{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(file.OriginFileName)}";
            var fileName = Path.Combine(uploaderFolder, file.FileName);

            _token ??= new CancellationTokenSource();
            try
            {
                var ret = await file.SaveToFileAsync(fileName, MaxFileLength, token: _token.Token);

                if (!ret)
                {
                    var errorMessage = $"UploadsSaveFileError {file.OriginFileName}";
                    file.Code = 1;
                    file.Error = errorMessage;
                    await ToastService.Error("UploadFile", errorMessage);
                }
                else
                {
                    using var context = DatabaseUtil.PooledDbContextFactory.CreateDbContext();
                    var message = await FeedPtSiqi.UploadExcelToDatabaseAsync(fileName, context);
                    if (string.IsNullOrEmpty(message))
                    {
                        await MessageService.Show(new()
                        {
                            IsAutoHide = false,
                            Content = "上传文件记录已成功添加到数据库！\r\n请重新刷新页面！",
                            Color = BootstrapBlazor.Components.Color.Success,
                            Icon = "fa-solid fa-circle-info",
                            ShowDismiss = true,
                            OnDismiss = () =>
                            {
                                return Task.CompletedTask;
                            },
                            ShowBar = true,
                            ShowShadow = true,
                            ShowMode = MessageShowMode.Multiple,
                        }, Message);
                    }
                    else
                    {
                        await MessageService.Show(new()
                        {
                            IsAutoHide = false,
                            Content = message,
                            Color = BootstrapBlazor.Components.Color.Success,
                            Icon = "fa-solid fa-circle-info",
                            ShowDismiss = true,
                            OnDismiss = () =>
                            {
                                return Task.CompletedTask;
                            },
                            ShowBar = true,
                            ShowShadow = true,
                            ShowMode = MessageShowMode.Multiple,
                        }, Message);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public void Dispose()
        {
            _token?.Cancel();
            _token?.Dispose();
            _token = null;
            GC.SuppressFinalize(this);
        }

        private static string GetSiqiIdUrl(uint siqiId)
        {
            return $"{MainLayout.Configuration.SiqiHost}/details.php?id={siqiId}";
        }

        private static string GetPreviewUrl(string previewUrl)
        {
            //获得域名左半部分
            var host = new Uri(MainLayout.Configuration.IskyHost).GetLeftPart(UriPartial.Authority);
            var firstPreviewUrl = previewUrl.Split(";").First();
            return !string.IsNullOrEmpty(firstPreviewUrl) ? $@"{host}{firstPreviewUrl}" : string.Empty;
        }

        private static double GetPercentage(FeedTask task)
        {
            return task switch
            {
                FeedTask.None => 1,
                FeedTask.GetPreviews => 20,
                FeedTask.GetPreviewsUrls => 40,
                FeedTask.CreateNfo => 50,
                FeedTask.CreateSeed => 60,
                FeedTask.Feed => 70,
                FeedTask.Seed => 90,
                _ => 0,
            };
        }

        [Inject]
        [NotNull]
        private ToastService? ToastService { get; set; }
        private async Task FeedPtSiqiOnClick(IEnumerable<Feed> feeds)
        {
            await MessageService.Show(new()
            {
                IsAutoHide = true,
                Content = "正在发种中。。。请开启自动刷新，查看任务状态",
                Color = BootstrapBlazor.Components.Color.Info,
                Icon = "fa-solid fa-circle-info",
                ShowDismiss = true,
                OnDismiss = () =>
                {
                    return Task.CompletedTask;
                },
                ShowBar = true,
                ShowShadow = true,
                ShowMode = MessageShowMode.Multiple,
            }, Message);

            var message = await FeedPtSiqi.AutoSeedSiqiAsync([.. feeds]);
            if (message != "发种已完成")
            {
                await MessageService.Show(new()
                {
                    IsAutoHide = false,
                    Content = message,
                    Color = BootstrapBlazor.Components.Color.Danger,
                    Icon = "fa-solid fa-circle-info",
                    ShowDismiss = true,
                    OnDismiss = () =>
                    {
                        return Task.CompletedTask;
                    },
                    ShowBar = true,
                    ShowShadow = true,
                    ShowMode = MessageShowMode.Multiple,
                }, Message);
            }
        }

        private async Task OnRowButtonClick(Feed item, string text)
        {
            var cate = ToastCategory.Success;
            var title = $"{text} {item.Title}";
            var content = "通过不同的函数区分按钮处理逻辑，参数 Item 为当前行数据";
            //await ToastService.Show(new ToastOption() { Category = cate, Title = title, Content = content });
            //await TableRows.QueryAsync();
        }
    }
}