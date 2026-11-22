using BiliLite.Extensions;
using BiliLite.Services.Biz;
using System;
using System.Linq;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace BiliLite.Controls.Search
{
    /// <summary>
    /// 给 AutoSuggestBox 挂上搜索历史面板：输入框为空时弹出历史，输入内容时交回联想词
    /// </summary>
    /// <remarks>
    /// 1. 面板用 Popup 而不是 Flyout：Flyout 一旦失去焦点就会被系统关闭，做不到"面板开着还能继续打字"。
    /// 2. 不开 Popup.IsLightDismissEnabled：那个 light dismiss 层会吃掉整个窗口的点击（标签页、页面跳转全失效），
    ///    桌面鼠标下还经常关不掉，等于把应用点死。改为监听根节点 PointerPressed 自己关面板。
    /// </remarks>
    public static class SearchHistoryBehavior
    {
        /// <summary>
        /// 面板关闭后短时间内不再打开，避免点击输入框时展开收起闪烁
        /// </summary>
        private const double REOPEN_GUARD_MILLISECONDS = 200;

        /// <summary>
        /// 为搜索框启用搜索历史面板
        /// </summary>
        /// <param name="searchBox">目标搜索框</param>
        /// <param name="searchService">搜索历史存储所在的服务</param>
        /// <param name="onSearchRequested">用户选中历史关键字后的回调</param>
        public static void Attach(AutoSuggestBox searchBox, SearchService searchService,
            Action<string> onSearchRequested)
        {
            if (searchBox == null) throw new ArgumentNullException(nameof(searchBox));
            if (searchService == null) throw new ArgumentNullException(nameof(searchService));

            var panel = new SearchHistoryPanel(searchService);
            var popup = new Popup()
            {
                Child = panel,
            };
            var lastClosedTime = DateTime.MinValue;

            void HideHistory()
            {
                if (popup.IsOpen)
                {
                    popup.IsOpen = false;
                }
            }

            void ShowHistory()
            {
                if (popup.IsOpen) return;
                if ((DateTime.Now - lastClosedTime).TotalMilliseconds < REOPEN_GUARD_MILLISECONDS) return;

                // 输入框有内容时交回联想词，不弹搜索历史
                if (!string.IsNullOrWhiteSpace(searchBox.Text))
                {
                    if (searchBox.Items.Count > 0)
                    {
                        searchBox.IsSuggestionListOpen = true;
                    }

                    return;
                }

                if (!panel.RefreshHistory()) return;

                // 空输入框时只留历史面板，别和原生联想词下拉叠在一起
                searchBox.IsSuggestionListOpen = false;
                PositionBelow(popup, searchBox);
                popup.IsOpen = true;
                KeepFocusInSearchBox(searchBox);
            }

            popup.Closed += (sender, e) => lastClosedTime = DateTime.Now;

            panel.SearchRequested += (sender, keyword) =>
            {
                HideHistory();
                searchBox.Text = keyword;
                searchBox.IsSuggestionListOpen = false;
                onSearchRequested?.Invoke(keyword);
            };
            panel.HistoryEmptied += (sender, e) => HideHistory();

            // 面板在独立的 Popup 树里，点击面板以外的地方都收不到事件，
            // 所以挂在根节点上统一关面板（包含已经 e.Handled 的点击）
            if (Window.Current?.Content is UIElement root)
            {
                root.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler((sender, e) =>
                {
                    if (!popup.IsOpen) return;
                    // 面板落在根节点坐标系下的矩形，点在面板内就不关
                    if (IsPointerInPopup(e, root, popup, panel)) return;
                    HideHistory();
                }), true);
            }

            searchBox.GotFocus += (sender, e) => ShowHistory();
            searchBox.Tapped += (sender, e) => ShowHistory();
            searchBox.TextChanged += (sender, e) =>
            {
                if (!string.IsNullOrWhiteSpace(searchBox.Text))
                {
                    HideHistory();
                    return;
                }

                // 内容被清空时收起联想词，切回搜索历史
                if (e.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
                {
                    HideHistory();
                    return;
                }

                ShowHistory();
            };
            searchBox.QuerySubmitted += (sender, e) => HideHistory();
            searchBox.KeyDown += (sender, e) =>
            {
                // Popup 不像 Flyout 那样自带 Esc 关闭
                if (e.Key != VirtualKey.Escape || !popup.IsOpen) return;
                HideHistory();
                e.Handled = true;
            };
        }

        /// <summary>
        /// 把面板摆到搜索框正下方
        /// </summary>
        private static void PositionBelow(Popup popup, FrameworkElement anchor)
        {
            if (Window.Current?.Content is not UIElement root) return;

            var position = anchor.TransformToVisual(root)
                .TransformPoint(new Point(0, anchor.ActualHeight));
            popup.HorizontalOffset = position.X;
            popup.VerticalOffset = position.Y;
        }

        /// <summary>
        /// 判断这次点击是不是落在面板里（Popup 偏移与根节点坐标同处窗口坐标系）
        /// </summary>
        private static bool IsPointerInPopup(PointerRoutedEventArgs args, UIElement root, Popup popup,
            FrameworkElement content)
        {
            var point = args.GetCurrentPoint(root).Position;
            var left = popup.HorizontalOffset;
            var top = popup.VerticalOffset;
            return point.X >= left && point.X <= left + content.ActualWidth
                && point.Y >= top && point.Y <= top + content.ActualHeight;
        }

        /// <summary>
        /// 万一焦点被面板带走，把焦点和光标还给搜索框（Popup 不会因此被关闭）
        /// </summary>
        private static void KeepFocusInSearchBox(AutoSuggestBox searchBox)
        {
            var textBox = searchBox.FindChildrenByType<TextBox>().FirstOrDefault();
            if (textBox == null) return;

            _ = textBox.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                if (textBox.FocusState != FocusState.Unfocused) return;
                textBox.Focus(FocusState.Programmatic);
                textBox.SelectionStart = textBox.Text?.Length ?? 0;
            });
        }
    }
}
