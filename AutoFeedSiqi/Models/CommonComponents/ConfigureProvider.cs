using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Exceptions;

namespace AutoFeedSiqi.Models.CommonComponents
{
    public static class ConfigureProvider
    {
        public static IConfiguration Configuration { get; set; } = ReadConfiguration();

        public static bool IsLinuxOperatingSystem { get; } = Configuration["Operating_System"] == "Linux";

        /// <summary>
        /// AspNetCore读取
        /// </summary>
        /// <param name="builder"></param>
        public static void ConfigureLog(this WebApplicationBuilder builder)
        {
            ConfigureLog();
            builder.Host.UseSerilog(
                dispose: true,
                logger: Log.Logger);
        }
        /// <summary>
        /// 通用配置
        /// </summary>
        public static void ConfigureLog()
        {
            var levelSwitch = new LoggingLevelSwitch();
            var seqApiKey = Configuration["Seq_apiKey"] ?? string.Empty;
            var seqServerUrl = Configuration["Seq_serverUrl"] ?? string.Empty;
            var loggerConfiguration = new LoggerConfiguration()
                 .ReadFrom.Configuration(Configuration)
                 .Enrich.FromLogContext()//使用Serilog.Context.LogContext中的属性丰富日志事件。配置Enrich.FromLogContext()的目的是为了从日志上下文中获取一些关键信息诸如用户ID或请求ID。
                 .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                 .MinimumLevel.ControlledBy(levelSwitch)
                 .Enrich.WithExceptionDetails()
                 .WriteTo.File(
                     path: "logs/.log",
                     outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {SourceContext} {ThreadId} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                     rollingInterval: RollingInterval.Day,
                     retainedFileCountLimit: 100,
                     restrictedToMinimumLevel: LogEventLevel.Verbose);
            // 配置同时输出到控制台和文件，并且指定文件名和文件转储方式（形如log-20211219.txt格式），转储文件保留的天数为15天，以及日志格式
            loggerConfiguration
                  .WriteTo.Console(
                      restrictedToMinimumLevel: LogEventLevel.Information,
                      outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {SourceContext} {ThreadId} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                  .WriteTo.Debug(
                      restrictedToMinimumLevel: LogEventLevel.Verbose,
                      outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {SourceContext} {ThreadId} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
            Log.Logger = loggerConfiguration.CreateLogger();//清除内置日志框架
        }

        #region private method
        /// <summary>
        /// 读取配置
        /// </summary>
        /// <returns></returns>
        private static IConfiguration ReadConfiguration()
        {
            using var host = Host.CreateDefaultBuilder().Build();
            var configurationBuilder = host.Services.GetRequiredService<IConfiguration>();
            return configurationBuilder;
        }
        #endregion
    }
}
