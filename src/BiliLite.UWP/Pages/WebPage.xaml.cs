using BiliLite.Controls;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class WebPage : BasePage
    {
        private readonly CookieService m_cookieService;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly WebPageViewModel m_viewModel;
        private bool m_needCookie = false;

        public WebPage()
        {
            m_viewModel = App.ServiceProvider.GetRequiredService<WebPageViewModel>();
            m_cookieService = App.ServiceProvider.GetRequiredService<CookieService>();
            this.InitializeComponent();
            Title = "网页浏览";
            this.Loaded += WebPage_Loaded;
        }

        private async void CoreWebView2_NewWindowRequested(CoreWebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
        {
            args.Handled = true;
            var handelUrlResult = await MessageCenter.HandelUrl(args.Uri);
            if (handelUrlResult) return;
            var md = new MessageDialog("是否使用外部浏览器打开此链接？");
            md.Commands.Add(new UICommand("确定", new UICommandInvokedHandler(async (e) => { await Windows.System.Launcher.LaunchUriAsync(new Uri(args.Uri)); })));
            md.Commands.Add(new UICommand("取消", new UICommandInvokedHandler((e) => { })));
            await md.ShowAsync();
        }

        private void WebPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!(this.Parent is MyFrame frame)) return;
            frame.ClosedPage -= WebPage_ClosedPage;
            frame.ClosedPage += WebPage_ClosedPage;
        }

        private void WebPage_ClosedPage(object sender, EventArgs e)
        {
            CloseWebView();
        }

        private void CloseWebView()
        {
            webView.Close();
            //(this.Content as Grid).Children.Remove(webView);
            webView = null;
            //GC.Collect();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await InitWebView2();
            if (e.NavigationMode != NavigationMode.New) return;

            var uri = "";
            try
            {
                var webPageNavigationInfo = JsonConvert.DeserializeObject<WebPageNavigationInfo>(JsonConvert.SerializeObject(e.Parameter));
                uri = webPageNavigationInfo.Url;
                m_needCookie = webPageNavigationInfo.NeedCookie;
            }
            catch 
            {
                uri = e.Parameter.ToString();
            }

            if (uri.Contains("h5/vlog"))
            {
                webView.MaxWidth = 500;
            }

            if (uri.Contains("read/cv"))
            {
                //如果是专栏，内容加载完成再显示
                webView.Visibility = Visibility.Collapsed;
            }

            var url = new Uri(uri);
            if (uri.StartsWith("ms-appx://"))
            {
                var templateText = await FileIO.ReadTextAsync(
                    await StorageFile.GetFileFromApplicationUriAsync(url));

                webView.NavigateToString(templateText);
            }
            else
            {
                if (url.Host.Contains(Constants.BILIBILI_HOST) && m_needCookie)
                {
                    foreach (var cookie in m_cookieService.Cookies)
                    {
                        var webCookie = webView.CoreWebView2.CookieManager.CreateCookie(cookie.Name, cookie.Value,
                            Constants.BILIBILI_HOST, "/");
                        webView.CoreWebView2.CookieManager.AddOrUpdateCookie(webCookie);
                    }
                }

                webView.Source = new Uri(uri);
            }
        }

        private async Task InitWebView2()
        {
            await webView.EnsureCoreWebView2Async();
            webView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back || e.SourcePageType == typeof(BlankPage))
            {
                NavigationCacheMode = NavigationCacheMode.Disabled;
                CloseWebView();
            }
            base.OnNavigatingFrom(e);
        }


        private void btnForword_Click(object sender, RoutedEventArgs e)
        {
            if (webView.CanGoForward)
            {
                webView.GoForward();
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            webView.Reload();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (webView.CanGoBack)
            {
                webView.GoBack();
            }
        }

        private void btnShare_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested -= DataTransferManager_DataRequested;
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            DataTransferManager.ShowShareUI();
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs e)
        {
            DataPackage dataPackage = e.Request.Data;
            dataPackage.Properties.Title = "共享链接";
            dataPackage.SetWebLink(webView.Source);
        }

        private async void btnOpenBrowser_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(webView.Source);
        }

        // webview2中没找到代替的api，暂时不用
        private async void webView_UnsupportedUriSchemeIdentified(object sender, WebViewUnsupportedUriSchemeIdentifiedEventArgs args)
        {
            if (args.Uri.AbsoluteUri.Contains("article"))
            {
                args.Handled = true;
                return;
            }
            if (args.Uri.AbsoluteUri.Contains("bilibili://"))
            {
                args.Handled = true;
                var re = await MessageCenter.HandelUrl(args.Uri.AbsoluteUri);
                if (!re)
                {
                    Notify.ShowMessageToast("不支持打开的链接" + args.Uri.AbsoluteUri);
                }
            }
        }

        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {
            Notify.ShowMessageToast("虽然看起来像个浏览器，但这完全这不是个浏览器啊！ ╰（‵□′）╯");
        }

        private void WebView_OnNavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        {
        }

        private async void WebView_OnNavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            if (Parent is Frame frame)
            {
                if (frame.Parent is TabViewItem tabViewItem &&
                    !string.IsNullOrEmpty(webView.CoreWebView2.DocumentTitle))
                {
                    tabViewItem.Header = webView.CoreWebView2.DocumentTitle;
                }
                else if (!(frame.Parent is TabViewItem))
                {
                    MessageCenter.ChangeTitle(this, webView.CoreWebView2.DocumentTitle);
                }
            }

            try
            {
                //专栏阅读设置
                if (sender.Source != null && sender.Source.AbsoluteUri.Contains("read/cv"))
                {
                    await webView?.CoreWebView2?.ExecuteScriptAsync(
                        @"$('#internationalHeader').hide();
$('.unlogin-popover').hide();
$('.up-info-holder').hide();
$('.nav-tab-bar').hide();
$('.international-footer').hide();
$('.page-container').css('padding-right','0');
$('.no-login').hide();
$('.author-container').show();
$('.author-container').css('margin','12px 0px -12px 0px');"
                    );
                    //将专栏图片替换成jpg
                    await webView?.CoreWebView2?.ExecuteScriptAsync(
                        @"document.getElementsByClassName('img-box').forEach(element => {
                element.getElementsByTagName('img').forEach(image => {
                    image.src=image.getAttribute('data-src')+'@progressive.jpg';
               });
            });"
                    );
                }

                await webView?.CoreWebView2?.ExecuteScriptAsync(
                    "$('.h5-download-bar').hide()"
                );
            }
            catch (Exception ex)
            {
                _logger.Warn(ex.Message, ex);
            }
            finally
            {
                if (webView != null) webView.Visibility = Visibility.Visible;
            }

            m_viewModel.IsEnableGoBack = webView.CanGoBack;
            m_viewModel.IsEnableGoForward = webView.CanGoForward;
        }
    }
}
