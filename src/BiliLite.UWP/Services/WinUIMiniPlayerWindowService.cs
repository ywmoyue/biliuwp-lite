using System;
using BiliLite.Models.Common.Player;
using BiliLite.Pages;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Windows.Graphics;
using WinUIEx;

namespace BiliLite.Services
{
    /// <summary>
    /// WinUI3 分支的小窗置顶实现：独立置顶窗口 + 独立播放器实例 + 播放交接。
    /// 播放面（WebView2 / MediaPlayerElement）无法跨窗口搬移，所以小窗不能复用主窗口的播放器，
    /// 改为在独立窗口里新建播放器，通过 <see cref="PlayerHandoffInfo"/> 交接进度。
    /// UWP 分支可用 ApplicationView.CompactOverlay 在同一窗口内实现同一接口。
    /// </summary>
    public class WinUIMiniPlayerWindowService : IPlayerMiniWindowService
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        private const string SETTING_WIDTH = "MiniPlayerWindowWidth";
        private const string SETTING_HEIGHT = "MiniPlayerWindowHeight";
        private const double DEFAULT_WIDTH = 480;
        private const double DEFAULT_HEIGHT = 270;

        /// <summary>
        /// 小窗与屏幕边缘的间距
        /// </summary>
        private const int SCREEN_MARGIN = 24;

        private WindowEx? m_window;
        private MiniPlayerPage? m_page;
        private IPlayerMiniWindowHost? m_owner;
        private bool m_isClosing;
        private bool m_suppressResume;

        public bool IsSupported => true;

        public bool IsActive => m_window != null;

        public bool RequiresPlaybackHandoff => true;

        public void Enter(PlayerHandoffInfo handoff, IPlayerMiniWindowHost owner)
        {
            if (m_window != null)
            {
                Activate();
                return;
            }

            m_owner = owner;
            m_suppressResume = false;

            try
            {
                var window = new WindowEx();
                var frame = new WindowFrame { CurrentWindow = window };
                window.Content = frame;
                window.Title = "小窗播放";
                window.IsAlwaysOnTop = true;

                ApplyInitialBounds(window);

                window.Closed += Window_Closed;
                m_window = window;

                window.Show();

                frame.Navigate(typeof(MiniPlayerPage), handoff);
                m_page = frame.Content as MiniPlayerPage;

                // 主窗口关闭时小窗一起关，避免留下孤立窗口
                var mainWindow = App.MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.Closed -= MainWindow_Closed;
                    mainWindow.Closed += MainWindow_Closed;
                }

                _logger.Info($"小窗已打开，交接位置={handoff.Position}");
            }
            catch (Exception ex)
            {
                _logger.Error("打开小窗失败", ex);
                Cleanup();
            }
        }

        public void Exit()
        {
            if (m_window == null || m_isClosing)
            {
                return;
            }

            CloseWindow();
        }

        public void CloseWithoutResume()
        {
            m_suppressResume = true;
            if (m_window == null || m_isClosing)
            {
                return;
            }

            CloseWindow();
        }

        private void CloseWindow()
        {
            var window = m_window;
            if (window == null)
            {
                m_isClosing = false;
                return;
            }

            m_isClosing = true;
            try
            {
                // 窗口关闭后 AppWindow 可能已经失效，先在还活着的时候记下尺寸
                PersistBounds();
                // 关闭动作会触发 Window_Closed，在那里回传状态并清理
                window.Close();
            }
            catch (Exception ex)
            {
                _logger.Error("关闭小窗失败", ex);
                Cleanup();
            }
        }

        private void Activate()
        {
            if (m_window == null)
            {
                return;
            }

            m_window.Activate();
            m_window.SetForegroundWindow();
        }

        /// <summary>
        /// 首次显示时的尺寸与位置：沿用上次尺寸，贴工作区右下角
        /// </summary>
        private void ApplyInitialBounds(WindowEx window)
        {
            var appWindow = window.AppWindow;
            var width = (int)Math.Round(SettingService.GetValue<double>(SETTING_WIDTH, DEFAULT_WIDTH));
            var height = (int)Math.Round(SettingService.GetValue<double>(SETTING_HEIGHT, DEFAULT_HEIGHT));
            appWindow.ResizeClient(new SizeInt32(width, height));

            var workArea = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary).WorkArea;
            var x = workArea.X + workArea.Width - width - SCREEN_MARGIN;
            var y = workArea.Y + workArea.Height - height - SCREEN_MARGIN;
            // 防止尺寸大于工作区时落到屏幕外
            x = Math.Max(workArea.X, x);
            y = Math.Max(workArea.Y, y);
            appWindow.Move(new PointInt32(x, y));
        }

        private void Window_Closed(object sender, WindowEventArgs args)
        {
            var owner = m_owner;
            var exitInfo = m_page?.LastState;
            var dispatcher = m_window?.DispatcherQueue;

            PersistBounds();
            Cleanup();

            if (owner == null || exitInfo == null)
            {
                m_suppressResume = false;
                return;
            }

            // 延后一拍再回调：页面销毁路径会在关闭小窗之后紧接着调用 CloseWithoutResume，
            // 先让那次调用把"不要接回播放"的意图登记下来
            void ResumeOnOwner()
            {
                var suppress = m_suppressResume;
                m_suppressResume = false;
                if (suppress)
                {
                    return;
                }

                owner.ResumeFromHandoff(exitInfo);
            }

            if (dispatcher == null || !dispatcher.TryEnqueue(DispatcherQueuePriority.Normal, ResumeOnOwner))
            {
                ResumeOnOwner();
            }
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            // 主窗口关掉了，小窗没有必要再"回到主窗口"
            m_owner = null;
            Exit();
        }

        private void PersistBounds()
        {
            try
            {
                var appWindow = m_window?.AppWindow;
                if (appWindow == null)
                {
                    return;
                }

                var size = appWindow.ClientSize;
                if (size.Width <= 0 || size.Height <= 0)
                {
                    return;
                }

                SettingService.SetValue<double>(SETTING_WIDTH, size.Width);
                SettingService.SetValue<double>(SETTING_HEIGHT, size.Height);
            }
            catch (Exception ex)
            {
                _logger.Debug("保存小窗尺寸失败", ex);
            }
        }

        private void Cleanup()
        {
            var mainWindow = App.MainWindow;
            if (mainWindow != null)
            {
                mainWindow.Closed -= MainWindow_Closed;
            }

            if (m_window != null)
            {
                m_window.Closed -= Window_Closed;
            }

            m_window = null;
            m_page = null;
            m_owner = null;
            m_isClosing = false;
        }
    }
}
