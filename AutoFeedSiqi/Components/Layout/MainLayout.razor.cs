using AutoFeedSiqi.Models.CommonComponents;
using AutoFeedSiqi.Models.CommonComponents.Model;
using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System.Diagnostics.CodeAnalysis;

namespace AutoFeedSiqi.Components.Layout
{
    public sealed partial class MainLayout
    {
        private bool UseTabSet { get; set; } = true;

        private string Theme { get; set; } = "";

        private bool IsFixedHeader { get; set; } = true;

        private bool IsFixedTabHeader { get; set; } = true;

        private bool IsFixedFooter { get; set; } = true;

        private bool IsFullSide { get; set; } = true;

        private bool ShowFooter { get; set; } = true;

        private bool ShowTabInHeader { get; set; } = true;

        private List<MenuItem>? Menus { get; set; }

        [SupplyParameterFromForm]
        public static Configuration Configuration { get; set; } = new();

        [Inject]
        [NotNull]
        private DialogService? DialogService { get; set; }

        [Inject]
        [NotNull]
        private MessageService? MessageService { get; set; }

        [NotNull]
        private Message? Message { get; set; }

        [Inject]
        [NotNull]
        private IThemeProvider? ThemeProvider { get; set; }

        /// <summary>
        /// OnInitialized 方法
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            Menus = GetIconSideMenuItems();

            using var context = DatabaseUtil.PooledDbContextFactory.CreateDbContext();
            Configuration = context.Configurations.FirstOrDefault() ?? new Configuration();
            //获得Configurations第二行数据
            //如果是开发环境，则使用第二行数据作为配置
            if (ConfigureProvider.Configuration["ASPNETCORE_ENVIRONMENT"] == "Development")
            {
                Configuration = context.Configurations.Skip(1).FirstOrDefault() ?? new Configuration();
            }
        }

        private static List<MenuItem> GetIconSideMenuItems()
        {
            var menus = new List<MenuItem>
            {
                new() { Text = "首页", Icon = "fa-solid fa-fw fa-flag", Url = "/" , Match = NavLinkMatch.All},
                new() { Text = "发种记录", Icon = "fa-solid fa-fw fa-users", Url = "/feed-history" }
            };

            return menus;
        }

        //获得发种身份
        private string GetIdentityVerification() => Configuration.SiqiUploaderToken == "d6k55a0lvancydsnnrgumxf7u8n56g52" ? "发种员" : "普通";

        //获得版本号
        private string GetVersion() => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

        private async Task NoRenderShowEditDialog()
        {
            var items = Utility.GenerateEditorItems<Configuration>();

            var item = items.First(i => i.GetFieldName() == nameof(Configuration.Id));
            item.Ignore = true;
            item = items.First(i => i.GetFieldName() == nameof(Configuration.ExcelPath));
            item.Ignore = true;
            item = items.First(i => i.GetFieldName() == nameof(Configuration.IskyToken));
            item.Ignore = true;
            item = items.First(i => i.GetFieldName() == nameof(Configuration.CreateAt));
            item.Ignore = true;
            item = items.First(i => i.GetFieldName() == nameof(Configuration.LastUpdatedAt));
            item.Ignore = true;
            item = items.First(i => i.GetFieldName() == nameof(Configuration.SiqiUploaderToken));
            item.PlaceHolder = "请输入 ...，非发种员请留空";
            item = items.First(i => i.GetFieldName() == nameof(Configuration.QbHost));
            item.PlaceHolder = "网址后面要加/api/v2,例如：http://192.168.0.118:9091/api/v2";

            var option = new EditDialogOption<Configuration>()
            {
                Title = "配置",
                Model = Configuration,
                Items = items,
                ItemsPerRow = 2,
                RowType = RowType.Inline,
                OnCloseAsync = () =>
                {
                    //NoRenderLogger.Log("close button is clicked");
                    return Task.CompletedTask;
                },
                OnEditAsync = async context =>
                {
                    using var contextDatabase = DatabaseUtil.PooledDbContextFactory.CreateDbContext();
                    contextDatabase.Configurations.Update((Configuration)context.Model);
                    contextDatabase.SaveChanges();

                    await MessageService.Show(new()
                    {
                        IsAutoHide = true,
                        Content = "配置已保存,请刷新页面！",
                        Color = BootstrapBlazor.Components.Color.Success,
                        Icon = "fa-solid fa-circle-info",
                        ShowDismiss = true,
                        OnDismiss = () =>
                        {
                            return Task.CompletedTask;
                        },
                        ShowBar = true,
                        ShowShadow = true,
                        ShowMode = MessageShowMode.Single,
                    }, Message);
                    return true;
                }
            };

            await DialogService.ShowEditDialog(option);
        }
    }
}
