using System;
using BiliLite.Models.Common;
using Microsoft.UI.Windowing;
using Windows.Graphics;

namespace BiliLite.Services
{
    /// <summary>
    /// WinUI3 平台窗口尺寸实现(基于 AppWindow)。
    /// 通过 AppWindow.Changed 监听尺寸变化,启动时通过 AppWindow.ResizeClient 恢复上次尺寸。
    /// 保存与恢复均以客户区尺寸(AppWindow.ClientSize / ResizeClient)为度量,
    /// 与用户看到的窗口内容区一致,避免重启后高度偏差。
    /// </summary>
    public class WinUIWindowSizeProvider : IWindowSizeProvider
    {
        private AppWindow m_appWindow;

        public event EventHandler<WindowSize> SizeChanged;

        /// <summary>
        /// 主窗口所在显示区域的工作区尺寸
        /// </summary>
        public WindowSize WorkAreaSize
        {
            get
            {
                if (m_appWindow == null)
                {
                    return new WindowSize(0, 0);
                }

                var workArea = DisplayArea.GetFromWindowId(m_appWindow.Id, DisplayAreaFallback.Primary).WorkArea;
                return new WindowSize(workArea.Width, workArea.Height);
            }
        }

        /// <summary>
        /// 开始监听主窗口尺寸变化(应用启动时调用一次)
        /// </summary>
        public void Initialize()
        {
            var window = App.MainWindow;
            m_appWindow = window.AppWindow;
            m_appWindow.Changed -= AppWindow_Changed;
            m_appWindow.Changed += AppWindow_Changed;
        }

        private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs e)
        {
            if (e.DidSizeChange)
            {
                // 保存客户区(内容区)尺寸,与恢复时 ResizeClient 的语义保持一致
                var size = m_appWindow.ClientSize;
                SizeChanged?.Invoke(this, new WindowSize(size.Width, size.Height));
            }
        }

        /// <summary>
        /// WinUI3 桌面应用没有"下次启动尺寸"的系统偏好,重启后的尺寸由启动时
        /// <see cref="RestoreWindowSize"/> 直接恢复,此处无需处理。
        /// </summary>
        public void OnWindowSizePersisted(WindowSize size)
        {
        }

        /// <summary>
        /// 应用启动时恢复窗口尺寸,需在窗口 Activate 之前调用
        /// </summary>
        public void RestoreWindowSize(WindowSize size)
        {
            ResizeWindow(size);
        }

        /// <summary>
        /// 窗口激活后精确校正窗口尺寸(与保存的客户区尺寸度量一致)
        /// </summary>
        public void ResizeWindow(WindowSize size)
        {
            if (m_appWindow == null)
            {
                return;
            }

            try
            {
                m_appWindow.ResizeClient(new SizeInt32((int)Math.Round(size.Width), (int)Math.Round(size.Height)));
            }
            catch (Exception)
            {
                // 调整失败时保持默认尺寸
            }
        }
    }
}