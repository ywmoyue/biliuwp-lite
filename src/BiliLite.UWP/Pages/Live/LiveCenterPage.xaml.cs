using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Modules.Live.LiveCenter;
using BiliLite.Services;
using BiliLite.Services.Interfaces;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages.Live
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LiveCenterPage : BasePage, IRefreshablePage, IUpdatePivotLayout
    {
        readonly LiveAttentionVM liveAttentionVM;
        readonly LiveAttentionUnLiveVM liveAttentionUnLiveVM;
        readonly LiveCenterHistoryVM liveCenterHistoryVM;
        readonly LiveCenterVM liveCenterVM;
        public LiveCenterPage()
        {
            this.InitializeComponent();
            Title = "直播中心";
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            liveAttentionVM = new LiveAttentionVM();
            liveAttentionUnLiveVM = new LiveAttentionUnLiveVM();
            liveCenterHistoryVM = new LiveCenterHistoryVM();
            liveCenterVM = new LiveCenterVM();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                liveCenterVM.GetUserInfo();
                await liveAttentionVM.GetFollows();
            }
        }

        private void AttentionlList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as LiveFollowAnchorModel;
            OpenAttentionlItem(data);
        }

        private void OpenAttentionlItem(LiveFollowAnchorModel data, bool dontGoTo = false)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Video,
                page = typeof(LiveDetailPage),
                title = data.uname + "的直播间",
                parameters = data.roomid,
                dontGoTo = dontGoTo,
            });
        }

        private void UnLiveList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as LiveFollowUnliveAnchorModel;
            OpenUnLiveItem(data);
        }

        private void OpenUnLiveItem(LiveFollowUnliveAnchorModel data, bool dontGoTo = false)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Video,
                page = typeof(LiveDetailPage),
                title = data.uname + "的直播间",
                parameters = data.roomid,
                dontGoTo = dontGoTo,
            });
        }

        private void HistoryList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as LiveHistoryItemModel;
            OpenHistoryItem(data);
        }

        private void OpenHistoryItem(LiveHistoryItemModel data, bool dontGoTo = false)
        {
            var bizId = data.roomid;
            if (bizId == 0)
            {
                bizId = data.history.oid;
            }
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Video,
                page = typeof(LiveDetailPage),
                title = data.name + "的直播间",
                parameters = bizId,
                dontGoTo = dontGoTo,
            });
        }

        private async void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedIndex == 1 && liveAttentionUnLiveVM.Items == null)
            {
                await liveAttentionUnLiveVM.Get();
            }
            if (pivot.SelectedIndex == 2 && liveCenterHistoryVM.Items == null)
            {
                await liveCenterHistoryVM.Get();
            }
        }

        public async Task Refresh()
        {
            throw new NotImplementedException();
        }

        public void UpdatePivotLayout()
        {
            pivot.UseLayoutRounding = !pivot.UseLayoutRounding;
            pivot.UseLayoutRounding = !pivot.UseLayoutRounding;
        }

        private void LiveItem_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!e.IsMiddleButtonNewTap(sender)) return;
            var element = e.OriginalSource as FrameworkElement;
            if (element.DataContext is LiveFollowAnchorModel attentionItem)
            {
                OpenAttentionlItem(attentionItem, true);
            }
            else if (element.DataContext is LiveFollowUnliveAnchorModel unLiveItem)
            {
                OpenUnLiveItem(unLiveItem, true);
            }
            else if (element.DataContext is LiveHistoryItemModel historyItem)
            {
                OpenHistoryItem(historyItem, true);
            }
        }
    }
}
