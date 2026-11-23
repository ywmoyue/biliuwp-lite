using System;
using BiliLite.Models.Common;

namespace BiliLite.Services
{
    /// <summary>
    /// 窗口尺寸能力接口。
    /// UWP 与 WinUI3 两个分支各自实现(UWP 使用 ApplicationView/Window.Current,
    /// WinUI3 使用 AppWindow),接口定义保持一致。
    /// </summary>
    public interface IWindowSizeProvider
    {
        /// <summary>
        /// 开始监听主窗口尺寸变化,应用启动时调用一次。
        /// </summary>
        void Initialize();

        /// <summary>
        /// 主窗口尺寸变化事件
        /// </summary>
        event EventHandler<WindowSize> SizeChanged;

        /// <summary>
        /// 主窗口所在显示区域的工作区尺寸,用于恢复时钳制
        /// </summary>
        WindowSize WorkAreaSize { get; }

        /// <summary>
        /// 窗口尺寸被持久化后调用。
        /// UWP 实现中用于更新系统"下次启动尺寸"偏好(ApplicationView.PreferredLaunchViewSize),
        /// 保证"重启应用"等功能重启后窗口尺寸不丢失;WinUI3 实现中通常无需处理。
        /// </summary>
        void OnWindowSizePersisted(WindowSize size);

        /// <summary>
        /// 应用启动时恢复窗口尺寸(需在窗口激活前调用)。
        /// UWP 实现中设置系统启动尺寸偏好;WinUI3 实现中直接调整主窗口尺寸。
        /// </summary>
        void RestoreWindowSize(WindowSize size);

        /// <summary>
        /// 窗口激活后精确校正窗口尺寸。
        /// 与保存尺寸使用同一度量(应用视图尺寸),避免启动尺寸偏好与实际窗口边界语义不一致
        /// 导致重启后窗口高度偏差(如差一个标题栏高度)。
        /// </summary>
        void ResizeWindow(WindowSize size);
    }
}