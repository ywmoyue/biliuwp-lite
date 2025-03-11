using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Search;
using BiliLite.Services;
using BiliLite.Services.Biz;
using BiliLite.Services.Interfaces;
using BiliLite.ViewModels.Search;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SearchPage : BasePage, IRefreshablePage, IUpdatePivotLayout
    {
        private readonly SearchService m_searchService;
        private readonly SearchPageViewModel m_viewModel;

        public SearchPage()
        {
            NavigationCacheMode = NavigationCacheMode.Required;
            m_searchService = App.ServiceProvider.GetRequiredService<SearchService>();
            m_viewModel = App.ServiceProvider.GetRequiredService<SearchPageViewModel>();
            m_viewModel.Init(m_searchService.PivotIndexCache = 0, m_searchService.ComboIndexCache = 0);
            this.InitializeComponent();
            Title = "搜索";
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            SearchParameter par = new SearchParameter();
            if (e.Parameter is string)
            {
                par.Keyword = e.Parameter.ToString();
            }
            else
            {
                par = e.Parameter as SearchParameter;
                if (par == null)
                {
                    par = JsonConvert.DeserializeObject<SearchParameter>(JsonConvert.SerializeObject(e.Parameter));
                }
            }

            par.Keyword = par.Keyword.TrimStart('@');
            txtKeyword.Text = par.Keyword;
            foreach (var item in m_viewModel.SearchItems)
            {
                item.Keyword = par.Keyword;
                item.Area = m_viewModel.Area.area;
            }

            txtKeyword.Focus(FocusState.Keyboard);
        }

        private async void txtKeyword_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var queryText = args.QueryText;
            if (string.IsNullOrEmpty(queryText))
            {
                NotificationShowExtensions.ShowMessageToast("关键字不能为空啊，喂(#`O′)");
                return;
            }

            if (await MessageCenter.HandelUrl(queryText))
            {
                return;
            }
            queryText = queryText.TrimStart('@');
            foreach (var item in m_viewModel.SearchItems)
            {
                item.Keyword = queryText;
                item.Area = m_viewModel.Area.area;
                item.Page = 1;
                item.HasData = false;
            }
            m_viewModel.SelectItem.Refresh();
            ChangeTitle("搜索:" + queryText);
        }

        public void ChangeTitle(string title)
        {
            if ((this.Parent as Frame).Parent is TabViewItem)
            {
                if (this.Parent != null)
                {
                    ((this.Parent as Frame).Parent as TabViewItem).Header = title;
                }
            }
            else
            {
                MessageCenter.ChangeTitle(this, title);
            }
        }
        private async void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedItem != null)
            {
                m_searchService.PivotIndexCache = pivot.SelectedIndex;
                var item = pivot.SelectedItem as ISearchPivotViewModel;
                if (!item.HasData && !item.Loading)
                {
                    await item.LoadData();
                }
            }

        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var data = (sender as ComboBox).DataContext as ISearchPivotViewModel;
            if (data.HasData && !data.Loading)
            {
                data.Refresh();
            }
        }
        private void Search_ItemClick(object sender, ItemClickEventArgs e)
        {
            SearchItemModelOpen(e.ClickedItem);
        }

        private void SearchItemModelOpen(object item, bool dontGoTo = false)
        {
            if (item is SearchVideoItem)
            {
                var data = item as SearchVideoItem;
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    page = typeof(VideoDetailPage),
                    title = data.title,
                    parameters = data.aid,
                    dontGoTo = dontGoTo
                });
                return;
            }
            if (item is SearchAnimeItem)
            {
                var data = item as SearchAnimeItem;
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    page = typeof(SeasonDetailPage),
                    title = data.title,
                    parameters = data.season_id,
                    dontGoTo = dontGoTo
                });
                return;
            }
            if (item is SearchUserItem)
            {
                var data = item as SearchUserItem;
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Contact,
                    title = data.uname,
                    page = typeof(UserInfoPage),
                    parameters = data.mid,
                    dontGoTo = dontGoTo
                });
                return;
            }
            if (item is SearchLiveRoomItem)
            {
                var data = item as SearchLiveRoomItem;
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    title = data.title,
                    page = typeof(LiveDetailPage),
                    parameters = data.roomid,
                    dontGoTo = dontGoTo
                });
                return;
            }
            if (item is SearchArticleItem)
            {
                var data = item as SearchArticleItem;
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Document,
                    page = typeof(WebPage),
                    title = data.title,
                    parameters = "https://www.bilibili.com/read/cv" + data.id,
                    dontGoTo = dontGoTo
                });
                return;
            }
            if (item is SearchTopicItem)
            {
                var data = item as SearchTopicItem;
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Document,
                    page = typeof(WebPage),
                    title = data.title,
                    parameters = data.arcurl,
                    dontGoTo = dontGoTo
                });
                return;
            }
        }

        private void Search_ItemPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!e.IsMiddleButtonNewTap(sender)) return;
            var element = e.OriginalSource as FrameworkElement;
            var item = element.DataContext;
            SearchItemModelOpen(item, true);
        }

        private void cbArea_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbArea.SelectedItem != null)
            {
                m_searchService.ComboIndexCache = cbArea.SelectedIndex;
                foreach (var item in m_viewModel.SearchItems)
                {
                    item.Area = m_viewModel.Area.area;
                    item.Page = 1;
                    item.HasData = false;
                }
                m_viewModel.SelectItem.Refresh();
            }
        }

        private async void txtKeyword_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput) return;
            var text = sender.Text;
            text = text.TrimEnd();
            if (string.IsNullOrWhiteSpace(text)) return;
            var suggestSearchContents = await m_searchService.GetSearchSuggestContents(text);
            if (m_viewModel.SuggestSearchContents == null)
            {
                m_viewModel.SuggestSearchContents = new System.Collections.ObjectModel.ObservableCollection<string>(suggestSearchContents);
            }
            else
            {
                m_viewModel.SuggestSearchContents.ReplaceRange(suggestSearchContents);
            }
        }

        public async Task Refresh()
        {
            if (!(pivot.SelectedItem is ISearchPivotViewModel searchVm)) return;
            searchVm.Refresh();
        }

        private void UpdateSize()
        {
            m_viewModel.PageWidth = ActualWidth;
            m_viewModel.PivotHeaderWidth =
                pivot.FindChildrenByType<PivotHeaderItem>().Select(x => x.ActualWidth).Sum();
        }

        private void SearchPage_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSize();
        }

        public void UpdatePivotLayout()
        {
            pivot.UseLayoutRounding = !pivot.UseLayoutRounding;
            pivot.UseLayoutRounding = !pivot.UseLayoutRounding;
        }
    }
}
