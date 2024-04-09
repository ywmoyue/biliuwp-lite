using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Models.Requests.Api;
using BiliLite.Modules.User.UserDetail;
using BiliLite.Pages.User;
using BiliLite.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using BiliLite.Models.Common.User;
using BiliLite.ViewModels.User;
using BiliLite.ViewModels.UserDynamic;
using Microsoft.Extensions.DependencyInjection;

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
    public sealed partial class UserInfoPage : BasePage
    {
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
            Notify.ShowComment(id, (int)commentType, CommentApi.CommentSort.Hot);
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
                if (e.Parameter is UserInfoParameter)
                {
                    var par = e.Parameter as UserInfoParameter;
                    mid = par.Mid;
                    tabIndex = (int)par.Tab;
                }
                else
                {
                    mid = e.Parameter.ToString();
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
                    appBar.Visibility = Visibility.Collapsed;
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
            if (m_viewModel.UserInfo == null) return;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Video,
                page = typeof(LiveDetailPage),
                title = m_viewModel.UserInfo.Name + "的直播间",
                parameters = m_viewModel.UserInfo.LiveRoom.RoomId
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
            if (pivotRight.SelectedIndex == 0 && splitView.IsPaneOpen && (repost.UserDynamicRepostViewModel.Items == null || repost.UserDynamicRepostViewModel.Items.Count == 0))
            {
                repost.LoadData(dynamic_id);
            }
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
            if (pivot.SelectedIndex == 1 && DynamicSpaceFrame.Content==null)
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
                page = typeof(WebPage),
                title = data.title,
                parameters = "https://www.bilibili.com/read/cv" + data.id
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
    }
}
