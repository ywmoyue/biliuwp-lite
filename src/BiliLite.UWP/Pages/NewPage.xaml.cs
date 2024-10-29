using BiliLite.Pages.Bangumi;
using Microsoft.UI.Xaml.Controls;
using System;
using System.IO;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using BiliLite.Models.Common;
using BiliLite.Services;
using BiliLite.Extensions;
using NavigationView = Microsoft.UI.Xaml.Controls.NavigationView;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class NewPage : Page
    {
        public NewPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SetBackground();
        }
        private async void SetBackground()
        {
            var background = SettingService.GetValue(SettingConstants.UI.BACKGROUND_IMAGE, Constants.App.BACKGROUND_IAMGE_URL);
            if (background == Constants.App.BACKGROUND_IAMGE_URL)
            {
                backgroundImage.Source = new BitmapImage(new Uri(background));
            }
            else
            {
                if (!await background.CheckFileExist())
                {
                    Notify.ShowMessageToast("背景图片不存在,请重新设置");
                    return;
                }
                var file = await StorageFile.GetFileFromPathAsync(background);
                var img = new BitmapImage();
                img.SetSource(await file.OpenReadAsync());
                backgroundImage.Source = img;
            }
        }
        private void BtnOpenRank_Click(object sender, RoutedEventArgs e)
        {
            OpenNewTabPage("排行榜", Symbol.FourBars, typeof(RankPage));
        }

        private void BtnOpenBangumiIndex_Click(object sender, RoutedEventArgs e)
        {
            OpenNewTabPage("番剧索引", Symbol.Filter, typeof(AnimeIndexPage));
        }

        private void BtnOpenBangumiTimeline_Click(object sender, RoutedEventArgs e)
        {
            OpenNewTabPage("番剧时间表", Symbol.Clock, typeof(TimelinePage), AnimeType.Bangumi);
        }

        private async void BtnOpenMyFollow_Click(object sender, RoutedEventArgs e)
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录");
                return;
            }
            OpenNewTabPage("我的收藏", Symbol.OutlineStar, typeof(User.FavoritePage), User.OpenFavoriteType.Video);
        }

        private void BtnOpenSetting_Click(object sender, RoutedEventArgs e)
        {
            OpenNewTabPage("设置", Symbol.Setting, typeof(SettingPage));
        }

        private async void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (string.IsNullOrEmpty(SearchBox.Text))
            {
                Notify.ShowMessageToast("关键字不能为空");
                return;
            }
            if (await MessageCenter.HandelUrl(SearchBox.Text))
            {
                return;
            }
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Find,
                page = typeof(SearchPage),
                title = "搜索:" + SearchBox.Text,
                parameters = new SearchParameter()
                {
                    keyword = SearchBox.Text,
                    searchType = SearchType.Video
                }
            });
        }

        private async void btnSetBackground_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.FileTypeFilter.Add(".png");
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            StorageFile file = await fileOpenPicker.PickSingleFileAsync();
            if (file != null)
            {
                SettingService.SetValue(SettingConstants.UI.BACKGROUND_IMAGE, file.Path);
                SetBackground();
            }
        }

        private void btnSetDefaultBackground_Click(object sender, RoutedEventArgs e)
        {
            SettingService.SetValue(SettingConstants.UI.BACKGROUND_IMAGE, Constants.App.BACKGROUND_IAMGE_URL);
            SetBackground();
        }

        private async void BtnOpenHistory_Click(object sender, RoutedEventArgs e)
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录");
                return;
            }
            OpenNewTabPage("历史记录", Symbol.Clock, typeof(User.HistoryPage));
        }

        private void BtnOpenDownload_Click(object sender, RoutedEventArgs e)
        {
            OpenNewTabPage("离线下载", Symbol.Download, typeof(DownloadPage));
        }

        private void BtnOpenLive_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Document,
                page = typeof(Live.LiveRecommendPage),
                title = "全部直播"
            });
        }

        private void OpenNewTabPage(string title, Symbol symbol, Type pageType, object parameter = null)
        {
            if (!(this.Parent is Frame frame)) return;
            // 首页中的新标签页Tab
            if (frame.Parent is NavigationView navigationView)
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = symbol,
                    page = pageType,
                    title = title,
                    parameters = parameter,
                });
            }
            // 新标签页
            else if (frame.Parent is TabViewItem tabItem)
            {
                tabItem.Header = title;
                tabItem.IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource() { Symbol = symbol };
                if (parameter == null)
                    this.Frame.Navigate(pageType);
                else
                    this.Frame.Navigate(pageType, parameter);
            }
        }
    }
}
