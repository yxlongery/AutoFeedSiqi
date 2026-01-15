using AutoFeedSiqi.Components;
using AutoFeedSiqi.Models.CommonComponents;
using Microsoft.AspNetCore.StaticFiles;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.ConfigureLog();
builder.Services.AddControllers();
builder.Services.AddBootstrapBlazor();
// 增加 Table Excel 导出服务
builder.Services.AddBootstrapBlazorTableExportService();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

#region UseSerilogRequestLogging
app.UseSerilogRequestLogging(options =>
{
    // Customize the message template
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

    options.EnrichDiagnosticContext = async (diagnosticContext, httpContext) =>
    {
        if (httpContext.Request.ContentLength.HasValue && httpContext.Request.ContentLength > 0)
        {
            using (StreamReader reader = new(httpContext.Request.Body))
            {
                var requestBody = await reader.ReadToEndAsync();
                diagnosticContext.Set("RequestBody", requestBody);
            }
        }
        diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress ?? httpContext.Connection.LocalIpAddress);
        diagnosticContext.Set("RemotePort", httpContext.Connection.RemotePort);
        diagnosticContext.Set("IsHttps", httpContext.Request.IsHttps);
        diagnosticContext.Set("ContentLength", httpContext.Request.ContentLength);
        diagnosticContext.Set("ContentType", httpContext.Request.ContentType);
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("PathBase", httpContext.Request.PathBase);
        diagnosticContext.Set("Protocol", httpContext.Request.Protocol);
        diagnosticContext.Set("QueryString", httpContext.Request.QueryString);
        diagnosticContext.Set("Query", httpContext.Request.Query.ToDictionary(x => x.Key, y => y.Value.ToString()));
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("Headers", httpContext.Request.Headers.ToDictionary(x => x.Key, y => y.Value.ToString()));
        diagnosticContext.Set("Cookies", httpContext.Request.Cookies.ToDictionary(x => x.Key, y => y.Value.ToString()));
    };
});
#endregion

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseRouting();//api
app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action}/{id?}");//api

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".moc"] = "application/x-msdownload";
provider.Mappings[".moc3"] = "application/x-msdownload";
provider.Mappings[".mtn"] = "application/x-msdownload";

app.UseStaticFiles(new StaticFileOptions { ContentTypeProvider = provider });

app.Run();
