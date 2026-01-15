using AutoFeedSiqi.Models.CommonComponents.EntityTypeConfiguration;
using AutoFeedSiqi.Models.CommonComponents.Model;
using Newtonsoft.Json;

namespace AutoFeedSiqi.Models.CommonComponents
{
    public static class SiqiUtil
    {
        /// <summary>
        /// 循环获取seedId
        /// </summary>
        /// <param name="eventBody"></param>
        /// <returns></returns>
        public static async Task SwitchPathOrSeedId(List<Feed> feeds, string? eventBody, Func<Feed, List<string>, Task<string>> UseSeedId)
        {
            var PtSiqiPaths = new PtSiqiPaths();
            if (!string.IsNullOrEmpty(eventBody))
            {
                PtSiqiPaths = JsonConvert.DeserializeObject<PtSiqiPaths>(eventBody);
            }

            foreach (var feed in feeds)
            {
                var seedId = (int)feed.Id;
                var paths = PtSiqiPaths?.Data?.FirstOrDefault(p => p.SeedId == seedId)?.Paths ?? [];
                await UseSeedId(feed, paths);
            }
        }
        /// <summary>
        /// 获得数据库内id对应的path
        /// </summary>
        /// <param name="seedId"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetIdPath(int seedId, AutoFeedSiqiDbContext context) => (from l in context.Feeds
                                                                                      where l.Id == seedId
                                                                                      select l.Path).ToList().First();
    }
}
