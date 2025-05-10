using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Services;
using BiliLite.Services.Biz;
using BiliLite.Services.Notification;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BiliLite
{
    public class Startup
    {
        public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            var displayMode = SettingService.GetValue<int>(SettingConstants.UI.DISPLAY_MODE, 0);
            services.AddDbContext<BiliLiteDbContext>();
            services.AddSingleton<SettingSqlService>();
            services.AddSingleton<PluginService>();
            services.AddTransient<SqlMigrateService>();

            services.AddSingleton<LiveTileService>();
            services.AddMapper();
            services.AddViewModels(displayMode);
            services.AddControls();
            services.AddDanmakuController();

            services.AddSingleton<DownloadService>();
            services.AddSingleton<CookieService>();
            services.AddSingleton<ShortcutKeyService>();
            services.AddTransient<PlayerToastService>();
            services.AddTransient<SettingsImportExportService>();
            services.AddQrCodeService();
            services.AddBizServices();
            services.AddSingleton<PlaySpeedMenuService>();
            services.AddSingleton<ContentFilterService>();
            services.AddSingleton<SearchService>();

            services.AddSingleton<GrpcService>();
            services.AddAttributeService(displayMode);
        }
    }
}
