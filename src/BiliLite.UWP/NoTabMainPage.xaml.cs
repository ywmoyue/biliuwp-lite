﻿using BiliLite.Controls;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Pages;
using BiliLite.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
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
            MainPageLoaded?.Invoke(this, EventArgs.Empty);
        }
    }

    public class NewInstanceFrame : Grid
    {
        public event NavigatedEventHandler Navigated;
        public NewInstanceFrame()
        {
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

        public async void GoBack()
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
                    await frame.AnimateYAsync(0, this.ActualHeight, 300);
                    frame.Navigated -= Frame_Navigated;
                    frame.Close();


                    this.Children.Remove(frame);
                    //frame = this.Children.Last() as Frame;

                    var lastFrameEle = Children.LastOrDefault();
                    if (lastFrameEle is Frame { Content: Page lastFramePage })
                    {
                        lastFramePage.Visibility = Visibility.Visible;
                    }
                }
            }
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
