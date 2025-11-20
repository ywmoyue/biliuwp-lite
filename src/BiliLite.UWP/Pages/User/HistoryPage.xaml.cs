using BiliLite.Models.Common;
using BiliLite.Models.Common.User;
using BiliLite.Services;
using BiliLite.Services.Interfaces;
using BiliLite.ViewModels.User;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using BiliLite.Models.Common.Article;
using Microsoft.UI.Xaml.Input;
using BiliLite.Extensions;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages.User
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HistoryPage : BasePage, IRefreshablePage, IUpdatePivotLayout
    {
        private readonly HistoryViewModel m_viewModel;
        public HistoryPage()
        {
            m_viewModel = App.ServiceProvider.GetRequiredService<HistoryViewModel>();
            this.InitializeComponent();
            Title = "历史记录";
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New && m_viewModel.Videos == null)
            {
                await m_viewModel.LoadHistory();
            }
        }

        private void Video_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as UserHistoryItem;
            OpenVideoItem(data);
        }

        private void removeVideoHistory_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuFlyoutItem).DataContext as UserHistoryItem;
            m_viewModel.Del(item);
        }

        private async void SearchBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var keyword = sender.Text;
            await m_viewModel.SearchHistory(keyword);
        }

        public async Task Refresh()
        {
            m_viewModel.Refresh();
        }

        public void UpdatePivotLayout()
        {
            pivot.UseLayoutRounding = !pivot.UseLayoutRounding;
            pivot.UseLayoutRounding = !pivot.UseLayoutRounding;
        }

        private void VideoItem_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!e.IsMiddleButtonNewTap(sender)) return;
            var element = e.OriginalSource as FrameworkElement;
            var item = element.DataContext as UserHistoryItem;
            OpenVideoItem(item, true);
        }

        private void OpenVideoItem(UserHistoryItem userHistoryItem, bool dontGoTo = false)
        {
            var data = userHistoryItem;
            // TODO: 改用方法Map
            if (data.History.Business == "pgc")
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    page = typeof(SeasonDetailPage),
                    title = data.Title,
                    parameters = data.Kid,
                    dontGoTo = dontGoTo,
                });
            }
            else if (data.History.Business == "live") // 直播
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    page = typeof(LiveDetailPage),
                    title = data.Title,
                    parameters = data.History.Oid,
                    dontGoTo = dontGoTo,
                });
            }
            else if (data.History.Business == "archive") // 普通视频
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    page = typeof(VideoDetailPage),
                    title = data.Title,
                    parameters = data.History.Bvid,
                    dontGoTo = dontGoTo,
                });
            }
            else if (data.History.Business == "article") // 专栏
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    page = typeof(ArticlePage), // 专栏好像没有写专门的页面
                    title = data.Title,
                    parameters = new ArticlePageNavigationInfo()
                    {
                        Url = "https://www.bilibili.com/read/cv" + data.History.Oid,
                        CvId = data.History.Oid + ""
                    },
                    dontGoTo = dontGoTo,
                });
            }
            else if (data.History.Business == "article-list") // 专栏文集, 但历史记录里实际保存的为其中的专栏文章
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    page = typeof(ArticlePage), // 专栏文集好像没有写专门的页面
                    title = data.Title,
                    parameters = new ArticlePageNavigationInfo()
                    {
                        Url = "https://www.bilibili.com/read/cv" + data.History.Cid,
                        CvId = data.History.Cid + ""
                    },
                    dontGoTo = dontGoTo,
                });
            }
            else
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    page = typeof(VideoDetailPage),
                    title = data.Title,
                    parameters = data.History.Bvid,
                    dontGoTo = dontGoTo,
                });
            }
        }
    }
}
