using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Models.Events;
using BiliLite.Pages;
using BiliLite.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Graphics.Display;
using System.IO;

namespace BiliLite
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    public sealed partial class App : Application
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();
        private static IHost _host;
        private static MainWindow? _window;

        public static IServiceProvider ServiceProvider { get => _host.Services; }

        public static MainWindow MainWindow { get => _window; }

        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            this.UnhandledException += App_UnhandledException;
            // RegisterExceptionHandlingSynchronizationContext();

            LogService.Init();
            RegisterService();

            var dir = AppContext.BaseDirectory;

            var dictDir = Path.Combine(dir, "Dictionary");
            var resDir = Path.Combine(dir, "JiebaResource");

            OpenCCNET.ZhConverter.Initialize(dictDir, resDir);

            this.InitializeComponent();
        }

        private void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            logger.Log("错误发生", LogType.Trace, e.Exception);
        }

        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                logger.Log("程序运行出现错误", LogType.Error, ex);
            }
            else
            {
                logger.Log("程序运行出现错误:" + e.ExceptionObject, LogType.Error);
            }
        }

        private void RegisterExceptionHandlingSynchronizationContext()
        {
            ExceptionHandlingSynchronizationContext
                .Register()
                .UnhandledException += SynchronizationContext_UnhandledException;
        }

        private void SynchronizationContext_UnhandledException(object sender, AysncUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            try
            {
                if (e.Exception is NotImplementedException)
                {
                    logger.Log("功能未实现", LogType.Error, e.Exception);
                    NotificationShowExtensions.ShowMessageToast("功能未实现");
                }
                else
                {
                    logger.Log("程序运行出现错误", LogType.Error, e.Exception);
                    NotificationShowExtensions.ShowMessageToast("程序出现一个错误，已记录");
                }
            }
            catch (Exception)
            {
            }
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            try
            {
                if (e.Exception is NotImplementedException)
                {
                    logger.Log("功能未实现", LogType.Error, e.Exception);
                    NotificationShowExtensions.ShowMessageToast("功能未实现");
                }
                else
                {
                    logger.Log("程序运行出现错误", LogType.Error, e.Exception);
                    NotificationShowExtensions.ShowMessageToast("程序出现一个错误，已记录");
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="args">有关启动请求和过程的详细信息。</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            _window = new MainWindow();
            await Navigation(args.Arguments, false);
            _window.Activate();

            await LogService.DeleteExpiredLogFile();

#if !DEBUG
            await BiliExtensions.CheckVersion(null, isSilentUpdateCheck: true);
#endif
        }

        private async Task Navigation(object arguments, bool prelaunch = false)
        {
            try
            {
                var systemId = Windows.System.Profile.SystemIdentification.GetSystemIdForPublisher();
                var deviceId = Windows.Security.Cryptography.CryptographicBuffer.EncodeToHexString(systemId.Id).ToUpper();
                ApiHelper.deviceId = deviceId ?? "";
            }
            catch (Exception)
            {
            }

            await InitBili();

            WindowFrame rootFrame = _window.Content as WindowFrame;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (rootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new WindowFrame();
                rootFrame.CurrentWindow = _window;
                rootFrame.NavigationFailed += OnNavigationFailed;

                // 将框架放在当前窗口中
                _window.Content = rootFrame;
            }

            if (prelaunch == false)
            {
                if (rootFrame.Content == null)
                {
                    var mainPage = ServiceProvider.GetRequiredService<IMainPage>();
                    rootFrame.Content = mainPage;
                }

                var pageSaveService = ServiceProvider.GetRequiredService<PageSaveService>();
                await pageSaveService.HandleStartApp();

                if (arguments != null && !string.IsNullOrEmpty(arguments.ToString()))
                {
                    await MessageCenter.HandelUrl(arguments.ToString());
                }

                var themeService = ServiceProvider.GetRequiredService<ThemeService>();
                //themeService.InitTitleBar();
                themeService.InitAccentColor();
                // WinUI 3 中 Mica 的处理方式不同，可能需要调整
                // themeService.InitMicaBrushBackgroundSource();
                //themeService.InitStyle();

                var tempFileService = ServiceProvider.GetRequiredService<TempFileService>();
                tempFileService.ClearTempFiles();
            }
        }

        private async Task InitBili()
        {
            ////首次运行设置首页的显示样式
            //if (SystemInformation.IsFirstRun)
            //{
            //   // WinUI 3 中获取显示信息的方式可能需要调整
            //   var display = DisplayInformation.GetForCurrentView();
            //   if (display.ScreenWidthInRawPixels >= 1920 && (display.ScreenWidthInRawPixels / display.ScreenHeightInRawPixels > 16 / 9))
            //   {
            //       SettingService.SetValue<int>(SettingConstants.UI.RECMEND_DISPLAY_MODE, 1);
            //       // // 当导航堆栈尚未还原时，导航到第一页，
            //       // // 并通过将所需信息作为导航参数传入来配置参数
            //       // bool loadState = args.PreviousExecutionState == ApplicationExecutionState.Terminated;
            //       // ExtendedSplash extendedSplash = new(args.SplashScreen, loadState);
            //       // rootFrame.Content = extendedSplash;
            //       // Window.Current.Content = rootFrame;
            //       // await Task.Delay(200); // 防止初始屏幕闪烁
            //   }
            //}

            await AppHelper.SetRegions();
            await InitDb();

            try
            {
                var downloadService = ServiceProvider.GetRequiredService<DownloadService>();
                //downloadService.LoadDownloading();
                downloadService.LoadDownloaded();
            }
            catch (Exception ex)
            {
                logger.Error("初始化加载下载视频错误", ex);
            }

            VideoPlayHistoryHelper.LoadABPlayHistories(true);

            //var pluginService = ServiceProvider.GetRequiredService<PluginService>();
            //await pluginService.Start();
        }

        private async Task InitDb()
        {
            var sqlMigrateService = ServiceProvider.GetRequiredService<SqlMigrateService>();
            await sqlMigrateService.MigrateDatabase();
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        // WinUI 3 中移除了 Suspending 事件，需要调整应用生命周期管理
        // 可以考虑使用其他方式处理应用状态保存


        //protected override void OnActivated(IActivatedEventArgs args)
        //{
        //    base.OnActivated(args);

        //    if (args.Kind == ActivationKind.Protocol)
        //    {
        //        ProtocolActivatedEventArgs eventArgs = args as ProtocolActivatedEventArgs;
        //        Navigation(eventArgs.Uri.AbsoluteUri, false);
        //    }
        //}

        private void RegisterService()
        {
            try
            {
                var startup = new Startup();
                var hostBuilder = new HostBuilder()
                    .ConfigureServices(startup.ConfigureServices);
                _host = hostBuilder.Build();
            }
            catch (Exception ex)
            {
                logger.Error("Start Host Error", ex);
            }
        }

        // WinUI 3 中背景激活的处理方式不同
        // protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        // {
        //     await NotificationShowExtensions.ShowTile();
        // }
    }
}
