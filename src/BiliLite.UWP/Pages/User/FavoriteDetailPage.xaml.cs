using BiliLite.Dialogs;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using BiliLite.Models.Common.Favorites;
using BiliLite.Models.Common.Video;
using BiliLite.ViewModels.Favourites;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages.User
{
    public class FavoriteDetailArgs
    {
        public int Type { get; set; } = 11;
        public string Id { get; set; }
    }
    /// <summary>
    /// 收藏夹详情、播放列表详情
    /// </summary>
    public sealed partial class FavoriteDetailPage : BasePage, IRefreshablePage
    {
        private readonly FavoriteDetailViewModel m_viewModel;

        public FavoriteDetailPage()
        {
            m_viewModel = App.ServiceProvider.GetRequiredService<FavoriteDetailViewModel>();
            this.InitializeComponent();
            Title = "收藏夹详情";
        }
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New && m_viewModel.FavoriteInfo == null)
            {
                FavoriteDetailArgs args = e.Parameter as FavoriteDetailArgs;
                if (args == null)
                {
                    args = JsonConvert.DeserializeObject<FavoriteDetailArgs>(JsonConvert.SerializeObject(e.Parameter));
                }
                m_viewModel.Id = args.Id;
                m_viewModel.Type = args.Type;
                m_viewModel.Page = 1;
                m_viewModel.Keyword = "";
                await m_viewModel.LoadFavoriteInfo();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private void Video_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as FavoriteInfoVideoItemModel;
            FavoriteInfoVideoItemModelOpen(sender, data);
        }

        private void FavoriteInfoVideoItemModelOpen(object sender, FavoriteInfoVideoItemModel item, bool dontGoTo = false)
        {
            if (item == null) return;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Play,
                page = typeof(VideoDetailPage),
                title = item.Title,
                parameters = item.Id,
                dontGoTo = dontGoTo
            });
        }

        private void Video_ItemPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!e.IsMiddleButtonNewTap(sender)) return;
            var element = e.OriginalSource as FrameworkElement;
            var item = element.DataContext as FavoriteInfoVideoItemModel;
            FavoriteInfoVideoItemModelOpen(sender, item, true);
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            m_viewModel.Search(searchBox.Text);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            listView.SelectAll();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            listView.SelectedItems.Clear();
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {
                if (!await Notify.ShowDialog("批量取消收藏", $"是否确定要取消收藏选中的{listView.SelectedItems.Count}个视频?"))
                {
                    return;
                }
                List<FavoriteInfoVideoItemModel> ls = new List<FavoriteInfoVideoItemModel>();
                foreach (FavoriteInfoVideoItemModel item in listView.SelectedItems)
                {
                    ls.Add(item);
                }
                await m_viewModel.Delete(ls);
            }
        }

        private async void btnMove_Click(object sender, RoutedEventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {
                List<FavoriteInfoVideoItemModel> ls = new List<FavoriteInfoVideoItemModel>();
                foreach (FavoriteInfoVideoItemModel item in listView.SelectedItems)
                {
                    ls.Add(item);
                }
                CopyOrMoveFavVideoDialog copyOrMoveFavVideoDialog = new CopyOrMoveFavVideoDialog(m_viewModel.Id, m_viewModel.FavoriteInfo.Mid, true, ls);
                await copyOrMoveFavVideoDialog.ShowAsync();
                m_viewModel.Refresh();
            }
        }

        private async void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (listView.SelectedItems.Count > 0)
            {
                List<FavoriteInfoVideoItemModel> ls = new List<FavoriteInfoVideoItemModel>();
                foreach (FavoriteInfoVideoItemModel item in listView.SelectedItems)
                {
                    ls.Add(item);
                }
                CopyOrMoveFavVideoDialog copyOrMoveFavVideoDialog = new CopyOrMoveFavVideoDialog(m_viewModel.Id, m_viewModel.FavoriteInfo.Mid, false, ls);
                await copyOrMoveFavVideoDialog.ShowAsync();
            }
        }

        private async void btnClean_Click(object sender, RoutedEventArgs e)
        {
            if (!await Notify.ShowDialog("清除失效", $"是否确定要清除已失效的视频?\r\n失效视频说不定哪天就恢复了哦~"))
            {
                return;
            }

            await m_viewModel.Clean();
        }

        private void AddToWatchLater_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as MenuFlyoutItem).DataContext as FavoriteInfoVideoItemModel;
            Modules.User.WatchLaterVM.Instance.AddToWatchlater(data.Id);
        }

        private async void PlayAll_Click(object sender, RoutedEventArgs e)
        {

            if (m_viewModel.ShowLoadMore)
            {
                Notify.ShowMessageToast("正在读取全部视频，请稍后");
                while (m_viewModel.ShowLoadMore)
                {
                    await m_viewModel.LoadFavoriteInfo();
                }
            }
            List<VideoPlaylistItem> items = new List<VideoPlaylistItem>();
            foreach (var item in m_viewModel.Videos)
            {
                if (item.Title != "已失效视频")
                {
                    items.Add(new VideoPlaylistItem()
                    {
                        Cover = item.Cover,
                        Author = item.Upper.Name,
                        Id = item.Id,
                        Title = item.Title,
                        Duration = TimeSpan.FromSeconds(item.Duration),
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
                    Index = 0,
                    Playlist = items,
                    Title = $"收藏夹:{m_viewModel.FavoriteInfo.Title}"
                }
            });
        }

        public async Task Refresh()
        {
            m_viewModel.Refresh();
        }

        private async void FavItemGridView_OnDragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            var item = args.Items.FirstOrDefault();
            if (!(item is FavoriteInfoVideoItemModel favVideo)) return;
            var endIndex = m_viewModel.Videos.IndexOf(favVideo);
            var targetId = "";
            if (endIndex != 0)
            {
                var target = m_viewModel.Videos[endIndex - 1];
                targetId = target.Id;
            }

            await m_viewModel.Sort(favVideo.Id, targetId);
        }
    }
}
