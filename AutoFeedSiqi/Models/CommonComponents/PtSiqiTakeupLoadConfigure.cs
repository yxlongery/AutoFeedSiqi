using AutoFeedSiqi.Models.CommonComponents.Model;

namespace AutoFeedSiqi.Models.CommonComponents
{
    public static class PtSiqiTakeupLoadConfigure
    {
        public static Configuration FeedConfiguration { get; set; } = ConfigureProvider.Configuration.GetSection("FeedConfiguration").Get<Configuration>() ?? new Configuration();
    }
}
