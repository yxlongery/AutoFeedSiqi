using AutoFeedSiqi.Components.Layout;
using AutoFeedSiqi.Components.Pages;
using AutoFeedSiqi.Models.CommonComponents;
using AutoFeedSiqi.Models.CommonComponents.EntityTypeConfiguration;
using AutoFeedSiqi.Models.CommonComponents.Model;
using BootstrapBlazor.Components;
using ENusbaum.Torrent;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using RestSharp;
using Serilog;
using System.Data;
using System.Dynamic;
using System.Net;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;

namespace AutoFeedSiqi.Models
{
    public static class FeedPtSiqi
    {
        //需要提前初始化Configuration
        public static Configuration Configuration { get; set; } = MainLayout.Configuration;

        /// <summary>
        /// 上传Excel内容到数据库
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<string> UploadExcelToDatabaseAsync(string excelPath, AutoFeedSiqiDbContext context)
        {
            var message = string.Empty;

            //GetExcelContent
            using ExcelPackage package = new ExcelPackage(new FileInfo(excelPath));
            ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
            var lastRow = worksheet?.LastValueCell.End.Row;
            if (lastRow == 2)
            {
                //说明没有表格是空的，没有数据
                message += $"“{excelPath}”文件中没有数据可供导入，增加数据后重试\r\n";
                //throw new Exception($"“{excelPath}”文件中没有数据可供导入，增加数据后重试");
            }

            var isHasError = false;

            //写入到数据库
            var feeds = worksheet?.Cells[$"A1:P{lastRow}"].ToCollectionWithMappings<Feed>(row =>
            {
                var path = row.GetValue<string>("文件夹");
                var title = "无标题";
                try
                {
                    title = string.IsNullOrEmpty(row.GetValue<string>("标题")) ? Path.GetFileNameWithoutExtension(path) : row.GetValue<string>("标题");
                }
                catch (Exception)
                {
                    title = "无标题";
                }
                var date = row.GetValue<string>("出版年");
                var type = row.GetValue<string>("类型");
                var sort = row.GetValue<string>("类别");
                var source = row.GetValue<string>("来源");
                var condition = row.GetValue<string>("状态");
                var color = row.GetValue<string>("颜色");
                var extension = row.GetValue<string>("格式");
                var language = row.GetValue<string>("语言");
                var quality = row.GetValue<string>("品质");

                if (!string.IsNullOrEmpty(title) && string.IsNullOrEmpty(path))
                {
                    message += $"“{title}”没有文件夹路径\r\n";
                }
                //title不能为空
                //if (string.IsNullOrEmpty(title))
                //{
                //    message += $"存在一行没有标题，请删除该行，或者完善该行数据\r\n";
                //}
                //path不能为空
                if (string.IsNullOrEmpty(path))
                {
                    message += $"存在一行没有文件夹路径，请删除该行，或者完善该行数据\r\n";
                }
                //检测data是否是有效日期
                if (!string.IsNullOrEmpty(date) && !DateTime.TryParse(date, out var dateValue))
                {
                    message += $"“{title}”的日期“{date}”无效\r\n";
                }
                //类型不能为空
                if (string.IsNullOrEmpty(type))
                {
                    message += $"“{title}”的类型不能为空\r\n";
                }
                //类别不能为空
                if (string.IsNullOrEmpty(sort))
                {
                    message += $"“{title}”的类别不能为空\r\n";
                }
                //来源不能为空
                if (string.IsNullOrEmpty(source))
                {
                    message += $"“{title}”的来源不能为空\r\n";
                }
                //状态不能为空
                if (string.IsNullOrEmpty(condition))
                {
                    message += $"“{title}”的状态不能为空\r\n";
                }
                //颜色不能为空
                if (string.IsNullOrEmpty(color))
                {
                    message += $"“{title}”的颜色不能为空\r\n";
                }
                //格式不能为空
                if (string.IsNullOrEmpty(extension))
                {
                    message += $"“{title}”的格式不能为空\r\n";
                }
                //语言不能为空
                if (string.IsNullOrEmpty(language))
                {
                    message += $"“{title}”的语言不能为空\r\n";
                }
                //品质不能为空
                if (string.IsNullOrEmpty(quality))
                {
                    message += $"“{title}”的品质不能为空\r\n";
                }

                Feed? feed = null;
                if (context.Feeds.Any(p => p.Path == path))
                {
                    Log.Debug($"数据库已存在相同文件夹路径的记录！路径：{{{nameof(path)}}}", path);
                }
                else
                {
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
                        if (Enum.GetValues<CommonComponents.Model.Extension>().FirstOrDefault(p => p.ToDisplayName() == extension) == Extension.PDF)
                        {
                            if (Directory.GetFiles(path, "*.pdf", SearchOption.AllDirectories).Length == 0)
                            {
                                message += $"“{title}”的文件夹路径“{path}”下不存在pdf文件\r\n";
                            }

                        }
                        else if (Enum.GetValues<CommonComponents.Model.Extension>().FirstOrDefault(p => p.ToDisplayName() == extension) == Extension.CBZOrCBR)
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
                        feed = new Feed
                        {
                            Title = title.Replace("'", ""),
                            Isbn = row.GetValue<string>("ISBN")?.Replace("'", "") ?? string.Empty,
                            Authors = row.GetValue<string>("作者")?.Replace("'", "") ?? string.Empty,
                            Press = row.GetValue<string>("出版社")?.Replace("'", "") ?? string.Empty,
                            Date = string.IsNullOrEmpty(date) ? DateTime.UnixEpoch : row.GetValue<DateTime>("出版年"),
                            Summary = row.GetValue<string>("简介")?.Replace("'", "''") ?? string.Empty,
                            DoubanId = row.GetValue<int>("豆瓣ID"),
                            Type = Enum.GetValues<CommonComponents.Model.Type>().FirstOrDefault(p => p.ToDisplayName() == type),
                            Sort = Enum.GetValues<CommonComponents.Model.Sort>().FirstOrDefault(p => p.ToDisplayName() == sort),
                            Source = Enum.GetValues<CommonComponents.Model.Source>().FirstOrDefault(p => p.ToDisplayName() == source),
                            Condition = Enum.GetValues<CommonComponents.Model.Condition>().FirstOrDefault(p => p.ToDisplayName() == condition),
                            Color = Enum.GetValues<CommonComponents.Model.Color>().FirstOrDefault(p => p.ToDisplayName() == color),
                            Extension = Enum.GetValues<CommonComponents.Model.Extension>().FirstOrDefault(p => p.ToDisplayName() == extension),
                            Language = Enum.GetValues<CommonComponents.Model.Language>().FirstOrDefault(p => p.ToDisplayName() == language),
                            Quality = Enum.GetValues<CommonComponents.Model.Quality>().FirstOrDefault(p => p.ToDisplayName() == quality),
                            Team = Configuration.SiqiUploaderToken == "d6k55a0lvancydsnnrgumxf7u8n56g52" ? Team.SQB : Team.Other,
                            Path = path
                        };
                        context.Add(feed);
                    }
                }
                return feed;
            }, options =>
            {
                options.HeaderRow = 0;
                options.DataStartRow = 2;
            });

            //如果feeds的数量为0，说明没有数据需要导入
            if (!isHasError && feeds?.Count > 0)
            {
                await context.SaveChangesAsync();
                Log.Information($"“{{{nameof(excelPath)}}}”文件已导入数据库!", excelPath);
            }
            else
            {
                Log.Information($"“{{{nameof(excelPath)}}}”文件中出现数据错误，请修正后重新导入！", excelPath);
                message += $"“{excelPath}”文件中出现数据错误，请修正后重新导入！\r\n";
            }
            if (!string.IsNullOrEmpty(message))
            {
                Log.Warning(message);
            }
            //筛选出所有task早于QBSeed或者task为QBSeed但是任务状态为false的记录，并且task不能为PtSiqiTask.None
            //var ptSiqiTasking = (from l in context.Feeds
            //                     where l.Task > FeedTask.None && (l.Task < FeedTask.Seed || (l.Task == FeedTask.Seed && l.TaskState == false))
            //                     select l).ToList();
            return message;
        }
        /// <summary>
        /// 获得pdf预览图
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [SupportedOSPlatform("windows")]
        [SupportedOSPlatform("linux")]
        public static async Task<PtSiqiPaths> GetPDFPreviewsAsync(Feed feed, string? eventBody = null)
        {
            var seedId = (int)feed.Id;

            var ptSiqiPathsInput = !string.IsNullOrEmpty(eventBody) ? JsonConvert.DeserializeObject<PtSiqiPaths>(eventBody) : new PtSiqiPaths();
            var paths = ptSiqiPathsInput?.Data?.FirstOrDefault(p => p.SeedId == seedId)?.Paths ?? [];
            if (paths.Count == 0)
            {
                paths = feed.Extension switch
                {
                    Extension.PDF => [.. Directory.GetFiles(feed.Path, "*.pdf", SearchOption.AllDirectories)],
                    Extension.CBZOrCBR => [.. Directory.GetFiles(feed.Path, "*.zip", SearchOption.AllDirectories), .. Directory.GetFiles(feed.Path, "*.rar", SearchOption.AllDirectories), .. Directory.GetFiles(feed.Path, "*.cbz", SearchOption.AllDirectories), .. Directory.GetFiles(feed.Path, "*.cbr", SearchOption.AllDirectories)]
                    //Extension.EPUB => throw new NotImplementedException(),
                    //Extension.ImageFormat => throw new NotImplementedException(),
                    //Extension.MOBI => throw new NotImplementedException(),
                    //Extension.Mixing => throw new NotImplementedException(),
                    //Extension.Other => throw new NotImplementedException(),
                    //_ => throw new NotImplementedException()
                };
            }
            paths.Sort();
            paths.RemoveRange(1, paths.Count - 1);
            var ptSiqiPathsOutput = new PtSiqiPaths() { Data = [] };
            List<string>? picPaths = [];
            foreach (var path in paths)
            {
                var webRootPath = Directory.GetCurrentDirectory();
                var uploaderFolder = Path.Combine(webRootPath, "previews");
                if (!Directory.Exists(uploaderFolder))
                {
                    Directory.CreateDirectory(uploaderFolder);
                }
                var previewsLists = Directory.GetFiles(uploaderFolder, $"{{seedid-{feed.Id}}}*", searchOption: SearchOption.AllDirectories).ToList();
                previewsLists.Sort();
                if (previewsLists.Count == 4)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        var picPath = Path.Combine(FeedPtSiqi.Configuration.AutoDataPath, $"{feed.Title} {{seedid-{feed.Id}}}", $"preview_page{i + 1:D3} {{seedid-{seedId}}}.jpg");
                        File.Move(previewsLists[i], picPath);
                        picPaths.Add(picPath);
                    }
                }
                else
                {
                    picPaths = feed.Extension switch
                    {
                        Extension.PDF => PdfToImage.ToPicRandom(path, feed),
                        Extension.CBZOrCBR => await CBZToImage.ToPicRandom(path, feed),
                        Extension.EPUB => throw new NotImplementedException(),
                        Extension.ImageFormat => throw new NotImplementedException(),
                        Extension.MOBI => throw new NotImplementedException(),
                        Extension.Mixing => throw new NotImplementedException(),
                        Extension.Other => throw new NotImplementedException(),
                        _ => throw new NotImplementedException()
                    };
                }
                ptSiqiPathsOutput?.Data?.Add(new PtSiqiPath()
                {
                    SeedId = seedId,
                    SelectPDF = path,
                    Paths = picPaths
                });
            }
            //如果picPaths的数量小于4，则抛出异常
            if (picPaths != null && picPaths.Count < 4)
            {
                throw new Exception($"“{paths[0]}”生成的预览图数量小于4张，请在需要的文件名前增加001前缀！");
            }
            return ptSiqiPathsOutput ?? new PtSiqiPaths();
        }
        /// <summary>
        /// 上传pdf预览图获得URL
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<PtSiqiPaths> GetPDFPreviewsUrlsAsync(Feed feed, AutoFeedSiqiDbContext context, string? lskyHost = null, string? lskyToken = null, string? eventBody = null)
        {
            lskyHost = string.IsNullOrEmpty(lskyHost) ? Configuration.IskyHost : lskyHost;
            var client = new RestClient(lskyHost);

            lskyToken = string.IsNullOrEmpty(lskyToken) ? Configuration.IskyToken : lskyToken;
            //如果参数和appsetting、读取的lskyToken都是空，重新注册获得lskyToken，写入到appsetting
            if (string.IsNullOrEmpty(lskyToken))
            {
                var lskyEmail = Configuration.IskyEmail;
                var lskyPassword = Configuration.IskyPassword;
                lskyToken = await CreateIskyToken(client, lskyEmail, lskyPassword);
                //UpdateIskyTokenToAppsetting(lskyToken);
                UpdateIskyTokenToDatabase(lskyToken);
            }
            var ptSiqiPaths = new PtSiqiPaths() { Data = [] };

            static async Task CoreAsync(string lskyToken, Feed feed, string path, RestClient client, AutoFeedSiqiDbContext context, PtSiqiPaths ptSiqiPaths)
            {
                var url = await Upload(client, path, lskyToken, feed);
                var seedId = (int)feed.Id;
                feed.PreviewUrl = string.IsNullOrWhiteSpace(feed.PreviewUrl) ? $@"{url}" : $@"{feed.PreviewUrl};{url}";
                feed.LastUpdatedAt = DateTime.Now;
                context.Update(feed);
                await context.SaveChangesAsync();
                ptSiqiPaths?.Data?.Add(new PtSiqiPath()
                {
                    SeedId = seedId,
                    Preview = path,
                    PreviewUrl = url
                });
                Log.Information($@"获得预览图URL! seedId：{{{nameof(seedId)}}}；url：{{{nameof(url)}}}；path：{{{nameof(path)}}}", seedId, url, path);
                await Task.Delay(1000);
            }

            static async Task<string> CreateIskyToken(RestClient client, string email, string password)
            {
                //创建请求
                var request = new RestRequest("tokens", Method.Post);
                var newRequestBody = @$"{{
    ""email"": ""{email}"",
    ""password"": ""{password}""
}}";
                request.AddStringBody(newRequestBody, DataFormat.Json);
                //执行请求
                var response = await client.ExecuteAsync(request);
                var responseContent = response.Content ?? string.Empty;
                //解析返回结果
                /*{
    "status": true,
    "message": "success",
    "data": {
        "token": ""
    }
}*/
                var eventBodyObject = JsonConvert.DeserializeObject<JObject>(responseContent);
                var lskyToken = eventBodyObject?["data"]?["token"]?.ToString() ?? string.Empty;
                return $"Bearer {lskyToken}";
            }

            static async Task<string> Upload(RestClient client, string path, string lskyToken, Feed feed)
            {
                //创建请求
                var request = new RestRequest("upload", Method.Post);
                request.AddHeader("Authorization", lskyToken);
                #region newRequestBody
                //request.AddHeader("Content-Type", "application/json");
                //var newRequestBody = @$"{{""email"":""1486193791@qq.com"",""password"":""ohDJkfcv^3!upOE4""}}";
                //request.AddStringBody(newRequestBody, DataFormat.Json);
                #endregion
                request.AddFile("file", path);
                //执行请求
                var response = await client.ExecuteAsync(request);
                var responseContent = response.Content ?? string.Empty;
                //解析返回结果
                Log.Information($@"上传预览图! path：{{{nameof(path)}}}；responseContent：{{{nameof(responseContent)}}}", path, responseContent);
                var responseContentPath = Path.Combine(Configuration.AutoDataPath, $"{feed.Title} {{seedid-{feed.Id}}}", $"上传预览图回复内容.txt");
                File.AppendAllText(responseContentPath, $"{responseContent}\r\n\r\n");
                var eventBodyObject = JsonConvert.DeserializeObject<JObject>(responseContent);
                var url = eventBodyObject?["data"]?["links"]?["url"]?.ToString() ?? string.Empty;
                //获得url后，去掉前面的host部分，只保留路径部分
                url = new Uri(url).PathAndQuery;
                return url;
            }

            static void UpdateIskyTokenToAppsetting(string lskyToken)
            {
                var appSettingsPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
                var json = File.ReadAllText(appSettingsPath);

                var jsonSettings = new JsonSerializerSettings();
                jsonSettings.Converters.Add(new ExpandoObjectConverter());
                jsonSettings.Converters.Add(new StringEnumConverter());

                dynamic config = JsonConvert.DeserializeObject<ExpandoObject>(json, jsonSettings);

                //更新
                config.PtSiqiTakeupLoad.IskyToken = lskyToken;
                var newJson = JsonConvert.SerializeObject(config, Formatting.Indented, jsonSettings);
                File.WriteAllText(appSettingsPath, newJson);
            }

            static void UpdateIskyTokenToDatabase(string lskyToken)
            {
                using var context = DatabaseUtil.PooledDbContextFactory.CreateDbContext();
                var config = context.Configurations.FirstOrDefault();
                if (config != null)
                {
                    config.IskyToken = lskyToken;
                    context.Update(config);
                    context.SaveChanges();
                }
            }

            var seedId = (int)feed.Id;
            var ptSiqiPathsInput = !string.IsNullOrEmpty(eventBody) ? JsonConvert.DeserializeObject<PtSiqiPaths>(eventBody) : new PtSiqiPaths();
            var paths = ptSiqiPathsInput?.Data?.FirstOrDefault(p => p.SeedId == seedId)?.Paths ?? [];

            if (paths.Count == 0)
            {
                var autoData = Path.Combine(Configuration.AutoDataPath, $"{feed.Title} {{seedid-{feed.Id}}}");
                paths = [.. Directory.GetFiles(autoData, "*.jpg")];
                paths.Sort();
                paths.RemoveRange(4, paths.Count - 4);
            }

            var ptSiqiPathsOutput = new PtSiqiPaths() { Data = [] };
            foreach (var path in paths)
            {
                await CoreAsync(lskyToken, feed, path, client, context, ptSiqiPaths);
            }
            return ptSiqiPathsOutput ?? new PtSiqiPaths();
        }
        /// <summary>
        /// 创建nfo文件
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<PtSiqiPaths> CreateNfoAsync(Feed feed)
        {
            //不接受eventBody的值
            //var seedId = Regex.Match(path, @".* {seedid-(?<id>\d*?)}$").Groups["id"].Value;
            var seedId = (int)feed.Id;
            var path = feed.Path;

            //序列化枚举为字符串
            var converter = new StringEnumConverter();
            var ptSiqisJson = JsonConvert.SerializeObject(feed, Formatting.Indented, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Converters = { converter }
            });
            var nfoPath = Path.Combine(Configuration.AutoDataPath, $"{feed.Title} {{seedid-{feed.Id}}}", $"books {{seedid-{seedId}}}.nfo");
            File.WriteAllText(nfoPath, ptSiqisJson);

            var ptSiqiPathsOutput = new PtSiqiPaths() { Data = [] };
            ptSiqiPathsOutput?.Data?.Add(new PtSiqiPath()
            {
                SeedId = seedId,
                Nfo = nfoPath
            });
            Log.Information($"已写入NFO! seedId：{{{nameof(seedId)}}}；nfoPath：{{{nameof(nfoPath)}}}", seedId, nfoPath);

            return ptSiqiPathsOutput ?? new PtSiqiPaths();
        }
        /// <summary>
        /// 制作种子
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task<PtSiqiPaths> CreateSeedAsync(Feed feed, string? siqiTrack = null, string? transmissionPath = null)
        {
            siqiTrack = string.IsNullOrEmpty(siqiTrack) ? Configuration.SiqiTrack : siqiTrack;

            //不接受eventBody的值
            var seedId = (int)feed.Id;
            var path = feed.Path;

            var torrentPath = Path.Combine(Configuration.AutoDataPath, $"{feed.Title} {{seedid-{feed.Id}}}", $"{feed.Title}.torrent");
            #region obsolete
            //transmissionPath = string.IsNullOrEmpty(transmissionPath) ? PtSiqiTakeupLoadConfigure.PtSiqiTakeupLoad.TransmissionPath : transmissionPath;
            //var arguments = $"""
            //         -p -t "{siqiTrack}" -o "{torrentPath}" "{path}"
            //        """;
            //运行命令行时，避免乱码情况发生
            //System.Console.InputEncoding = System.Text.Encoding.UTF8;
            //var process = Process.Start(new ProcessStartInfo()
            //{
            //    FileName = transmissionPath,
            //    Arguments = arguments
            //    #region obsolete
            //    //UseShellExecute = false,
            //    //StandardInputEncoding = Encoding.UTF8,
            //    //StandardOutputEncoding = Encoding.UTF8,
            //    //StandardErrorEncoding = Encoding.UTF8,
            //    //RedirectStandardInput = true,
            //    //RedirectStandardOutput = true,
            //    //RedirectStandardError = true
            //    #endregion
            //});
            //process?.WaitForExit();
            //var a = new TorrentFile().CreateFile(inputPath: path,
            //torrentName: feed.Title,
            //trackerAnnounceUrl: siqiTrack,
            //outputFile: torrentPath,
            //pieceSize: PieceSize.Auto);
            //System.Console.InputEncoding = System.Text.Encoding.UTF8;
            #endregion
            _ = new TorrentFile().CreateFile(path, feed.Title, siqiTrack, torrentPath);
            var ptSiqiPathsOutput = new PtSiqiPaths() { Data = [] };
            ptSiqiPathsOutput?.Data?.Add(new PtSiqiPath()
            {
                SeedId = seedId,
                Torrent = torrentPath
                //CreatTorrentCmd = $"{transmissionPath}{arguments}"
            });
            Log.Information($"已生成种子! seedId：{{{nameof(seedId)}}}；torrentPath：{{{nameof(torrentPath)}}}", seedId, torrentPath);

            return ptSiqiPathsOutput ?? new PtSiqiPaths();
        }
        /// <summary>
        /// 后台上传发种
        /// </summary>
        /// <param name="token"></param>
        /// <param name="seedId"></param>
        /// <returns></returns>
        public static async Task<uint> SeedSiqiBackAsync(Feed feed, AutoFeedSiqiDbContext context, string? eventBody = null)
        {
            static List<string> GetPreviewUrls(Feed feed) => [.. feed.PreviewUrl.Split(";")];

            static string AddContent(Feed feed, string oldDescr, string split = " | ")
            {
                if (split == "\r\n" && feed.DoubanId != 0)
                {
                    oldDescr = $@"豆瓣：[u][url=https://book.douban.com/subject/{feed.DoubanId}]https://book.douban.com/subject/{feed.DoubanId}[/url][/u]{split}{oldDescr}";
                }
                if (!string.IsNullOrWhiteSpace(feed.Isbn))
                {
                    oldDescr = $@"ISBN: {feed.Isbn}{split}{oldDescr}";
                }
                if (feed.Date != DateTime.UnixEpoch && feed.Date != DateTime.MinValue)
                {
                    oldDescr = $@"出版年: {feed.Date:yyyy-MM}{split}{oldDescr}";
                }
                if (!string.IsNullOrWhiteSpace(feed.Press))
                {
                    oldDescr = $@"出版社: {feed.Press}{split}{oldDescr}";
                }
                if (!string.IsNullOrWhiteSpace(feed.Authors))
                {
                    oldDescr = $@"作者: {feed.Authors}{split}{oldDescr}";
                }
                return oldDescr;
            }

            //作者: {feed.Authors} | 出版社: {feed.Press} | 出版年: {feed.Date} | ISBN: {feed.Isbn} | PDF
            static string GetSmallDescr(Feed feed) => AddContent(feed, $@"格式: {feed.Extension}", " | ");

            /*@$"[quote][color=Red]本站所有资源均是用户在网络上收集且上传发布的，本站不存储任何资源文件，且本站对用户所发布的所有资源内容一无所知。

本站内容仅做宽带网速测试、学习交流等使用，请您在下载后24小时内尽快删除。

转载本站官方资源至他站时，请务必注明此资源转载自思齐PT。[/color][/quote]
[img]{baseurl}/{previewUrls[0]}[/img]
[size=4][quote][b]作者：{feed.Authors}
出版社：{feed.Press}
出版年：{feed.Date}
ISBN：{feed.Isbn}
豆瓣：[u][url=https://book.douban.com/subject/{feed.DoubanId}]https://book.douban.com/subject/{feed.DoubanId}[/url][/u][/b]

[b]简介：[/b]{feed.Summary}[/quote][/size]
[img]{baseurl}/{previewUrls[1]}[/img]
[img]{baseurl}/{previewUrls[2]}[/img]
[img]{baseurl}/{previewUrls[3]}[/img]"*/
            static string GetDescr(Feed feed, string iskyHost, bool isUploader)
            {
                var previewUrls = GetPreviewUrls(feed);
                iskyHost = new Uri(iskyHost).GetLeftPart(UriPartial.Authority);

                var uploaderDescr = string.Empty;
                if (isUploader)
                {
                    uploaderDescr = @"[quote][color=Blue]本资源由思齐网络收集并整理发布、仅供学习交流等使用，若喜欢请支持购买正版。[/color][/quote]";
                }
                var beginningPartDescr = @$"[quote][color=Red]本站所有资源均是用户在网络上收集且上传发布的，本站不存储任何资源文件，且本站对用户所发布的所有资源内容一无所知。
本站内容仅做宽带网速测试、学习交流等使用，请您在下载后24小时内尽快删除。
转载本站官方资源至他站时，请务必注明此资源转载自思齐PT。[/color][/quote]{uploaderDescr}
[img]{iskyHost}{previewUrls[0]}[/img]
[size=4][quote][b]";
                var middlePartDescr = AddContent(feed, $@"格式: {feed.Extension}", "\r\n");//选择"\r\n"而不是"\n"，因为其他部分也有"\r\n"，索性一致，方便后面替换
                var endingPartDescr = @$"[/b]

[b]简介：[/b]{feed.Summary}[/quote][/size]
[img]{iskyHost}{previewUrls[1]}[/img]
[img]{iskyHost}{previewUrls[2]}[/img]
[img]{iskyHost}{previewUrls[3]}[/img]";
                var fileLists = Directory.GetFiles(feed.Path, "*", searchOption: SearchOption.AllDirectories).Select(f => f.Replace(Path.GetDirectoryName(feed.Path)!, "")[1..]);
                var fileView = $@"
[quote][color=Blue]文件目录：[/color]
{string.Join("\r\n", fileLists)}[/quote]";
                return $@"{beginningPartDescr}{middlePartDescr}{endingPartDescr}{fileView}".Replace("\r\n", "\n");
            }

            //static uint GetMapId(List<PtSiqiInfoMap> ptSiqiInfoMaps, string type, string elementName) => (from l in ptSiqiInfoMaps
            //                                                                                              where l.Group == type && l.ElementName == elementName
            //                                                                                              select l.ElementId).ToList().First();
            //发布上传
            static async Task<uint> TakeupLoadAsync(RestClient client, Feed feed, string host, string iskyHost, string cookie, bool isUploader, bool invisible)
            {
                var seedId = (int)feed.Id;
                var path = feed.Path;
                var title = feed.Title;
                //创建请求
                var request = new RestRequest("takeupload.php", Method.Post);
                ///添加header
                request.AddHeader("Cookie", cookie);
                request.AddHeader("user-agent", $"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36 Edg/141.0.0.0");
                request.AddHeader("referer", $"{host}/upload.php");
                request.AddHeader("origin", host);
                //添加文件
                var torrentPath = Path.Combine(Configuration.AutoDataPath, $"{feed.Title} {{seedid-{feed.Id}}}", $"{feed.Title}.torrent");
                var getFile = File.ReadAllBytes(torrentPath);
                request.AddFile("file", getFile, $"seedId-{seedId}.torrent");
                //添加表单
                var smallDescr = GetSmallDescr(feed);
                var descr = GetDescr(feed, iskyHost, isUploader);
                //var typeId = GetMapId(ptSiqiInfoMaps, "type", feed.Type);
                //var sortId = GetMapId(ptSiqiInfoMaps, "sort", feed.Sort);
                //var sourceId = GetMapId(ptSiqiInfoMaps, "source", feed.Source);
                //var conditionId = GetMapId(ptSiqiInfoMaps, "condition", feed.Condition);
                //var colorId = GetMapId(ptSiqiInfoMaps, "color", feed.Color);
                //var extensionId = GetMapId(ptSiqiInfoMaps, "extension", feed.Extension);
                //var languageId = GetMapId(ptSiqiInfoMaps, "language", feed.Language);
                //var qualityId = GetMapId(ptSiqiInfoMaps, "quality", feed.Quality);
                request.AddParameter("name", title);
                request.AddParameter("small_descr", smallDescr);
                request.AddParameter("descr", descr);
                request.AddParameter("type", (int)feed.Type);
                request.AddParameter("audiocodec_sel[4]", (int)feed.Sort);
                request.AddParameter("source_sel[4]", (int)feed.Source);
                request.AddParameter("medium_sel[4]", (int)feed.Condition);
                request.AddParameter("codec_sel[4]", (int)feed.Color);
                request.AddParameter("standard_sel[4]", (int)feed.Extension);
                request.AddParameter("processing_sel[4]", (int)feed.Language);
                request.AddParameter("customcat_sel[4]", (int)feed.Quality);
                request.AddParameter("team_sel[4]", (int)feed.Team);
                request.AddParameter("tags[4][]", "5");
                if (isUploader)
                {
                    request.AddParameter("tags[4][]", "1");
                    request.AddParameter("tags[4][]", "2");
                    request.AddParameter("tags[4][]", "3");
                }
                request.AddParameter("pos_state", "normal");
                request.AddParameter("pos_state_until", "");
                if (invisible)
                {
                    request.AddParameter("uplver", "yes");
                }
                //执行请求
                var response = await client.ExecuteAsync(request);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    //解析返回结果
                    var responseContent = response.Content ?? string.Empty;
                    //Log.Information($"上传种子响应内容! seedId：{{{nameof(seedId)}}}；responseContent：{{{nameof(responseContent)}}}", seedId, responseContent);
                    var responseContentPath = Path.Combine(Configuration.AutoDataPath, $"{feed.Title} {{seedid-{feed.Id}}}", $"发种回复内容.txt");
                    File.WriteAllText(responseContentPath, responseContent);
                    var torrentId = Regex.Match(response.ResponseUri?.OriginalString ?? string.Empty, @".*id=(\d*?)&.*").Groups[1].Value;
                    Log.Information($"已发布种子! seedId：{{{nameof(seedId)}}}；标题：{{{nameof(title)}}}；思齐ID：{{{nameof(torrentId)}}}", seedId, title, torrentId);
                    return uint.Parse(torrentId ?? "0");
                }
                else
                {
                    return 0;
                }
            }

            //读取配置参数
            var ptSiqiTakeupLoad = Configuration;
            if (!string.IsNullOrEmpty(eventBody))
            {
                ptSiqiTakeupLoad = JsonConvert.DeserializeObject<Configuration>(eventBody) ?? new Configuration();
            }
            //https://img.si-qi.xyz/api/v1
            var siqiHost = ptSiqiTakeupLoad.SiqiHost;
            var siqiCookie = ptSiqiTakeupLoad.SiqiCookie;
            var siqiUploaderToken = ptSiqiTakeupLoad.SiqiUploaderToken;
            var iskyHost = ptSiqiTakeupLoad.IskyHost;
            var isUploader = Configuration.SiqiUploaderToken == "d6k55a0lvancydsnnrgumxf7u8n56g52";
            var invisible = ptSiqiTakeupLoad.Invisible;

            //创建主机
            var client = new RestClient(siqiHost);
            //var ptSiqiInfoMaps = context.PtSiqiInfoMaps.ToList();

            var siqiTorrentId = await TakeupLoadAsync(client: client,
                                                      //ptSiqiInfoMaps: ptSiqiInfoMaps,
                                                      feed: feed,
                                                      host: siqiHost,
                                                      iskyHost: iskyHost,
                                                      cookie: siqiCookie,
                                                      isUploader: isUploader,
                                                      invisible: invisible);
            await Task.Delay(5000);
            return siqiTorrentId;
        }
        /// <summary>
        /// QB做种
        /// </summary>
        /// <param name="feed"></param>
        /// <param name="siqiId"></param>
        /// <returns></returns>
        public static async Task QBSeedAsync(Feed feed, uint siqiId)
        {
            var seedId = (int)feed.Id;
            var path = feed.Path;
            var title = feed.Title;

            //获得qBittorrent cookie
            static async Task<string> GetQBittorrentCookieAsync(RestClient qBittorrentClient, string username, string password)
            {
                var authRequest = new RestRequest("auth/login", Method.Post);
                authRequest.AddParameter("username", username);
                authRequest.AddParameter("password", password);
                var authResult = await qBittorrentClient.ExecuteAsync(authRequest);
                var cookiesTemp = authResult?.Cookies?[0];
                var qBCookie = $"{cookiesTemp?.Name}={cookiesTemp?.Value}";
                return qBCookie;
            }

            //读取配置参数
            var ptSiqiTakeupLoad = Configuration;
            var siqiHost = ptSiqiTakeupLoad.SiqiHost;
            var siqiPasskey = ptSiqiTakeupLoad.SiqiPasskey;
            //http://192.168.0.118:9091/api/v2
            var qbHost = ptSiqiTakeupLoad.QbHost;
            var qbUserName = ptSiqiTakeupLoad.QbUserName;
            var qbPassword = ptSiqiTakeupLoad.QbPassword;
            var qbTags = ptSiqiTakeupLoad.QbTags;

            var qBittorrentClient = new RestClient(qbHost);
            //获得QBittorrentCookie，在本次会话中有效
            var qBCookie = await GetQBittorrentCookieAsync(qBittorrentClient, username: qbUserName, password: qbPassword);

            var siqiTorrentUrl = $"{siqiHost}/download.php?id={siqiId}&passkey={siqiPasskey}";

            var torrentsRequest = new RestRequest("torrents/add", Method.Post);
            torrentsRequest.AddHeader("cookie", qBCookie);
            torrentsRequest.AlwaysMultipartFormData = true;
            torrentsRequest.AddParameter("urls", siqiTorrentUrl);
            torrentsRequest.AddParameter("savepath", Path.GetDirectoryName(path));
            torrentsRequest.AddParameter("skip_checking", "true");
            torrentsRequest.AddParameter("paused", "false");
            torrentsRequest.AddParameter("tags", qbTags);
            var result = await qBittorrentClient.ExecuteAsync(torrentsRequest);

            Log.Information($"qB做种：思齐ID={{{nameof(siqiId)}}}", siqiId);
        }
        /// <summary>
        /// 整合各个子功能，一条龙发种思齐PT
        /// </summary>
        /// <returns></returns>
        public static async Task<string> AutoSeedSiqiAsync(List<Feed> feeds)
        {
            static async Task<(FeedTask nextTask, bool nextTaskState)> ChangeTaskAsync(AutoFeedSiqiDbContext context, Feed feed, Feed? feedHistory, Func<Task> func, FeedTask task, FeedTask nextTask, bool nextTaskState = false)
            {
                Log.Information($@"“{{{nameof(task)}}}”任务开始！", task.ToDisplayName());

                await func();

                feed.Task = nextTask;
                feed.TaskState = nextTaskState;
                feed.LastUpdatedAt = DateTime.Now;
                context.Update(feed);
                await context.SaveChangesAsync();

                //修改FeedHistory.Feeds对应的task和taskState
                feedHistory!.Task = nextTask;
                feedHistory.TaskState = nextTaskState;

                Log.Information($@"“{{{nameof(task)}}}”任务结束！", task.ToDisplayName());
                return (nextTask, nextTaskState);
            }

            var context = DatabaseUtil.PooledDbContextFactory.CreateDbContext();

            //去除feeds中task任务为Seed并且状态为true的记录
            var oldCount = feeds.Count;
            feeds = feeds.Where(f => !(f.Task == FeedTask.Seed && f.TaskState == true)).ToList();
            var count = feeds.Count;
            var skipCount = oldCount - count;
            var index = 1;
            Log.Information($"找到 {{{nameof(count)}}} 条待发种记录,跳过{{{nameof(skipCount)}}}条已发种记录", count, skipCount);
            //按照id升序处理
            feeds = feeds.OrderBy(f => f.Id).ToList();
            foreach (var feed in feeds)
            {
                var seedId = feed.Id;
                var feedHistory = FeedHistory.Feeds.FirstOrDefault(fh => fh.Id == seedId);

                var title = feed.Title;
                var path = feed.Path;
                var task = feed.Task;
                var taskState = feed.TaskState;

                Log.Information($@"【{{{nameof(index)}}}/{{{nameof(count)}}}】开始自动发种！seedId：{{{nameof(seedId)}}}；标题：{{{nameof(title)}}}；文件夹：{{{nameof(path)}}}", index, count, seedId, title, path);
                index++;
                //如果AutoData目录不存在，则创建
                var autoData = Path.Combine(Configuration.AutoDataPath, $"{feed.Title} {{seedid-{feed.Id}}}");
                if (!Directory.Exists(autoData))
                {
                    Directory.CreateDirectory(autoData);
                    Log.Information($"已创建AutoData目录！路径：{{{nameof(autoData)}}}", autoData);
                }
                //正常情况下，task == FeedTask.None ，忽略taskState，一路执行到PtSiqiTask.Seed，然后结束。
                if (task == FeedTask.None)
                {
                    feed.Task = FeedTask.GetPreviews;
                    feed.TaskState = false;
                    feed.LastUpdatedAt = DateTime.Now;
                    context.Update(feed);
                    await context.SaveChangesAsync();
                    task = FeedTask.GetPreviews;
                    taskState = false;
                }

                if (task == FeedTask.GetPreviews && taskState == false)
                {
                    (task, taskState) = await ChangeTaskAsync(context, feed, feedHistory, async () => await GetPDFPreviewsAsync(feed), task, FeedTask.GetPreviewsUrls);
                }
                else
                {
                    Log.Information($@"跳过已完成任务“{{{nameof(FeedTask.GetPreviews)}}}”！", FeedTask.GetPreviews.ToDisplayName());
                }

                if (task == FeedTask.GetPreviewsUrls && taskState == false)
                {
                    (task, taskState) = await ChangeTaskAsync(context, feed, feedHistory, async () => await GetPDFPreviewsUrlsAsync(feed, context), task, FeedTask.CreateNfo);
                }
                else
                {
                    Log.Information($@"跳过已完成任务“{{{nameof(FeedTask.GetPreviewsUrls)}}}”！", FeedTask.GetPreviewsUrls.ToDisplayName());
                }

                if (task == FeedTask.CreateNfo && taskState == false)
                {
                    (task, taskState) = await ChangeTaskAsync(context, feed, feedHistory, async () => await CreateNfoAsync(feed), task, FeedTask.CreateSeed);
                }
                else
                {
                    Log.Information($@"跳过已完成任务“{{{nameof(FeedTask.CreateNfo)}}}”！", FeedTask.CreateNfo.ToDisplayName());
                }

                if (task == FeedTask.CreateSeed && taskState == false)
                {
                    (task, taskState) = await ChangeTaskAsync(context, feed, feedHistory, async () => await CreateSeedAsync(feed), task, FeedTask.Feed);
                }
                else
                {
                    Log.Information($@"跳过已完成任务“{{{nameof(FeedTask.CreateSeed)}}}”！", FeedTask.CreateSeed.ToDisplayName());
                }

                uint siqiId = 0;
                if (task == FeedTask.Feed && taskState == false)
                {
                    (task, taskState) = await ChangeTaskAsync(context, feed, feedHistory, async () => siqiId = await SeedSiqiBackAsync(feed, context), task, FeedTask.Seed);

                    feed.SiqiId = siqiId;
                    feed.LastUpdatedAt = DateTime.Now;
                    context.Update(feed);
                    await context.SaveChangesAsync();
                    feedHistory!.SiqiId = siqiId;
                }
                else
                {
                    Log.Information($@"跳过已完成任务“{{{nameof(FeedTask.Feed)}}}”！", FeedTask.Feed.ToDisplayName());
                }

                if (task == FeedTask.Seed && taskState == false)
                {
                    //如果siqiId是0，则从数据库中重新获取
                    if (siqiId == 0)
                    {
                        siqiId = context.Feeds.FirstOrDefault(f => f.Id == feed.Id)?.SiqiId ?? 0;
                    }
                    //如果siqiId不为0，则进行qB做种
                    if (siqiId != 0)
                    {
                        (task, taskState) = await ChangeTaskAsync(context, feed, feedHistory, async () => await QBSeedAsync(feed, siqiId), task, task, nextTaskState: true);
                        Log.Information($@"自动发种完成！seedId：{{{nameof(seedId)}}}；标题：{{{nameof(title)}}}；文件夹：{{{nameof(path)}}}", seedId, title, path);
                    }
                    else
                    {
                        Log.Error("siqiId为空");
                    }
                }
                else
                {
                    Log.Warning($@"存在异常任务：“{{{nameof(task)}}}”！", task.ToDisplayName());
                    return $@"存在异常任务：“{task}”！";
                }
            }
            return "发种已完成";
        }
    }
}
