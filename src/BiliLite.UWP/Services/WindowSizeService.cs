using System;
using System.Threading;
using BiliLite.Models.Common;

namespace BiliLite.Services
{
    /// <summary>
    /// 窗口尺寸保存与恢复服务(平台无关,通过 IWindowSizeProvider 接入平台差异)。
    /// 监听主窗口尺寸变化并持久化,应用启动时恢复上次保存的尺寸,
    /// 修复"重启应用"等功能重启后窗口恢复默认大小的问题。
    /// </summary>
    public class WindowSizeService
    {
        /// <summary>
        /// 拖动窗口过程中尺寸变化事件高频触发,防抖间隔
        /// </summary>
        private const int SAVE_DEBOUNCE_MILLISECONDS = 500;

        private readonly IWindowSizeProvider m_windowProvider;
        private readonly Timer m_saveTimer;
        private readonly object m_sync = new object();

        private WindowSize m_latestSize;

        public WindowSizeService(IWindowSizeProvider windowProvider)
        {
            m_windowProvider = windowProvider;
            m_windowProvider.SizeChanged += WindowProvider_SizeChanged;
            m_saveTimer = new Timer(OnSaveTimerElapsed, null, Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// 应用启动时调用(需在窗口激活前),恢复上次保存的窗口尺寸。
        /// </summary>
        public void RestoreWindowSize()
        {
            WindowSize size;
            if (!TryGetSavedWindowSize(out size))
            {
                return;
            }

            m_windowProvider.RestoreWindowSize(size);
        }

        /// <summary>
        /// 窗口激活后调用,精确校正一次窗口尺寸。
        /// 启动尺寸偏好与实际窗口边界可能差一个标题栏高度,激活后按保存时的
        /// 应用视图尺寸再校正一次,保证所见即所得。
        /// </summary>
        public void ApplySavedWindowSize()
        {
            WindowSize size;
            if (!TryGetSavedWindowSize(out size))
            {
                return;
            }

            m_windowProvider.ResizeWindow(size);
        }

        private bool TryGetSavedWindowSize(out WindowSize size)
        {
            size = new WindowSize(0, 0);
            try
            {
                if (!SettingService.HasValue(SettingConstants.UI.MAIN_WINDOW_WIDTH) || !SettingService.HasValue(SettingConstants.UI.MAIN_WINDOW_HEIGHT))
                {
                    return false;
                }

                var width = SettingService.GetValue(SettingConstants.UI.MAIN_WINDOW_WIDTH, 0d);
                var height = SettingService.GetValue(SettingConstants.UI.MAIN_WINDOW_HEIGHT, 0d);
                if (width <= 0 || height <= 0)
                {
                    return false;
                }

                // 恢复的尺寸上限钳制到当前工作区,避免窗口超出屏幕
                var workArea = m_windowProvider.WorkAreaSize;
                if (workArea.Width > 0 && workArea.Height > 0)
                {
                    width = Math.Min(width, workArea.Width);
                    height = Math.Min(height, workArea.Height);
                }

                size = new WindowSize(width, height);
                return true;
            }
            catch (Exception)
            {
                // 恢复失败时保持系统默认窗口尺寸,不影响应用启动
                return false;
            }
        }

        private void WindowProvider_SizeChanged(object sender, WindowSize e)
        {
            lock (m_sync)
            {
                m_latestSize = e;
            }
            m_saveTimer.Change(SAVE_DEBOUNCE_MILLISECONDS, Timeout.Infinite);
        }

        private void OnSaveTimerElapsed(object state)
        {
            WindowSize size;
            lock (m_sync)
            {
                size = m_latestSize;
            }

            try
            {
                SettingService.SetValue(SettingConstants.UI.MAIN_WINDOW_WIDTH, size.Width);
                SettingService.SetValue(SettingConstants.UI.MAIN_WINDOW_HEIGHT, size.Height);
                m_windowProvider.OnWindowSizePersisted(size);
            }
            catch (Exception)
            {
                // 持久化失败不影响应用运行
            }
        }
    }
}