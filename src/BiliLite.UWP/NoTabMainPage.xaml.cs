using BiliLite.Controls;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Pages;
using BiliLite.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NoTabMainPage : Page, IMainPage
    {
        private readonly ShortcutKeyService m_shortcutKeyService;
        private readonly Stack<string> m_titleStack = new Stack<string>();

        public NoTabMainPage()
        {
            m_shortcutKeyService = App.ServiceProvider.GetRequiredService<ShortcutKeyService>();
            m_shortcutKeyService.SetMainPage(this);
            this.InitializeComponent();
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            mode = SettingService.GetValue<int>(SettingConstants.UI.DISPLAY_MODE, 0);
            Window.Current.SetTitleBar(TitleBar);
            frame.Navigated += Frame_Navigated;
            MessageCenter.NavigateToPageEvent += NavigationHelper_NavigateToPageEvent;
            MessageCenter.ChangeTitleEvent += MessageCenter_ChangeTitleEvent;
            MessageCenter.ViewImageEvent += MessageCenter_ViewImageEvent;
            MessageCenter.MiniWindowEvent += MessageCenter_MiniWindowEvent;
            MessageCenter.FullscreenEvent += MessageCenter_FullscreenEvent;
            MessageCenter.SeekEvent += MessageCenter_SeekEvent;
            Window.Current.Content.PointerPressed += Content_PointerPressed;

            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;
        }

        public event EventHandler MainPageLoaded;

        public object CurrentPage => frame.Content;

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
                Window.Current.SetTitleBar(TitleBar);
            }
        }

        private void MessageCenter_FullscreenEvent(object sender, bool e)
        {
            MainWindowsTitleBar.Visibility = e ? Visibility.Collapsed : Visibility.Visible;
        }

        private void MessageCenter_SeekEvent(object sender, double e)
        {
            if (!(frame.Content is PlayPage playPage)) return;
            playPage.Seek(e);
        }

        private void Content_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var par = e.GetCurrentPoint(sender as Frame).Properties.PointerUpdateKind;
            if (SettingService.GetValue(SettingConstants.UI.MOUSE_MIDDLE_ACTION, (int)MouseMiddleActions.Back) == (int)MouseMiddleActions.Back
                && par == Windows.UI.Input.PointerUpdateKind.XButton1Pressed || par == Windows.UI.Input.PointerUpdateKind.MiddleButtonPressed)
            {
                //如果打开了图片浏览，则关闭图片浏览
                if (gridViewer.Visibility == Visibility.Visible)
                {
                    imgViewer_CloseEvent(this, null);
                    e.Handled = true;
                    return;
                }
                //处理多标签
                if (this.frame.CanGoBack)
                {
                    this.frame.GoBack();
                    e.Handled = true;
                    BackTitle();
                }

            }
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.Content is Pages.HomePage)
            {
                txtTitle.Text = "哔哩哔哩 UWP";
                // 在主页时隐藏返回主页按钮
                btnHome.Visibility = Visibility.Collapsed;
            }
            else
            {
                // 不在主页时显示返回主页按钮（仅在单窗口模式）
                btnHome.Visibility = mode == 1 ? Visibility.Visible : Visibility.Collapsed;
            }

            if (e.Content is Pages.BasePage && e.NavigationMode != NavigationMode.Back)
            {
                var title = (e.Content as BasePage).Title;
                PushTitle(title);
            }

            if (frame.CanGoBack)
            {
                btnBack.Visibility = Visibility.Visible;
            }
            else
            {
                btnBack.Visibility = Visibility.Collapsed;
            }
        }
        private int mode = 1;
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
            if (mode == 1)
            {
                ChangeTitle(e);
            }
        }

        private void NavigationHelper_NavigateToPageEvent(object sender, NavigationInfo e)
        {
            if (mode == 1)
            {
                //PushTitle(e.title);
                frame.Navigate(e.page, e.parameters);
                (frame.Content as Page).NavigationCacheMode = NavigationCacheMode.Required;
            }
            else
            {
                OpenNewWindow(e);
            }
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            //如果打开了图片浏览，则关闭图片浏览
            if (gridViewer.Visibility == Visibility.Visible)
            {
                imgViewer_CloseEvent(this, null);
                return;
            }
            if (frame.CanGoBack)
            {
                frame.GoBack();
                BackTitle();
                if (frame.Content is IScrollRecoverablePage page)
                {
                    page.ScrollRecover();
                }
            }
        }

        private async void btnHome_Click(object sender, RoutedEventArgs e)
        {
            // 如果打开了图片浏览，则关闭图片浏览
            if (gridViewer.Visibility == Visibility.Visible)
            {
                imgViewer_CloseEvent(this, null);
                return;
            }

            // 回到主页
            while (frame.CanGoBack || (frame as NewInstanceFrame).Children.Count > 1)
            {
                await (frame as NewInstanceFrame).GoBack();
            }

            // 清空标题堆栈
            m_titleStack.Clear();
            txtTitle.Text = "哔哩哔哩 UWP";
        }

        private async void OpenNewWindow(NavigationInfo e)
        {

            CoreApplicationView newView = CoreApplication.CreateNewView();
            int newViewId = 0;
            await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var res = App.Current.Resources;
                Frame frame = new Frame();
                frame.Navigate(e.page, e.parameters);
                Window.Current.Content = frame;
                Window.Current.Activate();
                newViewId = ApplicationView.GetForCurrentView().Id;
                ApplicationView.GetForCurrentView().Consolidated += (sender, args) =>
                {
                    frame.Navigate(typeof(BlankPage));
                    CoreWindow.GetForCurrentThread().Close();
                };
            });
            bool viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);
        }
        private void MessageCenter_ViewImageEvent(object sender, ImageViewerParameter e)
        {
            gridViewer.Visibility = Visibility.Visible;
            imgViewer.InitImage(e);
        }
        private void imgViewer_CloseEvent(object sender, EventArgs e)
        {
            if (gridViewer.Visibility == Visibility.Visible)
            {
                imgViewer.ClearImage();
                gridViewer.Visibility = Visibility.Collapsed;
            }
        }

        private void BackTitle()
        {
            if (m_titleStack.Count == 0)
            {
                txtTitle.Text = "哔哩哔哩 UWP";
                return;
            }

            var title = m_titleStack.Pop();
            txtTitle.Text = title;
        }

        private void PushTitle(string title)
        {
            if (title == null) return;
            m_titleStack.Push(txtTitle.Text);
            txtTitle.Text = title;
        }

        private void ChangeTitle(string title)
        {
            txtTitle.Text = title;
        }

        private void NoTabMainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            frame.Navigate(typeof(Pages.HomePage));
            btnHome.Visibility = Visibility.Collapsed;
            MainPageLoaded?.Invoke(this, EventArgs.Empty);
        }
    }

    public class NewInstanceFrame : Grid
    {
        private int m_limitPageCount = 10;

        public event NavigatedEventHandler Navigated;

        public NewInstanceFrame()
        {
            m_limitPageCount = SettingService.GetValue(SettingConstants.UI.SINGLE_WINDOW_KEEP_PAGE_COUNT, SettingConstants.UI.DEFAULT_SINGLE_WINDOW_KEEP_PAGE_COUNT);
            AddFrame();
        }
        public object Content
        {
            get
            {
                var frame = this.Children.Last() as Frame;
                return frame.Content;
            }
        }

        private void AddFrame()
        {
            var frame = new MyFrame();
            frame.Navigated += Frame_Navigated;

            this.Children.Add(frame);
            LimitResource();
        }

        private void LimitResource()
        {
            var frames = this.Children.Where(x => x.GetType() == typeof(MyFrame)).ToList();
            if (frames.Count > m_limitPageCount + 1)
            {
                var needReleaseFrames = frames.Skip(1).Take(frames.Count - (m_limitPageCount + 1));
                foreach (var frame in needReleaseFrames)
                {
                    var frameIndex = this.Children.IndexOf(frame);
                    if (frameIndex == -1) continue;

                    var myFrame = frame as MyFrame;
                    var fakeFrame = new FakeFrame();

                    // 保存当前页面信息
                    if (myFrame.Content is BasePage currentPage)
                    {
                        fakeFrame.CurrentPageType = currentPage.GetType();
                        fakeFrame.CurrentPageParameter = currentPage.NavigationParameter;
                    }
                    else
                    {
                        continue;
                    }

                    // 保存返回堆栈中的页面信息
                    //var backStackInfo = new List<(Type PageType, object Parameter)>();
                    //foreach (var entry in myFrame.BackStack)
                    //{
                    //    backStackInfo.Add((entry.SourcePageType, entry.Parameter));
                    //}
                    //fakeFrame.BackStackInfo = backStackInfo;

                    // 清空内容和返回堆栈
                    myFrame.Close();
                    myFrame.Content = null;
                    myFrame.BackStack.Clear();
                    myFrame.ForwardStack.Clear();
                    myFrame.Navigated -= Frame_Navigated;

                    // 标记为已释放状态
                    fakeFrame.IsReleased = true;

                    // 从this.Children中对应顺序位置插入替换fakeFrame和myFrame，然后销毁myFrame
                    this.Children.Remove(myFrame);
                    this.Children.Insert(frameIndex, fakeFrame);
                }
            }
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
            Navigated?.Invoke(sender, e);
        }

        public bool Navigate(Type sourcePageType, object parameter = null)
        {
            var frame = this.Children.Last() as Frame;
            //检查最后一个Frame中是否存在此页面
            var contaias = ContainsPageType(sourcePageType);
            if (contaias)
            {
                AddFrame();
            }
            if (frame.Content is PlayPage)
            {
                (frame.Content as PlayPage).Pause();
            }
            if (frame.Content is Page page)
            {
                page.Visibility = Visibility.Collapsed;
            }

            //跳转页面
            (this.Children.Last() as Frame).Navigate(sourcePageType, parameter);

            return true;
        }
        public bool CanGoBack
        {
            get
            {
                var frame = this.Children.Last() as Frame;
                return this.Children.Count > 1 || frame.CanGoBack;
            }
        }

        public async Task GoBack()
        {
            var frame = this.Children.Last() as MyFrame;

            if (frame.Content is Page page)
            {
                page.NavigationCacheMode = NavigationCacheMode.Disabled;
            }
            if (frame.CanGoBack)
            {
                frame.GoBack();
                frame.ForwardStack.Clear();

                if (frame.Content is Page forwardPage)
                {
                    forwardPage.Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (this.Children.Count > 1)
                {
                    // 检查上一个Frame是否被释放
                    var previousFrameIndex = this.Children.Count - 2;
                    var previousFrame = this.Children[previousFrameIndex] as FakeFrame;
                    if (previousFrame != null)
                    {
                        // 重建页面状态
                        var realFrame = await ReconstructFrame(previousFrame);

                        // 从this.Children中对应顺序位置插入替换fakeFrame和myFrame，然后销毁fakeFrame
                        this.Children.Remove(previousFrame);
                        this.Children.Insert(previousFrameIndex, realFrame);
                        realFrame.Navigated += Frame_Navigated;
                    }

                    await frame.AnimateYAsync(0, this.ActualHeight, 300);
                    frame.Navigated -= Frame_Navigated;
                    frame.Close();
                    this.Children.Remove(frame);

                    var lastFrameEle = Children.LastOrDefault();
                    if (lastFrameEle is Frame { Content: Page lastFramePage })
                    {
                        lastFramePage.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private async Task<MyFrame> ReconstructFrame(FakeFrame fakeFrame)
        {
            var myFrame = new MyFrame();

            // 重建返回堆栈
            //if (fakeFrame.BackStackInfo != null && fakeFrame.BackStackInfo.Any())
            //{
            //    // 先导航到返回堆栈的第一个页面
            //    if (fakeFrame.BackStackInfo.Count > 0)
            //    {
            //        var firstPage = fakeFrame.BackStackInfo.First();
            //        myFrame.Navigate(firstPage.PageType, firstPage.Parameter);

            //        // 等待导航完成
            //        await Task.Delay(100);

            //        // 重建剩余的返回堆栈
            //        for (int i = 1; i < fakeFrame.BackStackInfo.Count; i++)
            //        {
            //            var pageInfo = fakeFrame.BackStackInfo[i];
            //            myFrame.Navigate(pageInfo.PageType, pageInfo.Parameter);
            //            await Task.Delay(50);
            //        }
            //    }
            //}

            // 重建当前页面
            if (fakeFrame.CurrentPageType != null)
            {
                myFrame.Navigate(fakeFrame.CurrentPageType, fakeFrame.CurrentPageParameter);
            }

            return myFrame;
        }

        private bool ContainsPageType(Type sourcePageType)
        {
            var frame = this.Children.Last() as Frame;
            if (frame.CurrentSourcePageType == sourcePageType)
            {
                return true;
            }
            foreach (var item in frame.BackStack)
            {
                if (sourcePageType == item.SourcePageType)
                {
                    return true;
                }
            }
            return false;
        }
    }
    public class BlankPage : Page { }
}
