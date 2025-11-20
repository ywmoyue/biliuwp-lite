using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Services;
using BiliLite.Services.Biz;
using BiliLite.Services.Notification;
using CommunityToolkit.WinUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Runtime.CompilerServices;
using WinUIEx;

namespace BiliLite
{
    public class Startup
    {
        /// <summary>
        /// 初始化应用级别的依赖注入
        /// </summary>
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

    public static class HostExtensions
    {
        // 应用级别的服务提供者
        private static IServiceProvider? _applicationServiceProvider;

        // 为每个窗口存储服务作用域
        private static readonly ConditionalWeakTable<Window, IServiceScope> _windowScopes = new();

        // 为每个页面存储服务作用域
        private static readonly ConditionalWeakTable<Page, IServiceScope> _pageScopes = new();

        public static IServiceProvider ApplicationServiceProvider =>
            _applicationServiceProvider ?? throw new InvalidOperationException("DI system not initialized");

        /// <summary>
        /// 为窗口创建服务作用域
        /// </summary>
        public static void CreateWindowScope(this Window window)
        {
            if (_windowScopes.TryGetValue(window, out _))
            {
                return;
            }

            var windowScope = ApplicationServiceProvider.CreateScope();
            _windowScopes.Add(window, windowScope);

            // 当窗口关闭时释放作用域
            window.Closed += (sender, args) =>
            {
                _windowScopes.Remove(window);
                windowScope.Dispose();
            };
        }

        /// <summary>
        /// 为页面创建服务作用域
        /// </summary>
        public static void CreatePageScope(this Page page)
        {
            if (_pageScopes.TryGetValue(page, out _))
            {
                return;
            }

            var window = GetWindowFromPage(page);
            if (window == null || !_windowScopes.TryGetValue(window, out var windowScope))
            {
                throw new InvalidOperationException("Window scope not found for page");
            }

            var pageScope = windowScope.ServiceProvider.CreateScope();
            _pageScopes.Add(page, pageScope);

            // 当页面被移除时释放作用域
            page.Unloaded += (sender, args) =>
            {
                _pageScopes.Remove(page);
                pageScope.Dispose();
            };
        }

        /// <summary>
        /// 获取Window对象
        /// </summary>
        public static Window? GetWindowFromPage(Page page)
        {
            return (page.XamlRoot?.Content as WindowFrame)?.CurrentWindow;
        }

        /// <summary>
        /// 获取Window对象
        /// </summary>
        public static WindowEx? GetCurrentWindow(this FrameworkElement element)
        {
            return (element.XamlRoot?.Content as WindowFrame)?.CurrentWindow;
        }

        /// <summary>
        /// 从窗口获取服务
        /// </summary>
        public static T GetService<T>(this WindowEx window) where T : class
        {
            if (_windowScopes.TryGetValue(window, out var windowScope))
            {
                return windowScope.ServiceProvider.GetRequiredService<T>();
            }

            return ApplicationServiceProvider.GetRequiredService<T>();
        }

        /// <summary>
        /// 从控件获取服务
        /// </summary>
        public static T GetService<T>(this FrameworkElement element) where T : class
        {
            if (element is Page page && _pageScopes.TryGetValue(page, out var pageScope))
            {
                return pageScope.ServiceProvider.GetRequiredService<T>();
            }

            var current = element.FindAscendant<Page>();
            if (current != null && _pageScopes.TryGetValue(current, out var parentPageScope))
            {
                return parentPageScope.ServiceProvider.GetRequiredService<T>();
            }

            var windowFromElement = (element.XamlRoot?.Content as WindowFrame)?.CurrentWindow;
            if (windowFromElement != null && _windowScopes.TryGetValue(windowFromElement, out var elementWindowScope))
            {
                return elementWindowScope.ServiceProvider.GetRequiredService<T>();
            }

            return ApplicationServiceProvider.GetRequiredService<T>();
        }
    }
}