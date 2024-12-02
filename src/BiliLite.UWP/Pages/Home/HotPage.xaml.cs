using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Modules.User;
using BiliLite.Services;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using BiliLite.Models.Common.Home;
using BiliLite.ViewModels.Home;
using Microsoft.Extensions.DependencyInjection;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages.Home
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class HotPage : Page, IRefreshablePage, IScrollRecoverablePage
    {
        private readonly HotViewModel m_viewModel;

        public HotPage()
        {
            this.InitializeComponent();
            m_viewModel = App.ServiceProvider.GetRequiredService<HotViewModel>();
            this.DataContext = m_viewModel;
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
            if (e.NavigationMode == NavigationMode.New && m_viewModel.HotItems == null)
            {
                await m_viewModel.GetPopular();
            }
        }

        private async void gridHot_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = (e.ClickedItem as HotDataItemModel);
            await HotDataItemModelOpen(sender, data);
        }

        private async Task HotDataItemModelOpen(object sender, HotDataItemModel item, bool dontGoTo = false)
        {
            if (item == null) return;
            if (item.CardGoto == "av")
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    page = typeof(VideoDetailPage),
                    title = item.Title,
                    parameters = item.Param,
                    dontGoTo = dontGoTo
                });
            }
            else
            {
                await MessageCenter.HandelUrl(item.Uri, dontGoTo);
            }
        }

        private async void gridHot_ItemPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!e.IsMiddleButtonNewTap(sender)) return;
            var element = e.OriginalSource as FrameworkElement;
            var item = element.DataContext as HotDataItemModel;
            await HotDataItemModelOpen(sender, item, true);
        }

        private async void gridTop_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = (e.ClickedItem as HotTopItemModel);
            if (data.ModuleId == "rank")
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.FourBars,
                    page = typeof(RankPage),
                    title = "排行榜"
                });
            }
            else
            {
                await MessageCenter.HandelUrl(data.Uri);
            }
        }

        private void AddToWatchLater_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as MenuFlyoutItem).DataContext as HotDataItemModel;
            WatchLaterVM.Instance.AddToWatchlater(data.Param);
        }

        public async Task Refresh()
        {
            m_viewModel.Refresh();
        }

        public async void ScrollRecover()
        {
            await HotGridView.ScrollRecover();
        }
    }
}
