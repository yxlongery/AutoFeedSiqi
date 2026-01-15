using AutoFeedSiqi.Models.CommonComponents.Model;
using Newtonsoft.Json;

namespace AutoFeedSiqi.Models.CommonComponents
{
    public static class ControllerUtil
    {
        /// <summary>
        /// 获取路径列表
        /// </summary>
        /// <param name="eventBody"></param>
        /// <returns></returns>
        public static async Task<List<string>> GetPathsAsync(string eventBody)
        {
            await Task.CompletedTask;
            var commonModel = JsonConvert.DeserializeObject<CommonModel>(eventBody);
            return commonModel?.Paths?.ToList() ?? [];
        }
        /// <summary>
        /// 处理单个path
        /// </summary>
        /// <param name="eventBody"></param>
        /// <param name=""></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static async Task GetPathAsync(string eventBody, Func<string, Task<string>> func)
        {
            var paths = await GetPathsAsync(eventBody);
            foreach (var path in paths)
            {
                _ = await func(path);
            }
        }
    }
}
