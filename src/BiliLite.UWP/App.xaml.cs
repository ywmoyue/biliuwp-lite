using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Models.Events;
using BiliLite.Pages;
using BiliLite.Services;
using FFmpegInteropX;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace BiliLite
{
    /// <summary>
    /// 提供特定于应用程序的行为，以补充默认的应用程序类。
    /// </summary>
    sealed partial class App : Application, ILogProvider
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();
        private static IHost _host;

        public static IServiceProvider ServiceProvider { get => _host.Services; }

        /// <summary>
        /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
        /// 已执行，逻辑上等同于 main() 或 WinMain()。
        /// </summary>
        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException; ;
            App.Current.UnhandledException += App_UnhandledException;
            // RegisterExceptionHandlingSynchronizationContext();
            FFmpegInteropLogging.SetLogLevel(LogLevel.Info);
            FFmpegInteropLogging.SetLogProvider(this);
            // SqlHelper.InitDB();
            LogService.Init();
            RegisterService();
            OpenCCNET.ZhConverter.Initialize();
            this.Suspending += OnSuspending;
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
                    Notify.ShowMessageToast("功能未实现");
                }
                else
                {
                    logger.Log("程序运行出现错误", LogType.Error, e.Exception);
                    Notify.ShowMessageToast("程序出现一个错误，已记录");
                }
            }
            catch (Exception)
            {
            }
        }
        private void App_UnhandledException(object sender, Windows.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            try
            {
                if (e.Exception is NotImplementedException)
                {
                    logger.Log("功能未实现", LogType.Error, e.Exception);
                    Notify.ShowMessageToast("功能未实现");
                }
                else
                {
                    logger.Log("程序运行出现错误", LogType.Error, e.Exception);
                    Notify.ShowMessageToast("程序出现一个错误，已记录");
                }
            }
            catch (Exception)
            {
            }

        }

        public void Log(LogLevel level, string message)
        {
            logger.Trace($"FFmpeg ({level}): {message}");
        }
        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {

            Navigation(e.Arguments, e.PrelaunchActivated);
            await LogService.DeleteExpiredLogFile();
        }


        private async void Navigation(object arguments, bool prelaunch = false)
        {
            // We don't have ARM64 support of SYEngine.
            if (RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
            {
                SYEngine.Core.Initialize();
            }
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
            Frame rootFrame = Window.Current.Content as Frame;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (rootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                //主题颜色
                rootFrame.RequestedTheme = (ElementTheme)SettingService.GetValue<int>(SettingConstants.UI.THEME, 0);

                // 将框架放在当前窗口中
                Window.Current.Content = rootFrame;
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
                // 确保当前窗口处于活动状态
                Window.Current.Activate();

                var themeService = ServiceProvider.GetRequiredService<ThemeService>();
                themeService.InitTitleBar();
                themeService.InitAccentColor();
            }
        }

        private async Task InitBili()
        {
            //首次运行设置首页的显示样式
            if (SystemInformation.IsFirstRun)
            {
                var display = DisplayInformation.GetForCurrentView();
                if (display.ScreenWidthInRawPixels >= 1920 && (display.ScreenWidthInRawPixels / display.ScreenHeightInRawPixels > 16 / 9))
                {
                    //如果屏幕分辨率大于16：9,设置为List
                    SettingService.SetValue<int>(SettingConstants.UI.RECMEND_DISPLAY_MODE, 1);
                    // // 当导航堆栈尚未还原时，导航到第一页，
                    // // 并通过将所需信息作为导航参数传入来配置参数
                    // bool loadState = args.PreviousExecutionState == ApplicationExecutionState.Terminated;
                    // ExtendedSplash extendedSplash = new(args.SplashScreen, loadState);
                    // rootFrame.Content = extendedSplash;
                    // Window.Current.Content = rootFrame;
                    // await Task.Delay(200); // 防止初始屏幕闪烁
                }
            }

            //圆角
            App.Current.Resources["ImageCornerRadius"] = new CornerRadius(SettingService.GetValue<double>(SettingConstants.UI.IMAGE_CORNER_RADIUS, 0));
            await AppHelper.SetRegions();
            await InitDb();

            try
            {
                var downloadService = ServiceProvider.GetRequiredService<DownloadService>();
                downloadService.LoadDownloading();
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

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            base.OnActivated(args);

            if (args.Kind == ActivationKind.Protocol)
            {
                ProtocolActivatedEventArgs eventArgs = args as ProtocolActivatedEventArgs;
                Navigation(eventArgs.Uri.AbsoluteUri, false);
            }

        }

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

        protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            //base.OnBackgroundActivated(args);
            //IBackgroundTaskInstance taskInstance = args.TaskInstance;
            await NotificationShowExtensions.Tile();
        }
    }
}