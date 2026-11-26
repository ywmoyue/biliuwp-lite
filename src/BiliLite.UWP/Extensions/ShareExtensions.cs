using BiliLite.Extensions.Notifications;
using BiliLite.Services;
using Microsoft.UI.Xaml;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;

namespace BiliLite.Extensions
{
    /// <summary>
    /// IDataTransferManagerInterop，桌面宿主通过窗口句柄获取分享管理器
    /// </summary>
    [ComImport]
    [Guid("3A3DCD6C-3EAB-43DC-BCDE-45671CE800C8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IDataTransferManagerInterop
    {
        IntPtr GetForWindow([In] IntPtr appWindow, [In] ref Guid riid);

        void ShowShareUIForWindow(IntPtr appWindow);
    }

    /// <summary>
    /// 系统分享（Windows 分享面板）。
    /// DataTransferManager.GetForCurrentView / ShowShareUI 是 UWP 独占 API，
    /// WinUI3 桌面宿主调用会抛异常，必须改走 IDataTransferManagerInterop 的按窗口方式。
    /// </summary>
    public static class ShareExtensions
    {
        // IDataTransferManager 的 IID，作为 GetForWindow 的 riid
        private static readonly Guid _dataTransferManagerIid = new Guid("A5CAEE9B-8708-49D1-8D36-67D25A8DA00C");

        private static readonly GlobalLogger _logger = GlobalLogger.FromCurrentType();

        // 分享管理器是按窗口分配的，按窗口缓存投影对象，保证订阅/退订作用在同一个对象上
        private static readonly ConditionalWeakTable<Window, WindowShareState> _shareStates = new();

        private sealed class WindowShareState
        {
            public IDataTransferManagerInterop Interop { get; init; } = null!;

            public IntPtr HWnd { get; init; }

            public DataTransferManager Manager { get; init; } = null!;

            public TypedEventHandler<DataTransferManager, DataRequestedEventArgs>? Handler { get; set; }
        }

        /// <summary>
        /// 弹出系统分享面板，由 <paramref name="dataRequestedHandler"/> 负责填充分享内容。
        /// 只能在本窗口已加载的元素上调用（需要 XamlRoot 才能拿到窗口句柄）。
        /// </summary>
        public static void ShowShareUI(this FrameworkElement element,
            TypedEventHandler<DataTransferManager, DataRequestedEventArgs> dataRequestedHandler)
        {
            if (element == null || dataRequestedHandler == null) return;

            var window = element.GetCurrentWindow();
            if (window == null)
            {
                _logger.Warn("无法获取当前窗口，系统分享不可用");
                NotificationShowExtensions.ShowMessageToast("系统分享暂时不可用");
                return;
            }

            try
            {
                var state = _shareStates.GetValue(window, CreateState);

                // 上一次的请求如果没有被回调，先摘掉，避免下次分享时重复构造内容
                var previous = state.Handler;
                if (previous != null)
                {
                    state.Manager.DataRequested -= previous;
                    state.Handler = null;
                }

                TypedEventHandler<DataTransferManager, DataRequestedEventArgs>? handler = null;
                handler = (sender, args) =>
                {
                    state.Handler = null;
                    try
                    {
                        dataRequestedHandler(sender, args);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("构造分享内容失败", ex);
                    }
                    finally
                    {
                        if (handler != null) sender.DataRequested -= handler;
                    }
                };

                state.Handler = handler;
                state.Manager.DataRequested += handler;

                state.Interop.ShowShareUIForWindow(state.HWnd);
            }
            catch (Exception ex)
            {
                _logger.Error("弹出系统分享面板失败", ex);
                NotificationShowExtensions.ShowMessageToast("系统分享暂时不可用");
            }
        }

        private static WindowShareState CreateState(Window window)
        {
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            var interop = DataTransferManager.As<IDataTransferManagerInterop>();
            var iid = _dataTransferManagerIid;
            var manager = DataTransferManager.FromAbi(interop.GetForWindow(hWnd, ref iid));
            return new WindowShareState
            {
                Interop = interop,
                HWnd = hWnd,
                Manager = manager,
            };
        }
    }
}
