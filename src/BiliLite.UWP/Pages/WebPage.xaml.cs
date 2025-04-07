using BiliLite.Controls;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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
        private bool m_needCookie = false;

        public WebPage()
        {
            m_cookieService = App.ServiceProvider.GetRequiredService<CookieService>();
            this.InitializeComponent();
            Title = "网页浏览";
            this.Loaded += WebPage_Loaded;
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
        }

        private void WebPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Parent is not MyFrame frame) return;
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

            string uri;
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
            webView.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
        }

        private async void CoreWebView2_NewWindowRequested(CoreWebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
        {
            args.Handled = true;
            var handelUrlResult = await MessageCenter.HandelUrl(args.Uri);
            if (handelUrlResult) return;
            await NotificationShowExtensions.ShowMessageDialog("是否使用外部浏览器打开此链接？", "", new Uri(args.Uri));
        }

        private void CoreWebView2_NavigationStarting(CoreWebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        {
            string targetUrl = args.Uri;
            if (!Regex.IsMatch(targetUrl, BiliPattern))
            {
                NotificationShowExtensions.ShowMessageToast("检测到未知的重定向，已自动切换到移动版网站");

                args.Cancel = true;
                UseMoblieUserAgent();
            }
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

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs e)
        {
            DataPackage dataPackage = e.Request.Data;
            dataPackage.Properties.Title = "共享链接";
            dataPackage.SetWebLink(webView.Source);
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
                    NotificationShowExtensions.ShowMessageToast("不支持打开的链接" + args.Uri.AbsoluteUri);
                }
            }
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement button = sender as FrameworkElement;
            switch (button.Tag as string)
            {
                case "Back":
                    if (webView.CanGoBack)
                        webView.GoBack();
                    break;
                case "Forword":
                    if (webView.CanGoForward)
                        webView.GoForward();
                    break;
                case "Refresh":
                    webView.Reload();
                    break;
                case "Info":
                    NotificationShowExtensions.ShowMessageToast("虽然看起来像个浏览器，但这完全这不是个浏览器啊！ ╰（‵□′）╯");
                    break;
                case "Desktop":
                    UseMoblieUserAgent(false);
                    break;
                case "Mobile":
                    UseMoblieUserAgent();
                    break;
                case "Share":
                    DataTransferManager.ShowShareUI();
                    break;
                case "OpenBrowser":
                    await Windows.System.Launcher.LaunchUriAsync(webView.Source);
                    break;
            }
        }

        private readonly string desktopUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/89.0.4389.105 Safari/537.36";
        private readonly string mobileUserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 14_0 like Mac OS X) AppleWebKit/537.36 (KHTML, like Gecko) Version/14.0 Mobile/15E148 Safari/537.36";

        private void UseMoblieUserAgent(bool isMobile = true)
        {
            if (isMobile)
            {
                webView.CoreWebView2.Settings.UserAgent = mobileUserAgent;
                Moblie.Visibility = Visibility.Collapsed;
            }
            else
            {
                webView.CoreWebView2.Settings.UserAgent = desktopUserAgent;
                Moblie.Visibility = Visibility.Visible;
            }
            webView.Reload();
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
                else if (frame.Parent is not TabViewItem)
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
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (Regex.IsMatch(AutoSuggestBox.Text, GeneralPattern))
            {
                if (AutoSuggestBox.Text.Contains("bilibili.com"))
                {
                    webView.Source = new Uri(AutoSuggestBox.Text);
                }
                else
                {
                    NotificationShowExtensions.ShowMessageToast("仅支持浏览B站网页");
                }
            }
            else if (!string.IsNullOrEmpty(AutoSuggestBox.Text))
            {
                var searchUri = $"https://search.bilibili.com/all?keyword={AutoSuggestBox.Text}";
                webView.Source = new Uri(searchUri);
            }
            else
            {
                webView.Source = new Uri(AutoSuggestBox.PlaceholderText);
            }
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (Regex.IsMatch(sender.Text, GeneralPattern))
            {
                sender.QueryIcon = new SymbolIcon(Symbol.Go);
            }
            else
            {
                sender.QueryIcon = new SymbolIcon(Symbol.Find);
            }
        }

        private readonly string BiliPattern = @"^https?://(?:www\.|m\.|search\.)?bilibili\.com(/.*)?$";
        private readonly string GeneralPattern = @"^(https?:\/\/)?([a-zA-Z0-9-]+\.)?([a-zA-Z0-9-]+\.)[a-zA-Z]{2,3}(\/.*)?$";
    }
}