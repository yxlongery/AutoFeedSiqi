using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Context;

namespace AutoFeedSiqi.Models.CommonComponents
{
    /// <summary>
    /// 基础类控制器
    /// </summary>
    public class BaseController : Controller
    {
        /// <summary>
        /// 获得EventBody
        /// </summary>
        /// <returns></returns>
        protected async Task<string> GetEventBodyAsync()
        {
            var eventBody = string.Empty;
            if (Request.ContentLength.HasValue && Request.ContentLength > 0)
            {
                Request.EnableBuffering();
                using (StreamReader reader = new(Request.Body, leaveOpen: true))
                {
                    eventBody = await reader.ReadToEndAsync();
                    using (LogContext.PushProperty("EventBody", eventBody))
                    {
                        Log.Information("收到一个Event");
                    }
                }
                Request.Body.Position = 0;
            }
            return eventBody;
        }
        /// <summary>
        /// 通用的返回方法
        /// </summary>
        /// <param name="token">token</param>
        /// <param name="func"></param>
        /// <returns></returns>
        protected async Task<ActionResult> CommonReturnAsync(string token, Func<string, Task<ActionResult>> func)
        {
            if (token == $"siqi{DateTime.Now:yyyy}")
            {
                Log.Information($"请求token合法：{{{nameof(token)}}}", token);
                try
                {
                    var eventBody = await GetEventBodyAsync();
                    if (eventBody.Contains("\"challenge\":"))
                    {
                        return Ok(eventBody);
                    }
                    else
                    {
                        Log.Verbose("开始HandleEventAsync");
                        return await func(eventBody);
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"Source:{e.Source}");
                    Log.Error($"TargetSite:{e.TargetSite}");
                    Log.Error($"InnerExceptionMessage:{e.InnerException?.Message}");
                    Log.Error($"StackTrace:\r\n{e.StackTrace}");
                    var message = e.Message;
                    using (LogContext.PushProperty("异常信息", e, true))
                    {
                        Log.Warning($"提示：{{{nameof(message)}}}", message);
                        return BadRequest(e.Message);
                    }
                }
            }
            else
            {
                Log.Error($"请求token不合法：{{{nameof(token)}}}", token);
                return NotFound($"请求token不合法：{token}");
            }
        }
    }
}
