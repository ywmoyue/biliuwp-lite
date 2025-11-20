using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Models.Common.User;
using BiliLite.Models.Common.Video;
using BiliLite.Models.Requests.Api;
using BiliLite.Modules.User.UserDetail;
using BiliLite.Pages.User;
using BiliLite.Services;
using BiliLite.Services.Biz;
using BiliLite.Services.Interfaces;
using BiliLite.ViewModels.User;
using BiliLite.ViewModels.UserDynamic;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using BiliLite.Models.Common.Article;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages
{
    public enum UserTab
    {
        SubmitVideo = 0,
        Dynamic = 1,
        Article = 2,
        Collection = 3,
        Favorite = 4,
        Attention = 5,
        Fans = 6,
    }
    public class UserInfoParameter
    {
        public string Mid { get; set; }
        public UserTab Tab { get; set; } = UserTab.SubmitVideo;
    }
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class UserInfoPage : BasePage, IRefreshablePage, IUpdatePivotLayout
    {
        private readonly MediaListService m_mediaListService;
        readonly UserDynamicViewModel m_userDynamicViewModel;
        UserDetailViewModel m_viewModel;
        UserSubmitVideoViewModel m_userSubmitVideoViewModel;
        UserSubmitCollectionViewModel m_userSubmitCollectionViewModel;
        UserSubmitArticleVM userSubmitArticleVM;
        UserFavlistVM userFavlistVM;
        UserFollowVM fansVM;
        UserFollowVM followVM;
        private bool IsStaggered { get; set; } = false;
        bool isSelf = false;
        public UserInfoPage()
        {
            m_viewModel = App.ServiceProvider.GetRequiredService<UserDetailViewModel>();
            m_mediaListService = App.ServiceProvider.GetRequiredService<MediaListService>();
            this.InitializeComponent();
            Title = "用户中心";
            m_userSubmitVideoViewModel = App.ServiceProvider.GetService<UserSubmitVideoViewModel>();
            m_userSubmitCollectionViewModel = App.ServiceProvider.GetService<UserSubmitCollectionViewModel>();
            userSubmitArticleVM = new UserSubmitArticleVM();
            userFavlistVM = new UserFavlistVM();
            m_userDynamicViewModel = new UserDynamicViewModel();
            fansVM = new UserFollowVM(true);
            followVM = new UserFollowVM(false);
            m_userDynamicViewModel.OpenCommentEvent += UserDynamicViewModelOpenCommentEvent;
            splitView.PaneClosed += SplitView_PaneClosed;
            m_viewModel.LiveStreaming += (_, e) => btnLiveRoom.Label = "正在直播";
        }
        private void SplitView_PaneClosed(SplitView sender, object args)
        {
            comment.ClearComment();
            repost.UserDynamicRepostViewModel.Clear();
        }
        string dynamic_id;
        private void UserDynamicViewModelOpenCommentEvent(object sender, UserDynamicItemDisplayViewModel e)
        {
            //splitView.IsPaneOpen = true;
            dynamic_id = e.DynamicID;
            pivotRight.SelectedIndex = 1;
            repostCount.Text = e.ShareCount.ToString();
            commentCount.Text = e.CommentCount.ToString();
            CommentApi.CommentType commentType = CommentApi.CommentType.Dynamic;
            var id = e.ReplyID;
            switch (e.Type)
            {

                case UserDynamicDisplayType.Photo:
                    commentType = CommentApi.CommentType.Photo;
                    break;
                case UserDynamicDisplayType.Video:

                    commentType = CommentApi.CommentType.Video;
                    break;
                case UserDynamicDisplayType.Season:
                    id = e.OneRowInfo.AID;
                    commentType = CommentApi.CommentType.Video;
                    break;
                case UserDynamicDisplayType.ShortVideo:
                    commentType = CommentApi.CommentType.MiniVideo;
                    break;
                case UserDynamicDisplayType.Music:
                    commentType = CommentApi.CommentType.Song;
                    break;
                case UserDynamicDisplayType.Article:
                    commentType = CommentApi.CommentType.Article;
                    break;
                case UserDynamicDisplayType.MediaList:
                    if (e.OneRowInfo.Tag != "收藏夹")
                        commentType = CommentApi.CommentType.Video;
                    break;
                default:
                    id = e.DynamicID;
                    break;
            }
            NotificationShowExtensions.ShowCommentDialog(id, (int)commentType, CommentApi.CommentSort.Hot);
            //comment.LoadComment(new Controls.LoadCommentInfo()
            //{
            //    CommentMode = (int)commentType,
            //    CommentSort = Api.CommentApi.commentSort.Hot,
            //    Oid = id
            //});
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            //SetStaggered();
            if (e.NavigationMode == NavigationMode.New)
            {
                var mid = "";
                var tabIndex = 0;
                if (e.Parameter is string)
                {
                    mid = e.Parameter.ToString();
                }
                else
                {
                    if (e.Parameter is long userId)
                    {
                        mid = userId.ToString();
                    }
                    else
                    {
                        var par = e.Parameter as UserInfoParameter;
                        if (par == null)
                        {
                            par = JsonConvert.DeserializeObject<UserInfoParameter>(JsonConvert.SerializeObject(e.Parameter));
                        }
                        mid = par.Mid;
                        tabIndex = (int)par.Tab;
                    }
                }

                m_viewModel.Mid = mid;
                m_userSubmitVideoViewModel.Mid = mid;
                m_userSubmitCollectionViewModel.Mid = mid;
                userSubmitArticleVM.mid = mid;
                userFavlistVM.mid = mid;
                fansVM.mid = mid;
                followVM.mid = mid;
                if (m_viewModel.Mid == SettingService.Account.UserID.ToString())
                {
                    isSelf = true;
                    UserAppBar.Visibility = Visibility.Collapsed;
                    followHeader.Visibility = Visibility.Visible;
                }
                else
                {
                    isSelf = false;
                    followHeader.Visibility = Visibility.Collapsed;
                }
                m_userDynamicViewModel.DynamicType = DynamicType.Space;
                m_userDynamicViewModel.Uid = mid;
                m_viewModel.GetUserInfo();
                await UserFollowingTagsFlyout.Init(mid);
                if (tabIndex != 0)
                {
                    pivot.SelectedIndex = tabIndex;
                }
            }
        }

        private async void SubmitVideo_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as SubmitVideoItemModel;
            if (!string.IsNullOrEmpty(data.RedirectUrl))
            {
                var seasonId = await MessageCenter.HandelSeasonID(data.RedirectUrl);
                MessageCenter.NavigateToPage(this, new NavigationInfo()
                {
                    icon = Symbol.Play,
                    page = typeof(SeasonDetailPage),
                    title = data.Title,
                    parameters = seasonId
                });
                return;
            }
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Play,
                page = typeof(VideoDetailPage),
                title = data.Title,
                parameters = data.Aid
            });
        }

        private void btnLiveRoom_Click(object sender, RoutedEventArgs e)
        {
            if (!m_viewModel.HaveLiveRoom) return;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Video,
                page = typeof(LiveDetailPage),
                title = m_viewModel.UserSpaceInfo.Name + "的直播间",
                parameters = m_viewModel.UserSpaceInfo.LiveRoom.RoomId
            });
        }

        private void btnChat_Click(object sender, RoutedEventArgs e)
        {
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Message,
                title = "消息中心",
                page = typeof(WebPage),
                parameters = $"https://message.bilibili.com/#whisper/mid{m_viewModel.Mid}"
            });
        }

        private void searchVideo_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            m_userSubmitVideoViewModel.Refresh();
        }

        private void comVideoOrder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            m_userSubmitVideoViewModel.Keyword = "";
            m_userSubmitVideoViewModel?.Refresh();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (m_userSubmitVideoViewModel == null ||
                m_userSubmitVideoViewModel.CurrentTid == m_userSubmitVideoViewModel.SelectTid.Tid) return;
            m_userSubmitVideoViewModel.Keyword = "";
            m_userSubmitVideoViewModel?.Refresh();

        }

        private void pivotRight_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            throw new NotImplementedException();
            //if (pivotRight.SelectedIndex == 0 && splitView.IsPaneOpen && (repost.UserDynamicRepostViewModel.Items == null || repost.UserDynamicRepostViewModel.Items.Count == 0))
            //{
            //    repost.LoadData(dynamic_id);
            //}
        }

        //void SetStaggered()
        //{
        //    var staggered = SettingService.GetValue<int>(SettingConstants.UI.DYNAMIC_DISPLAY_MODE, 0) == 1;
        //    if (staggered != IsStaggered)
        //    {
        //        IsStaggered = staggered;
        //        if (staggered)
        //        {
        //            btnGrid_Click(this, null);
        //        }
        //        else
        //        {
        //            btnList_Click(this, null);
        //        }
        //    }
        //}

        //private void btnGrid_Click(object sender, RoutedEventArgs e)
        //{
        //    SettingService.SetValue<int>(SettingConstants.UI.DYNAMIC_DISPLAY_MODE, 1);
        //    IsStaggered = true;
        //    btnGrid.Visibility = Visibility.Collapsed;
        //    btnList.Visibility = Visibility.Visible;
        //    //XAML
        //    list.ItemsPanel = (ItemsPanelTemplate)this.Resources["GridPanel"];
        //}

        //private void btnList_Click(object sender, RoutedEventArgs e)
        //{
        //    IsStaggered = false;
        //    //右下角按钮
        //    btnGrid.Visibility = Visibility.Visible;
        //    btnList.Visibility = Visibility.Collapsed;
        //    //设置
        //    SettingService.SetValue<int>(SettingConstants.UI.DYNAMIC_DISPLAY_MODE, 0);
        //    //XAML
        //    list.ItemsPanel = (ItemsPanelTemplate)this.Resources["ListPanel"];
        //}

        //private void btnTop_Click(object sender, RoutedEventArgs e)
        //{
        //    list.ScrollIntoView(list.Items[0]);
        //}

        private async void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedIndex == 0 && m_userSubmitVideoViewModel.SubmitVideoItems == null)
            {
                await m_userSubmitVideoViewModel.GetSubmitVideo();
            }
            if (pivot.SelectedIndex == 1 && DynamicSpaceFrame.Content == null)
            {
                DynamicSpaceFrame.Navigate(typeof(DynamicSpacePage), m_viewModel.Mid);
                //await m_userDynamicViewModel.GetDynamicItems();
            }
            if (pivot.SelectedIndex == 2 && userSubmitArticleVM.SubmitArticleItems == null)
            {
                await userSubmitArticleVM.GetSubmitArticle();
            }
            if (pivot.SelectedIndex == 3 && m_userSubmitCollectionViewModel.SubmitCollectionItems == null)
            {
                await m_userSubmitCollectionViewModel.GetSubmitCollection();
            }
            if (pivot.SelectedIndex == 4 && userFavlistVM.Items == null)
            {
                await userFavlistVM.Get();
            }
            if (pivot.SelectedIndex == 5 && followVM.Items == null)
            {
                if (isSelf)
                {
                    await followVM.GetTags();
                }

                await followVM.Get();
            }
            if (pivot.SelectedIndex == 6 && fansVM.Items == null)
            {
                await fansVM.Get();
            }
        }

        private void SubmitArticle_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as SubmitArticleItemModel;
            MessageCenter.NavigateToPage(null, new NavigationInfo()
            {
                icon = Symbol.Document,
                page = typeof(ArticlePage),
                title = data.title,
                parameters = new ArticlePageNavigationInfo()
                {
                    Url = "https://www.bilibili.com/read/cv" + data.id,
                    CvId = data.id + "",
                }
            });
        }

        private async void SubmitCollection_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!(e.ClickedItem is SubmitCollectionItemModel data)) return;
            await MessageCenter.HandelUrl(data.Uri);
        }

        private void comArticleOrder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            userSubmitArticleVM?.Refresh();
        }

        private void FavList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as FavFolderItemModel;
            MessageCenter.NavigateToPage(null, new NavigationInfo()
            {
                icon = Symbol.OutlineStar,
                page = typeof(FavoriteDetailPage),
                title = "收藏夹",
                parameters = new FavoriteDetailArgs()
                {
                    Id = data.id.ToString(),
                }
            });
        }

        private void UserList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var data = e.ClickedItem as UserFollowItemModel;
            MessageCenter.NavigateToPage(null, new NavigationInfo()
            {
                icon = Symbol.Contact,
                page = typeof(UserInfoPage),
                title = data.uname,
                parameters = data.mid
            });
        }

        private void AddToWatchLater_Click(object sender, RoutedEventArgs e)
        {
            var data = (sender as MenuFlyoutItem).DataContext as SubmitVideoItemModel;
            Modules.User.WatchLaterVM.Instance.AddToWatchlater(data.Aid);
        }

        private void comFollowOrder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            followVM.Refresh();
        }

        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (followVM != null && followVM.CurrentTid != followVM.SelectTid.TagId)
            {
                if (followVM.SelectTid.TagId == -1)
                {
                    searchFollow.Visibility = Visibility.Visible;
                }
                else
                {
                    searchFollow.Visibility = Visibility.Collapsed;
                }
                followVM.Refresh();
            }
        }

        private void searchFollow_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            followVM.Refresh();
        }

        private void BtnFollowingTag_OnClick(object sender, RoutedEventArgs e)
        {
            UserFollowingTagsFlyout.ShowAt(sender as DependencyObject);
        }

        public async Task Refresh()
        {
            throw new System.NotImplementedException();
        }

        private async void BtnPlayAll_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var items = new List<VideoPlaylistItem>();
            var mediaList = await m_mediaListService.GetMediaList(m_userSubmitVideoViewModel.PlayAllMediaListId, 1);

            if (mediaList == null)
            {
                return;
            }

            foreach (var item in mediaList.MediaList)
            {
                items.Add(new VideoPlaylistItem()
                {
                    Cover = item.Cover,
                    Author = item.Upper.Name,
                    Id = item.Id.ToString(),
                    Title = item.Title,
                    Duration = TimeSpan.FromSeconds(item.Duration),
                });
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
                    Title = $"{m_viewModel.UserSpaceInfo.Name}:全部视频",
                    MediaListId = m_userSubmitVideoViewModel.PlayAllMediaListId,
                    IsOnlineMediaList = true,
                    Info = $"共{mediaList.TotalCount}集"
                }
            });
        }

        private void UpdateSize()
        {
            m_viewModel.PivotHeaderWidth =
                pivot.Items.Select(x => (((x as PivotItem).Header as FrameworkElement).Parent as FrameworkElement).ActualWidth).Sum();
            m_viewModel.UserBarWidth = UserBar.ActualWidth;
            m_viewModel.PageWidth = ActualWidth;
        }

        private void UserInfoPage_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSize();
        }

        private void UserInfoPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateSize();
        }

        private void UserBar_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSize();
        }

        public void UpdatePivotLayout()
        {
            pivot.UseLayoutRounding = !pivot.UseLayoutRounding;
            pivot.UseLayoutRounding = !pivot.UseLayoutRounding;
        }

        private void BtnSetLocalAttention_OnClick(object sender, RoutedEventArgs e)
        {
            var localAttentionUserService = App.ServiceProvider.GetRequiredService<LocalAttentionUserService>();
            localAttentionUserService.AttentionUp(m_viewModel.Mid, m_viewModel.UserSpaceInfo.Name);
        }

        private void BtnCancelLocalAttention_OnClick(object sender, RoutedEventArgs e)
        {
            var localAttentionUserService = App.ServiceProvider.GetRequiredService<LocalAttentionUserService>();
            localAttentionUserService.CancelAttention(m_viewModel.Mid);
        }
    }
}
