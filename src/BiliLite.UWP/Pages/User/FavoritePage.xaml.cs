using BiliLite.Dialogs;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Modules;
using BiliLite.Services;
using BiliLite.Services.Interfaces;
using BiliLite.ViewModels.User;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages.User
{
    public enum OpenFavoriteType
    {
        Video = 0,
        Bangumi = 1,
        Cinema = 2,
        Music = 3
    }
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FavoritePage : BasePage, IRefreshablePage, IUpdatePivotLayout
    {
        MyFollowSeasonVM animeVM;
        MyFollowSeasonVM cinemaVM;
        MyFollowVideoViewModel m_videoViewModel;
        public FavoritePage()
        {
            this.InitializeComponent();
            Title = "我的收藏";
            animeVM = new MyFollowSeasonVM(true);
            cinemaVM = new MyFollowSeasonVM(false);
            m_videoViewModel = App.ServiceProvider.GetRequiredService<MyFollowVideoViewModel>();
        }
        OpenFavoriteType openFavoriteType = OpenFavoriteType.Video;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                if (e.Parameter != null)
                {
                    openFavoriteType = (OpenFavoriteType)(e.Parameter.ToInt32());
                }
                if (openFavoriteType == OpenFavoriteType.Bangumi)
                {
                    pivot.SelectedIndex = 1;
                }
                if (openFavoriteType == OpenFavoriteType.Cinema)
                {
                    pivot.SelectedIndex = 2;
                }
            }
        }

        private async void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedItem == null)
            {
                return;
            }
            switch (pivot.SelectedIndex)
            {
                case 0:
                    if (m_videoViewModel.Loading || m_videoViewModel.MyFavorite != null)
                    {
                        return;
                    }
                    await m_videoViewModel.LoadFavorite();
                    break;
                case 1:
                    if (animeVM.Loading || animeVM.Follows != null)
                    {
                        return;
                    }
                    await animeVM.LoadFollows();
                    break;
                case 2:
                    if (cinemaVM.Loading || cinemaVM.Follows != null)
                    {
                        return;
                    }
                    await cinemaVM.LoadFollows();
                    break;
                default:
                    break;
            }
        }

        private void BangumiSeason_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as FollowSeasonModel;
            FollowSeasonModelOpen(sender, data);
        }

        private void FollowSeasonModelOpen(object sender, FollowSeasonModel item, bool dontGoTo = false)
        {
            if (item == null) return;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Play,
                page = typeof(SeasonDetailPage),
                title = item.Title,
                parameters = item.SeasonId,
                dontGoTo = dontGoTo
            });
        }

        private void BangumiSeason_ItemPressed(object sender, PointerRoutedEventArgs e)
        {
            if (!e.IsMiddleButtonNewTap(sender)) return;
            var element = e.OriginalSource as FrameworkElement;
            var item = element.DataContext as FollowSeasonModel;
            FollowSeasonModelOpen(sender, item, true);
        }

        private void VideoFavorite_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as FavoriteItemViewModel;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.OutlineStar,
                page = typeof(FavoriteDetailPage),
                title = data.Title,
                parameters = new FavoriteDetailArgs()
                {
                    Id = data.Id,
                    Type = data.Type
                }
            });
        }

        private async void btnCreateFavBox_Click(object sender, RoutedEventArgs e)
        {
            CreateFavFolderDialog createFavFolderDialog = new CreateFavFolderDialog();
            await createFavFolderDialog.ShowAsync();
            m_videoViewModel.Refresh();
        }

        private async void btnFavBoxEdit_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as MenuFlyoutItem).DataContext as FavoriteItemViewModel;
            EditFavFolderDialog editFavFolderDialog = new EditFavFolderDialog(data.Id, data.Title, data.Intro, data.Privacy ? false : true);
            await editFavFolderDialog.ShowAsync();
            m_videoViewModel.Refresh();
        }

        private async void btnFavBoxDel_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as MenuFlyoutItem).DataContext as FavoriteItemViewModel;
            await m_videoViewModel.DelFavorite(data.Id);
            m_videoViewModel.Refresh();
        }

        public async Task Refresh()
        {
            m_videoViewModel.Refresh();
        }

        private void VideoFavGridView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            if (e.Items.Count == 0)
                return;

            // 获取被拖拽的项
            var draggedItem = e.Items[0];

            // 获取数据源集合
            var collection = m_videoViewModel.MyFavorite;

            // 检查项是否为第一个元素
            if (collection.IndexOf(draggedItem) == 0)
            {
                e.Cancel = true; // 取消拖拽操作
            }
        }

        private async void VideoFavGridView_OnDragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            await m_videoViewModel.SortMyFavorite();
        }

        private void BtnShowAllCollected_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                dontGoTo = false,
                icon = Symbol.Favorite,
                page = typeof(CollectedPage),
                title = "我的收藏与订阅"
            });
        }

        private async void BtnCollectedDel_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is MenuFlyoutItem menuItem)) return;
            var data = menuItem.DataContext as FavoriteItemViewModel;
            await m_videoViewModel.CancelCollected(data);
        }

        public void UpdatePivotLayout()
        {
            pivot.UseLayoutRounding = !pivot.UseLayoutRounding;
            pivot.UseLayoutRounding = !pivot.UseLayoutRounding;
        }
    }
}
