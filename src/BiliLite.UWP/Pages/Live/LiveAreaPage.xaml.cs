using BiliLite.Models.Common;
using BiliLite.Modules.Live;
using BiliLite.Services;
using BiliLite.Services.Interfaces;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages.Live
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LiveAreaPage : BasePage, IUpdatePivotLayout
    {
        readonly LiveAreaVM liveAreaVM;
        public LiveAreaPage()
        {
            this.InitializeComponent();
            Title = "直播分区";
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            liveAreaVM = new LiveAreaVM();
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New && liveAreaVM.Items == null)
            {
                await liveAreaVM.GetItems();
            }
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var area = e.ClickedItem as LiveAreaItemModel;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Document,
                page = typeof(LiveAreaDetailPage),
                title = area.name,
                parameters = new LiveAreaPar()
                {
                    parent_id = area.parent_id,
                    area_id = area.id
                }
            });
        }

        public void UpdatePivotLayout()
        {
            pivot.UseLayoutRounding = !pivot.UseLayoutRounding;
            pivot.UseLayoutRounding = !pivot.UseLayoutRounding;
        }
    }
}
