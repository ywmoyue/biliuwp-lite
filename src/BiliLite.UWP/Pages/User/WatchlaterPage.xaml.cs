using System;
using BiliLite.Models.Common;
using BiliLite.Modules.User;
using BiliLite.Services;
using BiliLite.Services.Biz;
using BiliLite.Extensions.Notifications;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using BiliLite.Models.Common.Video;
using BiliLite.Models.Common.User.WatchLater;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Microsoft.UI.Xaml;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages.User
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class WatchlaterPage : BasePage, IRefreshablePage
    {
        WatchLaterViewModel watchLaterVM;
        public WatchlaterPage()
        {
            this.InitializeComponent();
            Title = "稍后再看";
            watchLaterVM = new WatchLaterViewModel();
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
                        IsWatchlaterItem = true,
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

        private async void CleanFinishedButton_Click(object sender, RoutedEventArgs e)
        {
            if (watchLaterVM.Videos == null || watchLaterVM.Videos.Count == 0)
            {
                NotificationShowExtensions.ShowMessageToast("稍后再看列表为空");
                return;
            }

            var watchLaterService = App.ServiceProvider.GetRequiredService<WatchLaterService>();
            var finishedVideos = watchLaterService.FindFinishedVideos(watchLaterVM.Videos.ToList());

            if (finishedVideos.Count == 0)
            {
                NotificationShowExtensions.ShowMessageToast("未找到已看完的视频");
                return;
            }

            var result = await NotificationShowExtensions.ShowMessageDialog(
                "移除已看完视频", 
                $"已找到{finishedVideos.Count}个已看完视频，是否全部移除？"
            );

            if (!result) return;

            NotificationShowExtensions.ShowMessageToast("批量操作中...");

            var successCount = await watchLaterService.RemoveFinishedVideos(finishedVideos);

            foreach (var video in finishedVideos)
            {
                watchLaterVM.Videos.Remove(video);
            }

            NotificationShowExtensions.ShowMessageToast($"已成功移除{successCount}个已看完视频");
        }
    }
}
