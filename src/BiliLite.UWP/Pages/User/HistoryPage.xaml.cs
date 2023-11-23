using BiliLite.Models.Common;
using BiliLite.Modules.User;
using BiliLite.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using BiliLite.Models.Common.User;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages.User
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HistoryPage : BasePage
    {
        HistoryVM historyVM;
        public HistoryPage()
        {
            this.InitializeComponent();
            Title = "历史记录";
            historyVM = new HistoryVM();
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New && historyVM.Videos == null)
            {
                await historyVM.LoadHistory();
            }
        }

        private void Video_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as UserHistoryItem;
            // TODO: 改用方法Map
            if(data.History.Business == "pgc")
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    page = typeof(SeasonDetailPage),
                    title = data.Title,
                    parameters = data.Kid
                });
            } 
            else if (data.History.Business == "live") // 直播
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon= Symbol.Play,
                    page = typeof(LiveDetailPage),
                    title = data.Title,
                    parameters = data.History.Oid
                });
            }
            else if (data.History.Business == "archive") // 普通视频
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    page = typeof(VideoDetailPage),
                    title = data.Title,
                    parameters = data.History.Bvid
                });
            }
            else if (data.History.Business == "article") // 专栏
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    page = typeof(WebPage), // 专栏好像没有写专门的页面
                    title = data.Title,
                    parameters = "https://www.bilibili.com/read/cv" + data.History.Oid
                });
            }
            else if (data.History.Business == "article-list") // 专栏文集, 但历史记录里实际保存的为其中的专栏文章
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    page = typeof(WebPage), // 专栏文集好像没有写专门的页面
                    title = data.Title,
                    parameters = "https://www.bilibili.com/read/cv" + data.History.Cid
                });
            }
            else
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    page = typeof(VideoDetailPage),
                    title = data.Title,
                    parameters = data.History.Bvid
                });
            }
        }

        private void removeVideoHistory_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuFlyoutItem).DataContext as UserHistoryItem;
            historyVM.Del(item);
        }
    }
}
