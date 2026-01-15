using AutoFeedSiqi.Models;
using AutoFeedSiqi.Models.BookInfoExtractor;
using AutoFeedSiqi.Models.CommonComponents;
using AutoFeedSiqi.Models.CommonComponents.EntityTypeConfiguration;
using AutoFeedSiqi.Models.CommonComponents.Model;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Data;

namespace AutoFeedSiqi.Controllers
{
    public class FeedSiqiController : BaseController
    {
        /// <summary>
        /// 上传Excel内容到数据库
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> UploadExcelToDatabaseAsync(string token)
        {
            return await CommonReturnAsync(token, async (eventBody) =>
            {
                using var context = DatabaseUtil.PooledDbContextFactory.CreateDbContext();
                var excelPath = (await ControllerUtil.GetPathsAsync(eventBody)).First();
                var feed = await FeedPtSiqi.UploadExcelToDatabaseAsync(excelPath, context);
                return Ok(new { data = feed });
            });
        }
        /// <summary>
        /// 获得pdf预览图
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> GetPDFPreviewsAsync(string token, int seedId)
        {
            return await CommonReturnAsync(token, async (eventBody) =>
            {
                using var context = DatabaseUtil.PooledDbContextFactory.CreateDbContext();
                var ptSiqiPaths = await FeedPtSiqi.GetPDFPreviewsAsync(GetPtSiqi(context, seedId), eventBody);
                return Ok(ptSiqiPaths);
            });
        }
        /// <summary>
        /// 上传pdf预览图获得URL
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> GetPDFPreviewsUrlsAsync(string token, int seedId, string? lskyHost, string? lskyToken)
        {
            return await CommonReturnAsync(token, async (eventBody) =>
            {
                using var context = DatabaseUtil.PooledDbContextFactory.CreateDbContext();
                var ptSiqiPaths = await FeedPtSiqi.GetPDFPreviewsUrlsAsync(GetPtSiqi(context, seedId), context, lskyHost, lskyToken, eventBody);
                return Ok(ptSiqiPaths);
            });
        }
        /// <summary>
        /// 创建nfo文件
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> CreateNfoAsync(string token, int seedId)
        {
            return await CommonReturnAsync(token, async (eventBody) =>
            {
                using var context = DatabaseUtil.PooledDbContextFactory.CreateDbContext();
                var ptSiqiPaths = await FeedPtSiqi.CreateNfoAsync(GetPtSiqi(context, seedId));
                return Ok(ptSiqiPaths);
            });
        }
        /// <summary>
        /// 制作种子
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> CreateSeedAsync(string token, int seedId, string? siqiTrack, string? transmissionPath)
        {
            return await CommonReturnAsync(token, async (eventBody) =>
            {
                using var context = DatabaseUtil.PooledDbContextFactory.CreateDbContext();
                var ptSiqiPaths = await FeedPtSiqi.CreateSeedAsync(GetPtSiqi(context, seedId), siqiTrack, transmissionPath);
                return Ok(ptSiqiPaths);
            });
        }
        /// <summary>
        /// 生成js模板，一键填入信息
        /// </summary>
        /// <param name="token"></param>
        /// <param name="seedId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> SeedSiqiAsync(string token, int seedId, int? endSeedId)
        {
            return await CommonReturnAsync(token, async (eventBody) =>
            {
                using var context = DatabaseUtil.PooledDbContextFactory.CreateDbContext();
                var feed = (from l in context.Feeds
                            where l.Id == seedId
                            select l).First();
                var baseurl = @"https://img.si-qi.xyz/g";
                var previewUrls = feed.PreviewUrl.Split(";").ToList();
                #region 获得descr
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
                var descr = string.Empty;
                var beginningPartDescr = @$"[quote][color=Red]本站所有资源均是用户在网络上收集且上传发布的，本站不存储任何资源文件，且本站对用户所发布的所有资源内容一无所知。

本站内容仅做宽带网速测试、学习交流等使用，请您在下载后24小时内尽快删除。
转载本站官方资源至他站时，请务必注明此资源转载自思齐PT。[/color][/quote]
[quote][color=Blue]本资源由思齐网络收集并整理发布、仅供学习交流等使用，若喜欢请支持购买正版。[/color][/quote]
[img]{baseurl}/{previewUrls[0]}[/img]
[size=4][quote][b]";
                var endingPartDescr = @$"[/b]

[b]简介：[/b]{feed.Summary}[/quote][/size]
[img]{baseurl}/{previewUrls[1]}[/img]
[img]{baseurl}/{previewUrls[2]}[/img]
[img]{baseurl}/{previewUrls[3]}[/img]";
                var middlePartDescr = $@"格式: {feed.Extension}";
                //作者: {feed.Authors} | 出版社: {feed.Press} | 出版年: {feed.Date} | ISBN: {feed.Isbn} | PDF
                var smallDescr = $@"格式: {feed.Extension}";
                if (feed.DoubanId != 0)
                {
                    middlePartDescr = $@"豆瓣：[u][url=https://book.douban.com/subject/{feed.DoubanId}]https://book.douban.com/subject/{feed.DoubanId}[/url][/u]\n{middlePartDescr}";
                }
                if (!string.IsNullOrWhiteSpace(feed.Isbn))
                {
                    smallDescr = $@"ISBN: {feed.Isbn} | {smallDescr}";
                    middlePartDescr = $"ISBN：{feed.Isbn}\r\n{middlePartDescr}";
                }
                if (feed.Date != DateTime.MinValue)
                {
                    smallDescr = $@"出版年: {feed.Date} | {smallDescr}";
                    middlePartDescr = $"出版年：{feed.Date}\r\n{middlePartDescr}";
                }
                if (!string.IsNullOrWhiteSpace(feed.Press))
                {
                    smallDescr = $@"出版社: {feed.Press} | {smallDescr}";
                    middlePartDescr = $"出版社：{feed.Press}\r\n{middlePartDescr}";
                }
                if (!string.IsNullOrWhiteSpace(feed.Authors))
                {
                    smallDescr = $@"作者: {feed.Authors} | {smallDescr}";
                    middlePartDescr = $"作者：{feed.Authors}\r\n{middlePartDescr}";
                }
                descr = $@"{beginningPartDescr}{middlePartDescr}{endingPartDescr}";
                descr = descr.Replace("\r\n", @"\n");
                #endregion
                //添加种子
                var torrentPath = @$"C:\\Users\\yxlonger\\Documents\\Downloads\\pt\\YxlongerWeb\\连环画\\人民出版社\\安徽人民出版社\\单本\\{feed.Title} {{seedid-{feed.Id}}}.torrent";
                var addTorrentJs = @$"const fileInput = document.querySelector('input[type=""file""]');
const myFile = new File([""content""], ""{torrentPath}"", {{
  type: ""text/plain"",
}});
const dataTransfer = new DataTransfer();
dataTransfer.items.add(myFile);
fileInput.files = dataTransfer.files;";
                #region 获得mapId
                //static uint GetMapId(DbSet<PtSiqiInfoMap> ptSiqiInfoMaps, string type, string elementName) => (from l in ptSiqiInfoMaps
                //                                                                                               where l.Group == type && l.ElementName == elementName
                //                                                                                               select l).First().ElementId;
                //var typeId = GetMapId(context.PtSiqiInfoMaps, "type", feed.Type);
                //var sortId = GetMapId(context.PtSiqiInfoMaps, "sort", feed.Sort);
                //var sourceId = GetMapId(context.PtSiqiInfoMaps, "source", feed.Source);
                //var conditionId = GetMapId(context.PtSiqiInfoMaps, "condition", feed.Condition);
                //var colorId = GetMapId(context.PtSiqiInfoMaps, "color", feed.Color);
                //var extensionId = GetMapId(context.PtSiqiInfoMaps, "extension", feed.Extension);
                //var languageId = GetMapId(context.PtSiqiInfoMaps, "language", feed.Language);
                //var qualityId = GetMapId(context.PtSiqiInfoMaps, "quality", feed.Quality);
                #endregion
                //生成JS
                var js = $@"document.getElementsByName(""name"")[0].value=""{feed.Title}"";
document.getElementsByName(""small_descr"")[0].value=""{smallDescr}"";
document.getElementsByName(""descr"")[0].value =""{descr}"";
document.getElementsByName(""type"")[0].value={feed.Type};
document.getElementsByName(""audiocodec_sel[4]"")[0].value={feed.Sort};
document.getElementsByName(""source_sel[4]"")[0].value={feed.Source};
document.getElementsByName(""medium_sel[4]"")[0].value={feed.Condition}
document.getElementsByName(""codec_sel[4]"")[0].value={feed.Color}
document.getElementsByName(""standard_sel[4]"")[0].value={feed.Extension};
document.getElementsByName(""processing_sel[4]"")[0].value={feed.Language};
document.getElementsByName(""customcat_sel[4]"")[0].value={feed.Quality};
document.getElementsByName(""team_sel[4]"")[0].value=6;
document.getElementsByName(""tags[4][]"")[1].checked=true;
document.getElementsByName(""tags[4][]"")[2].checked=true;
document.getElementsByName(""tags[4][]"")[3].checked=true;
document.getElementsByName(""tags[4][]"")[4].checked=true;
document.getElementsByName(""uplver"")[0].checked=false;;
//显示其它项
document.getElementsByClassName(""mode_4"")[0].style=""display"";
document.getElementsByClassName(""mode_4"")[1].style=""display"";";////添加种子  {addTorrentJs}
                Log.Information($"已生成{seedId}JS", seedId);
                return Ok(js);
            });
        }
        /// <summary>
        /// 后台上传发种
        /// </summary>
        /// <param name="token"></param>
        /// <param name="seedId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> SeedSiqiBackAsync(string token, int seedId)
        {
            return await CommonReturnAsync(token, async (eventBody) =>
            {
                using var context = DatabaseUtil.PooledDbContextFactory.CreateDbContext();
                await FeedPtSiqi.SeedSiqiBackAsync(GetPtSiqi(context, seedId), context, eventBody);
                return Ok();
            });
        }
        /// <summary>
        ///  整合各个子功能，一条龙发种思齐PT
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> AutoSeedSiqiAsync(string token)
        {
            return await CommonReturnAsync(token, async (eventBody) =>
            {
                var excelPath = FeedPtSiqi.Configuration.ExcelPath;
                if (!string.IsNullOrEmpty(eventBody))
                {
                    var paths = await ControllerUtil.GetPathsAsync(eventBody);
                    excelPath = paths.First();
                }
                //await FeedPtSiqi.AutoSeedSiqiAsync(excelPath);
                return Ok();
            });
        }

        /// <summary>
        /// 解析图书信息
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> ParseBookInfoAsync(string token)
        {
            return await CommonReturnAsync(token, async (eventBody) =>
            {
                await Task.CompletedTask;
                var bookInfo = HtmlParser.ParseBookInfo(eventBody);
                //var bookInfo = new BookInfoExtractor().ExtractFromHtml(eventBody);
                var bookInfoStr = bookInfo.ToString();
                return Ok(bookInfoStr);
            });
        }

        private static Feed GetPtSiqi(AutoFeedSiqiDbContext context, int seedId) => (from l in context.Feeds
                                                                                     where l.Id >= seedId
                                                                                     select l).ToList().First();
    }
}