using System;
using BiliLite.Models.Common;
using BiliLite.Modules.User;
using BiliLite.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using BiliLite.Models.Common.Video;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages.User
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class WatchlaterPage : BasePage, IRefreshablePage
    {
        WatchLaterVM watchLaterVM;
        public WatchlaterPage()
        {
            this.InitializeComponent();
            Title = "稍后再看";
            watchLaterVM = new WatchLaterVM();
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                await watchLaterVM.LoadData();
            }
        }

        private void Video_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (watchLaterVM.Videos == null) return;
            var data = e.ClickedItem as WatchlaterItemModel;
            List<VideoPlaylistItem> items = new List<VideoPlaylistItem>();
            foreach (var item in watchLaterVM.Videos)
            {
                if (item.title != "已失效视频")
                {
                    items.Add(new VideoPlaylistItem()
                    {
                        Cover = item.pic,
                        Author = item.owner.name,
                        Id = item.aid,
                        Title = item.title,
                        Duration = TimeSpan.FromSeconds(item.duration),
                    });
                }

            }
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Play,
                page = typeof(VideoDetailPage),
                title = "视频播放",
                parameters = new VideoPlaylist()
                {
                    Index = watchLaterVM.Videos.IndexOf(data),
                    Playlist = items,
                    Title = $"稍后再看"
                }
            });
        }

        public async Task Refresh()
        {
            watchLaterVM.Refresh();
        }
    }
}
