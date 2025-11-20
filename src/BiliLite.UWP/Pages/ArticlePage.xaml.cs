using BiliLite.Controls;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Navigation;
using BiliLite.Extensions;
using Flurl.Http;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using BiliLite.Services;
using BiliLite.Models.Common.Article;
using Microsoft.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages;

/// <summary>
/// 可用于自身或导航至 Frame 内部的空白页。
/// </summary>
public sealed partial class ArticlePage : BasePage
{
    private ArticlePageNavigationInfo m_articlePageNavigationInfo;

    public ArticlePage()
    {
        this.InitializeComponent();
        this.Loaded += ArticlePage_Loaded; ;
    }

    private void ArticlePage_Loaded(object sender, RoutedEventArgs e)
    {
        if (this.Parent is not MyFrame frame) return;
        frame.ClosedPage -= ArticlePage_ClosedPage;
        frame.ClosedPage += ArticlePage_ClosedPage;
    }

    private void ArticlePage_ClosedPage(object sender, EventArgs e)
    {
        CloseWebView();
    }

    private void CloseWebView()
    {
        WebView.Close();
        //(this.Content as Grid).Children.Remove(webView);
        WebView = null;
        //GC.Collect();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await InitWebView2();

        var navigationInfo = JsonConvert.DeserializeObject<ArticlePageNavigationInfo>(JsonConvert.SerializeObject(e.Parameter));
        m_articlePageNavigationInfo = navigationInfo;
        var htmlFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Reader/index.html"));
        var folder = await htmlFile.GetParentAsync();

        WebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
            "reader.bililte.service",  // 自定义的域名
            folder.Path,         // 映射的本地文件夹路径
            CoreWebView2HostResourceAccessKind.Allow);

        WebView.CoreWebView2.Navigate($"https://reader.bililte.service/index.html");
    }

    private async Task InitWebView2()
    {
        await WebView.EnsureCoreWebView2Async();
        WebView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived; ;
    }

    private async void CoreWebView2_WebMessageReceived(CoreWebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        var json = args.WebMessageAsJson;
        if (json == null) return;
        var @event = JsonConvert.DeserializeObject<BaseArticleReaderEventMessage>(json);
        if (@event.Event == ArticleReaderEventLists.LOADED)
        {
            var articleModelStr = JsonConvert.SerializeObject(m_articlePageNavigationInfo).ToBase64();

            await WebView.CoreWebView2.ExecuteScriptAsync($"window.loadArticle('{articleModelStr}')");
        }
        else if (@event.Event == ArticleReaderEventLists.HOST_FETCH)
        {
            var data = JsonConvert.DeserializeObject<ArticleReaderHostFetchData>(JsonConvert.SerializeObject(@event.Data));
            await HostFetch(data);
        }
        else if (@event.Event == ArticleReaderEventLists.OPEN_IMG)
        {
            var data = JsonConvert.DeserializeObject<ArticleReaderOpenImgData>(JsonConvert.SerializeObject(@event.Data));
            OpenImage(data);
        }
        else if (@event.Event == ArticleReaderEventLists.SET_TITLE)
        {
            var data = JsonConvert.DeserializeObject<ArticleReaderSetTitleData>(JsonConvert.SerializeObject(@event.Data));

            if (Parent is Frame frame)
            {
                if (frame.Parent is TabViewItem tabViewItem)
                {
                    tabViewItem.Header = data.Title;
                }
                else if (frame.Parent is not TabViewItem)
                {
                    MessageCenter.ChangeTitle(this, data.Title);
                }
            }
        }
    }
    private void OpenImage(ArticleReaderOpenImgData data)
    {
        MessageCenter.OpenImageViewer(data.ImgUrlList, data.Index);
    }

    private async Task HostFetch(ArticleReaderHostFetchData data)
    {
        var result = await data.Url.WithHeaders(data.Headers).GetStringAsync();
        var fetchResult = new
        {
            id = data.Id,
            result = result,
        };
        var base64 = JsonConvert.SerializeObject(fetchResult).ToBase64();
        await WebView.CoreWebView2.ExecuteScriptAsync($"window.setHostFetchResult('{base64}')");
    }

    private async void WebView_OnNavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
    }

    private async void AppBarButton_Click(object sender, RoutedEventArgs e)
    {
        FrameworkElement button = sender as FrameworkElement;
        switch (button.Tag as string)
        {
            case "OpenBrowser":
                await Windows.System.Launcher.LaunchUriAsync(new Uri(m_articlePageNavigationInfo.Url));
                break;
        }
    }
}