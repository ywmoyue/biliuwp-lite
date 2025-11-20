using BiliLite.Models.Common;
using BiliLite.Modules.Live;
using BiliLite.Services;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages.Live
{
    public class LiveAreaPar
    {
        public int area_id { get; set; } = 0;
        public int parent_id { get; set; } = 0;
    }
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LiveAreaDetailPage : BasePage, IRefreshablePage
    {
        LiveAreaDetailVM liveAreaDetailVM;
        public LiveAreaDetailPage()
        {
            this.InitializeComponent();
            Title = "分区详情";
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New && liveAreaDetailVM == null)
            {
                var data = e.Parameter as LiveAreaPar;
                if (data == null)
                {
                    data = JsonConvert.DeserializeObject<LiveAreaPar>(JsonConvert.SerializeObject(e.Parameter));
                }
                liveAreaDetailVM = new LiveAreaDetailVM(data.area_id, data.parent_id);
                await liveAreaDetailVM.GetItems();
            }
        }

        private async void RefreshContainer_RefreshRequested(Microsoft.UI.Xaml.Controls.RefreshContainer sender, Microsoft.UI.Xaml.Controls.RefreshRequestedEventArgs args)
        {
            await Refresh();
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

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as ToggleButton).DataContext as LiveTagItemModel;
            if (data.Select) return;
            var select = liveAreaDetailVM.Tags.FirstOrDefault(x => x.Select);
            select.Select = false;
            data.Select = true;
            liveAreaDetailVM.SelectTag = data;
            liveAreaDetailVM.Refresh();
        }

        public async Task Refresh()
        {
            liveAreaDetailVM.Refresh();
        }
    }
}
