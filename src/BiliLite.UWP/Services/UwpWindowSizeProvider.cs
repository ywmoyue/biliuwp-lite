using System;
using BiliLite.Models.Common;
using Windows.Foundation;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace BiliLite.Services
{
    /// <summary>
    /// UWP 平台窗口尺寸实现。
    /// 通过 Window.Current 监听尺寸变化,通过 ApplicationView 的系统启动尺寸偏好
    /// (PreferredLaunchViewSize)让重启后的新进程使用上次的窗口尺寸。
    /// 保存与恢复均以"应用视图尺寸"(ApplicationView.VisibleBounds / TryResizeView)为度量,
    /// 避免与窗口边界尺寸语义不一致导致重启后高度偏差。
    /// </summary>
    public class UwpWindowSizeProvider : IWindowSizeProvider
    {
        public event EventHandler<WindowSize> SizeChanged;

        /// <summary>
        /// 当前窗口所在显示器的工作区尺寸
        /// </summary>
        public WindowSize WorkAreaSize
        {
            get
            {
                var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
                return new WindowSize(bounds.Width, bounds.Height);
            }
        }

        /// <summary>
        /// 开始监听主窗口尺寸变化(应用启动时调用一次)
        /// </summary>
        public void Initialize()
        {
            Window.Current.SizeChanged -= Current_SizeChanged;
            Window.Current.SizeChanged += Current_SizeChanged;
        }

        private void Current_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            // 保存应用视图(可视内容区)尺寸,与恢复时 TryResizeView 的语义保持一致
            var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            SizeChanged?.Invoke(this, new WindowSize(bounds.Width, bounds.Height));
        }

        /// <summary>
        /// 窗口尺寸持久化后同步更新系统"下次启动尺寸"偏好
        /// </summary>
        public void OnWindowSizePersisted(WindowSize size)
        {
            SetPreferredLaunchSize(size);
        }

        /// <summary>
        /// 应用启动时恢复窗口尺寸,需在 Window.Current.Activate() 之前调用
        /// </summary>
        public void RestoreWindowSize(WindowSize size)
        {
            SetPreferredLaunchSize(size);
        }

        /// <summary>
        /// 窗口激活后精确校正窗口尺寸(与保存的视图尺寸度量一致)
        /// </summary>
        public void ResizeWindow(WindowSize size)
        {
            try
            {
                _ = ApplicationView.GetForCurrentView().TryResizeView(new Size(size.Width, size.Height));
            }
            catch (Exception)
            {
                // 平板模式等不支持调整时忽略
            }
        }

        private static void SetPreferredLaunchSize(WindowSize size)
        {
            try
            {
                ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
                ApplicationView.PreferredLaunchViewSize = new Size(size.Width, size.Height);
            }
            catch (Exception)
            {
                // 平板模式等不支持该系统偏好时忽略
            }
        }
    }
}