using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Models.Events;
using BiliLite.Pages;
using BiliLite.Services;
using FFmpegInteropX;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/p/?LinkID=234238

namespace BiliLite
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    partial class ExtendedSplash : Page, ILogProvider
    {
        internal Rect splashImageRect; // Rect to store splash screen image coordinates.
        private SplashScreen splash; // Variable to hold the splash screen object.
        internal bool dismissed = false; // Variable to track splash screen dismissal status.

        public ExtendedSplash(SplashScreen splashScreen, bool loadState)
        {
            this.InitializeComponent();

            // Listen for window resize events to reposition the extended splash screen image accordingly.
            // This is important to ensure that the extended splash screen is formatted properly in response to snapping, unsnapping, rotation, etc...
            Window.Current.SizeChanged += new WindowSizeChangedEventHandler(ExtendedSplash_OnResize);

            splash = splashScreen;

            if (splash != null)
            {
                // Register an event handler to be executed when the splash screen has been dismissed.
                splash.Dismissed += new TypedEventHandler<SplashScreen, object>(DismissedEventHandler);

                // Retrieve the window coordinates of the splash screen image.
                splashImageRect = splash.ImageLocation;
                PositionImage();

                // Optional: Add a progress ring to your splash screen to show users that content is loading
                //PositionRing();
            }

            // Restore the saved session state if necessary
            //RestoreState(loadState);
        }

        void RestoreState(bool loadState)
        {
            if (loadState)
            {
                // TODO: write code to load state
            }
        }

        // Position the extended splash screen image in the same location as the system splash screen image.
        void PositionImage()
        {
            extendedSplashImage.SetValue(Canvas.LeftProperty, splashImageRect.X);
            extendedSplashImage.SetValue(Canvas.TopProperty, splashImageRect.Y);
            extendedSplashImage.Height = splashImageRect.Height;
            extendedSplashImage.Width = splashImageRect.Width;

        }

        //void PositionRing()
        //{
        //    loadingRing.SetValue(Canvas.LeftProperty, splashImageRect.X + splashImageRect.Width * 0.5 - loadingRing.Width * 0.5);
        //    loadingRing.SetValue(Canvas.TopProperty, splashImageRect.Y + splashImageRect.Height + splashImageRect.Height * 0.1);
        //}

        void ExtendedSplash_OnResize(object sender, WindowSizeChangedEventArgs e)
        {
            // Safely update the extended splash screen image coordinates. This function will be fired in response to snapping, unsnapping, rotation, etc...
            if (splash != null)
            {
                // Update the coordinates of the splash screen image.
                splashImageRect = splash.ImageLocation;
                PositionImage();
                //PositionRing();
            }
        }

        // Include code to be executed when the system has transitioned from the splash screen to the extended splash screen (application's first view).
        void DismissedEventHandler(SplashScreen sender, object e)
        {
            dismissed = true;

            // Complete app setup operations here...
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) => PreLoading();

        private static readonly ILogger logger = GlobalLogger.FromCurrentType();


        /// <summary>
        /// Initialize contents data and save as local file
        /// </summary>
        async void PreLoading()
        {
            App.ExtendAcrylicIntoTitleBar();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            App.Current.UnhandledException += App_UnhandledException;
            // RegisterExceptionHandlingSynchronizationContext();
            FFmpegInteropLogging.SetLogLevel(LogLevel.Info);
            FFmpegInteropLogging.SetLogProvider(this);
            // SqlHelper.InitDB();
            LogService.Init();
            RegisterService();
            OpenCCNET.ZhConverter.Initialize();

            //首次运行设置首页的显示样式
            if (SystemInformation.IsFirstRun)
            {
                var display = DisplayInformation.GetForCurrentView();
                if (display.ScreenWidthInRawPixels >= 1920 && (display.ScreenWidthInRawPixels / display.ScreenHeightInRawPixels > 16 / 9))
                {
                    //如果屏幕分辨率大于16：9,设置为List
                    SettingService.SetValue<int>(SettingConstants.UI.RECMEND_DISPLAY_MODE, 1);
                }
            }
            //圆角
            App.Current.Resources["ImageCornerRadius"] = new CornerRadius(SettingService.GetValue<double>(SettingConstants.UI.IMAGE_CORNER_RADIUS, 0));
            await AppHelper.SetRegions();
            await InitDb();
            try
            {
                var downloadService = App.ServiceProvider.GetRequiredService<DownloadService>();
                downloadService.LoadDownloading();
                downloadService.LoadDownloaded();
            }
            catch (Exception ex)
            {
                logger.Error("初始化加载下载视频错误", ex);
            }
            VideoPlayHistoryHelper.LoadABPlayHistories(true);

            DismissExtendedSplash();
        }

        private async Task InitDb()
        {
            var sqlMigrateService = App.ServiceProvider.GetRequiredService<SqlMigrateService>();
            await sqlMigrateService.MigrateDatabase();
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

        private void RegisterService()
        {
            try
            {
                var startup = new Startup();

                var hostBuilder = new HostBuilder()
                    .ConfigureServices(startup.ConfigureServices);
                App._host = hostBuilder.Build();
            }
            catch (Exception ex)
            {
                logger.Error("Start Host Error", ex);
            }
        }

        void DismissExtendedSplash()
        {
            var rootFrame = Window.Current.Content as Frame;
            var mainPage = App.ServiceProvider.GetRequiredService<IMainPage>();
            rootFrame.Content = mainPage;
            Window.Current.Content = rootFrame;
        }
    }
}