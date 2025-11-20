using BiliLite.Models.Common;
using BiliLite.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using BiliLite.Models.Common.Home;
using BiliLite.ViewModels.Home;
using Microsoft.Extensions.DependencyInjection;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages.Home
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class RegionsPage : Page
    {
        private readonly RegionViewModel m_viewModel;

        public RegionsPage()
        {
            m_viewModel = App.ServiceProvider.GetRequiredService<RegionViewModel>();
            this.InitializeComponent();
            if (SettingService.GetValue<bool>(SettingConstants.UI.CACHE_HOME, true))
            {
                this.NavigationCacheMode = NavigationCacheMode.Enabled;
            }
            else
            {
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New && m_viewModel.Regions == null)
            {
                await m_viewModel.GetRegions();
            }

        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as RegionItem;

            // TODO: 专栏/VLOG分区临时使用webview处理
            if (item.Name == "VLOG")
            {
                item.Uri = "https://www.bilibili.com/v/life/daily/?tag=530003";
            }

            if (item.Name == "专栏")
            {
                item.Uri = "https://www.bilibili.com/read/home";
            }

            if (item.Uri.Contains("http"))
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.World,
                    page = typeof(WebPage),
                    title = item.Name,
                    parameters = item.Uri
                });
                return;
            }
            if (item.Children != null)
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Document,
                    page = typeof(Pages.RegionDetailPage),
                    title = item.Name,
                    parameters = new OpenRegionInfo()
                    {
                        id = item.Tid
                    }
                });
                return;
            }
            if (item.Name == "番剧")
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Home,
                    page = typeof(Pages.Home.AnimePage),
                    title = item.Name,
                    parameters = AnimeType.Bangumi
                });
                return;
            }
            if (item.Name == "国创")
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Home,
                    page = typeof(Pages.Home.AnimePage),
                    title = item.Name,
                    parameters = AnimeType.Bangumi
                });
                return;
            }
            if (item.Name == "放映厅")
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Home,
                    page = typeof(Pages.Home.MoviePage),
                    title = item.Name
                });
                return;
            }
            if (item.Name == "直播")
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Home,
                    page = typeof(Pages.Home.LivePage),
                    title = item.Name
                });
                return;
            }
            if (item.Name == "全区排行榜")
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.FourBars,
                    page = typeof(RankPage),
                    title = "排行榜"
                });
                return;
            }
        }
    }
}
