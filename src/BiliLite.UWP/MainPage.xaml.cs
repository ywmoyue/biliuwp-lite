using BiliLite.Controls;
using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Pages;
using BiliLite.Services;
using BiliLite.Services.Interfaces;
using BiliLite.ViewModels.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace BiliLite
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page, IMainPage
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly ShortcutKeyService m_shortcutKeyService;
        private readonly MainPageViewModel m_viewModel;

        public MainPage()
        {
            m_viewModel = App.ServiceProvider.GetRequiredService<MainPageViewModel>();
            m_shortcutKeyService = App.ServiceProvider.GetRequiredService<ShortcutKeyService>();
            m_shortcutKeyService.SetMainPage(this);

            InitTabViewStyle();

            this.InitializeComponent();
            // 处理标题栏
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;

            Window.Current.SetTitleBar(AvailableDragRegion);

            //处理页面跳转
            MessageCenter.NavigateToPageEvent += NavigationHelper_NavigateToPageEvent;
            MessageCenter.ChangeTitleEvent += MessageCenter_ChangeTitleEvent;
            MessageCenter.ViewImageEvent += MessageCenter_ViewImageEvent;
            MessageCenter.MiniWindowEvent += MessageCenter_MiniWindowEvent;
            MessageCenter.GoBackEvent += MessageCenter_GoBackEvent;
            MessageCenter.SeekEvent += MessageCenter_SeekEvent;

            App.Current.Suspending += Current_Suspending;
            // Window.Current.Content.PointerPressed += Content_PointerPressed;

            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
        }

        public event EventHandler MainPageLoaded;

        public object CurrentPage
        {
            get
            {
                if (tabView.SelectedItem is not TabViewItem tabItem) return null;
                if (tabItem.Content is not Frame frame) return null;
                return frame.Content;
            }
        }

        private void Dispatcher_AcceleratorKeyActivated(Windows.UI.Core.CoreDispatcher sender, Windows.UI.Core.AcceleratorKeyEventArgs args)
        {
            if (args.EventType.ToString().Contains("Down"))
            {
                m_shortcutKeyService.HandleKeyDown(args.VirtualKey);
            }
            if (args.EventType.ToString().Contains("Up"))
            {
                m_shortcutKeyService.HandleKeyUp(args.VirtualKey);
            }
        }

        private async void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            _logger.Trace("应用挂起");
            var tabs = tabView.TabItems;
            foreach (var tab in tabs)
            {
                if (tab is not TabViewItem tabItem) continue;
                if (tabItem.Content is not MyFrame frame) continue;
                var page = frame.Content;
                if (page is not PlayPage playPage) continue;
                await playPage.ReportHistory();
            }
        }

        private void MessageCenter_SeekEvent(object sender, double e)
        {
            if (tabView.SelectedItem is not TabViewItem tabItem) return;
            if (tabItem.Content is not Frame frame) return;
            if (frame.Content is not PlayPage playPage) return;
            playPage.Seek(e);
        }

        private void MessageCenter_GoBackEvent(object sender, EventArgs e)
        {
            GoBack();
        }

        private void MessageCenter_MiniWindowEvent(object sender, bool e)
        {
            if (e)
            {
                MiniWindowsTitleBar.Visibility = Visibility.Visible;
                Window.Current.SetTitleBar(MiniWindowsTitleBar);
            }
            else
            {
                MiniWindowsTitleBar.Visibility = Visibility.Collapsed;
                Window.Current.SetTitleBar(AvailableDragRegion);
            }
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.NavigationMode == NavigationMode.New && e.Parameter != null && !string.IsNullOrEmpty(e.Parameter.ToString()))
            {
                var result = await MessageCenter.HandelUrl(e.Parameter.ToString());
                if (!result)
                {
                    NotificationShowExtensions.ShowMessageToast("无法打开链接:" + e.Parameter.ToString());
                }
            }
        }

        private void MessageCenter_ChangeTitleEvent(object sender, string e)
        {
            if (sender == null)
            {
                (tabView.SelectedItem as TabViewItem).Header = e;
                return;
            }

            foreach (var item in tabView.TabItems)
            {
                if (item is not TabViewItem tabViewItem) continue;
                if (tabViewItem.Content is not MyFrame frame) continue;
                if (sender == frame.Content)
                {
                    tabViewItem.Header = e;
                    break;
                }
            }
        }

        private async void NavigationHelper_NavigateToPageEvent(object sender, NavigationInfo e)
        {
            var item = new TabViewItem()
            {
                Header = e.title,
                IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource() { Symbol = e.icon }
            };
            var frame = new MyFrame();
            //注册鼠标点击事件
            frame.PointerPressed += Content_PointerPressed;
            frame.Navigate(e.page, e.parameters);
            item.Content = frame;
            var pageSaveService = App.ServiceProvider.GetRequiredService<PageSaveService>();
            frame.PageId = await pageSaveService.AddPage(e.title, e.page, e.parameters, e.icon);

            tabView.TabItems.Add(item);
            if (!e.dontGoTo)
                tabView.SelectedItem = item;
            item.UpdateLayout();
        }
        private void Content_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (SettingService.GetValue(SettingConstants.UI.MOUSE_MIDDLE_ACTION, (int)MouseMiddleActions.Back) == (int)MouseMiddleActions.Back
                && e.IsUseMiddleButton(sender))
            {
                GoBack();
                e.Handled = true;
            }
        }

        private async void GoBack()
        {
            //如果打开了图片浏览，则关闭图片浏览
            if (gridViewer.Visibility == Visibility.Visible)
            {
                imgViewer_CloseEvent(this, null);
                return;
            }

            //处理多标签
            if (tabView.SelectedItem != tabView.TabItems[0])
            {
                var frame = (tabView.SelectedItem as TabViewItem).Content as MyFrame;
                if (frame.CanGoBack)
                {
                    frame.Close();
                    frame.GoBack();
                }
                else
                {
                    await ClosePage(tabView.SelectedItem as TabViewItem);
                    //frame.Close();
                    //tabView.TabItems.Remove(tabView.SelectedItem);
                }
            }
        }

        /// <summary>
        /// 处理标题栏
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            if (FlowDirection == FlowDirection.LeftToRight)
            {
                CustomDragRegion.MinWidth = sender.SystemOverlayRightInset;
                ShellTitlebarInset.MinWidth = sender.SystemOverlayLeftInset;
            }
            else
            {
                CustomDragRegion.MinWidth = sender.SystemOverlayLeftInset;
                ShellTitlebarInset.MinWidth = sender.SystemOverlayRightInset;
            }
            CustomDragRegion.Height = ShellTitlebarInset.Height = sender.Height;
        }

        private void TabView_AddTabButtonClick(Microsoft.UI.Xaml.Controls.TabView sender, object args)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                page = typeof(NewPage),
                title = "新建页面"
            });
        }

        private async void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            var selectedTab = sender.SelectedItem as TabViewItem;
            var tabToClose = args.Tab;

            // 判断已选中的选项卡
            if (selectedTab == tabToClose)
            {
                if (gridViewer.Visibility == Visibility.Visible)
                {
                    imgViewer_CloseEvent(this, null);
                }
            }

            await ClosePage(args.Tab);
        }
        private async Task ClosePage(TabViewItem tabItem)
        {
            tabView.TabItems.Remove(tabItem);
            var frame = tabItem.Content as MyFrame;
            if (frame.Content is Page { Content: Grid grid })
            {
                grid.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    grid.Children.Clear();
                });
            }

            var pageSaveService = App.ServiceProvider.GetRequiredService<PageSaveService>();
            await pageSaveService.RemovePage(frame.PageId);

            frame.Close();
            //frame.Navigate(typeof(BlankPage));
            // frame.BackStack.Clear();
            tabItem.Content = null;
            GC.Collect();
        }
        private void tabView_Loaded(object sender, RoutedEventArgs e)
        {
            // 根据Tab高度设置图片视图边距
            gridViewer.Margin = new Thickness(0, m_viewModel.TabHeight, 0, 0);

            var frame = new MyFrame();

            frame.Navigate(typeof(HomePage));

            (tabView.TabItems[0] as TabViewItem).Content = frame;
        }
        private async void MessageCenter_ViewImageEvent(object sender, ImageViewerParameter e)
        {
            gridViewer.Visibility = Visibility.Visible;
            await gridViewer.FadeInAsync();
            imgViewer.InitImage(e);
        }
        private async void imgViewer_CloseEvent(object sender, EventArgs e)
        {
            if (gridViewer.Visibility == Visibility.Visible)
            {
                imgViewer.ClearImage();
                await gridViewer.FadeOutAsync();
                gridViewer.Visibility = Visibility.Collapsed;
            }
        }

        private void NewTabKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                page = typeof(NewPage),
                title = "新建页面"
            });
            args.Handled = true;
        }

        private async void CloseSelectedTabKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            if (((TabViewItem)tabView.SelectedItem).IsClosable)
            {

                await ClosePage((TabViewItem)tabView.SelectedItem);
            }
            args.Handled = true;
        }
        private void TabView_OnPreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Space && e.OriginalSource.GetType() != typeof(TextBox))
                e.Handled = true;
        }

        private void InitTabViewStyle()
        {
            var resources = Application.Current.Resources;
            var dict = resources.MergedDictionaries.FirstOrDefault(x => x.Source.AbsoluteUri.Contains("TabViewStyle"));

            var styleKvp = dict.FirstOrDefault(x => x.Key.ToString().Contains("TabViewItem"));

            if (styleKvp.Value is Style style)
            {
                style.Setters.Add(new Setter(TabViewItem.MinWidthProperty, m_viewModel.TabItemMinWidth));
                style.Setters.Add(new Setter(TabViewItem.MaxWidthProperty, m_viewModel.TabItemMaxWidth));
                style.Setters.Add(new Setter(TabViewItem.HeightProperty, m_viewModel.TabHeight));
            }

            var tabStyleKvp = dict.FirstOrDefault(x => x.Key.ToString().Contains("TabViewListView"));

            if (tabStyleKvp.Value is Style tabStyle)
            {
                tabStyle.Setters.Add(new Setter(TabViewItem.HeightProperty, m_viewModel.TabHeight));
            }
        }

        private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            MainPageLoaded?.Invoke(this, EventArgs.Empty);
        }

        private void TabView_TabDragCompleted(TabView sender, TabViewTabDragCompletedEventArgs args)
        {
            var draggedTabViewItem = args.Tab;
            var draggedFrame = draggedTabViewItem?.Content as Frame;
            var draggedPage = draggedFrame.Content as Page;
            if (draggedPage is IUpdatePivotLayout updateable)
            {
                updateable.UpdatePivotLayout();
            }
        }

        private void TabView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (gridViewer.Visibility == Visibility.Visible)
            {
                imgViewer_CloseEvent(this, null);
            }
        }
    }
}
