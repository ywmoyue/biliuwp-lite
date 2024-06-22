using BiliLite.Extensions;
using BiliLite.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BiliLite
{
    public class Startup
    {
        public void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddMapper();
            services.AddViewModels();
            services.AddControls();
            services.AddDanmakuController();

            services.AddSingleton<CookieService>();
            services.AddSingleton<ShortcutKeyService>();
            services.AddTransient<PlayerToastService>();
            services.AddTransient<SettingsImportExportService>();
            services.AddQrCodeService();
            services.AddBizServices();
            services.AddSingleton<PlaySpeedMenuService>();
            
            services.AddSingleton<GrpcService>();
        }
    }
}
