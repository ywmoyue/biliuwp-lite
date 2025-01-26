using BiliLite.Models.Common;
using BiliLite.Modules.Live;
using BiliLite.Services;
using BiliLite.Services.Interfaces;
using System.Threading.Tasks;

using Windows.UI.Xaml.Controls;

using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages.Live
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LiveRecommendPage : BasePage, IRefreshablePage, IUpdatePivotLayout
    {
        readonly LiveRecommendVM liveRecommendVM;
        public LiveRecommendPage()
        {
            this.InitializeComponent();
            Title = "推荐直播";
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            liveRecommendVM = new LiveRecommendVM();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
            {
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
            }
            base.OnNavigatingFrom(e);
        }
        private async void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedItem == null)
            {
                return;
            }
            var item = pivot.SelectedItem as LiveRecommendItem;
            if (item.Items.Count == 0 && !item.Loading)
            {
                await item.GetItems();
            }


        }

        private void AdaptiveGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as LiveRecommendItemModel;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Video,
                page = typeof(LiveDetailPage),
                title = data.uname + "的直播间",
                parameters = data.roomid
            });
        }

        private async void RefreshContainer_RefreshRequested(Microsoft.UI.Xaml.Controls.RefreshContainer sender, Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs args)
        {
            await Refresh();
        }

        public async Task Refresh()
        {
            if (pivot.SelectedItem is LiveRecommendItem item)
            {
                item.Refresh();
            }
        }

        public void UpdatePivotLayout()
        {
            pivot.UseLayoutRounding = !pivot.UseLayoutRounding;
            pivot.UseLayoutRounding = !pivot.UseLayoutRounding;
        }
    }
}
