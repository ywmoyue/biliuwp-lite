using BiliLite.Models.Common;
using BiliLite.Pages.Bangumi;
using BiliLite.Services;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using BiliLite.Models.Common.Region;
using BiliLite.ViewModels.Region;
using BiliLite.Pages.Live;
using Google.Type;
using Newtonsoft.Json;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class RegionDetailPage : BasePage, IRefreshablePage
    {
        RegionDetailViewModel m_viewModel;
        OpenRegionInfo regionInfo;
        public RegionDetailPage()
        {
            this.InitializeComponent();
            Title = "分区详情";
            m_viewModel = new RegionDetailViewModel();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New||m_viewModel.Regions==null)
            {
                if (e.Parameter!=null)
                {
                    regionInfo = e.Parameter as OpenRegionInfo;
                    if (regionInfo == null)
                    {
                        regionInfo = JsonConvert.DeserializeObject<OpenRegionInfo>(JsonConvert.SerializeObject(e.Parameter));
                    }
                }
                else
                {
                    regionInfo = new OpenRegionInfo();
                }
                m_viewModel.InitRegion(regionInfo.id, regionInfo.tid);
            }
        }

        private async void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedItem==null)
            {
                return;
            }
            if(pivot.SelectedItem is RegionDetailHomeViewModel)
            {
                GridOrder.Visibility = Visibility.Collapsed;
                var data = pivot.SelectedItem as RegionDetailHomeViewModel;
                if (!data.Loading&&data.Banners==null)
                {
                    await data.LoadHome();
                }
            }
            else
            {
                var data = pivot.SelectedItem as RegionDetailChildViewModel;
                if (!data.Loading && data.Tasgs == null)
                {
                    await data.LoadHome();
                }
                GridOrder.Visibility = Visibility.Visible;
                GridOrder.DataContext = data;
               
            }
        }

        private void btnOpenRank_Click(object sender, RoutedEventArgs e)
        {
            if (regionInfo.id == 13)
            {
                //打开番剧排行榜
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.FourBars,
                    page = typeof(SeasonRankPage),
                    title = "热门榜单",
                    parameters = AnimeType.Bangumi
                });
                return;
            }
            if (regionInfo.id == 167)
            {
                //打开国创排行榜
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.FourBars,
                    page = typeof(SeasonRankPage),
                    title = "热门榜单",
                    parameters = AnimeType.GuoChuang
                });
                return;
            } 
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.FourBars,
                page = typeof(RankPage),
                title = "排行榜",
                parameters= regionInfo.id
            });
        }

        private  void cbTags_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTags.SelectedItem==null)
            {
                return;
            }
          (pivot.SelectedItem as RegionDetailChildViewModel).Refresh();

        }

        private void cbOrder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbOrder.SelectedItem == null)
            {
                return;
            }
            (pivot.SelectedItem as RegionDetailChildViewModel).Refresh();
        }

        private void AdaptiveGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as RegionVideoItemModel;

            // 连载动画/完结动画
            if (data.Rid == 33 || data.Rid == 32)
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    page = typeof(SeasonDetailPage),
                    parameters = data.Param,
                    title = data.Title
                });
                return;
            }

            MessageCenter.NavigateToPage(this,new NavigationInfo() { 
                icon= Symbol.Play,
                page=typeof(VideoDetailPage),
                parameters=data.Param,
                title=data.Title
            });
        }

        private async void BtnOpenBanner_Click(object sender, RoutedEventArgs e)
        {
           await MessageCenter.HandelUrl(((sender as HyperlinkButton).DataContext as RegionHomeBannerItemModel).Uri);
        }

        private void AddToWatchLater_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as MenuFlyoutItem).DataContext as RegionVideoItemModel;

            Modules.User.WatchLaterVM.Instance.AddToWatchlater(data.Param);
        }

        public async Task Refresh()
        {
            if (cbTags.SelectedItem == null)
            {
                return;
            }
            (pivot.SelectedItem as RegionDetailChildViewModel).Refresh();
        }
    }
    public class RegionDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate HomeTemplate { get; set; }

        public DataTemplate ChildTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is RegionDetailHomeViewModel)
            {
                return HomeTemplate;
            }
            else
            {
                return ChildTemplate;
            }
            

        }
    }

    public class OpenRegionInfo
    {
        /// <summary>
        /// 分区ID
        /// </summary>
        public int id { get; set; } = 1;
        /// <summary>
        /// 子分区ID
        /// </summary>
        public int tid { get; set; } = 0;
    }
}
