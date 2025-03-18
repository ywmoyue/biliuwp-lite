using BiliLite.Models.Requests.Api;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using BiliLite.Models.Common;
using BiliLite.ViewModels.UserDynamic;
using Microsoft.Extensions.DependencyInjection;
using BiliLite.Models.Common.Comment;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages.User
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class DynamicDetailPage : BasePage
    {
        private readonly UserDynamicDetailViewModel m_viewModel;

        public DynamicDetailPage()
        {
            m_viewModel = App.ServiceProvider.GetRequiredService<UserDynamicDetailViewModel>();
            m_viewModel.OpenCommentEvent += UserDynamicViewModelOpenCommentEvent;
            this.InitializeComponent();
            Title = "动态详情";
            splitView.PaneClosed += SplitView_PaneClosed;
        }
        private void SplitView_PaneClosed(SplitView sender, object args)
        {
            Comment.ClearComment();
            repost.UserDynamicRepostViewModel.Clear();
        }

        string dynamic_id;
        private void UserDynamicViewModelOpenCommentEvent(object sender, DynamicV2ItemViewModel e)
        {
            CommentApi.CommentType commentType = CommentApi.CommentType.Dynamic;
            var id = e.Extend.BusinessId;
            switch (e.CardType)
            {
                case Constants.DynamicTypes.DRAW:
                    commentType = CommentApi.CommentType.Photo;
                    break;
                case Constants.DynamicTypes.AV:
                case Constants.DynamicTypes.UGC_SEASON:
                    commentType = CommentApi.CommentType.Video;
                    break;
                case Constants.DynamicTypes.PGC:
                    id = e.Dynamic.DynPgc.Aid.ToString();
                    commentType = CommentApi.CommentType.Video;
                    break;
                //case UserDynamicDisplayType.ShortVideo:
                //    commentType = CommentApi.CommentType.MiniVideo;
                //    break;
                case Constants.DynamicTypes.MUSIC:
                    commentType = CommentApi.CommentType.Song;
                    break;
                case Constants.DynamicTypes.ARTICLE:
                    commentType = CommentApi.CommentType.Article;
                    break;
                //case UserDynamicDisplayType.MediaList:
                //    if (e.OneRowInfo.Tag != "收藏夹")
                //        commentType = CommentApi.CommentType.Video;
                //    break;
                default:
                    id = e.Extend.DynIdStr;
                    break;
            }

            OpenCommentCore(id, (int)commentType, CommentApi.CommentSort.Hot);
        }

        private void OpenCommentCore(string oid, int commentMode, CommentApi.CommentSort commentSort)
        {
            Comment.LoadComment(new LoadCommentInfo()
            {
                CommentMode = commentMode,
                CommentSort = commentSort,
                Oid = oid,
                IsDialog = true
            });
            splitView.IsPaneOpen = true;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                dynamic_id = e.Parameter.ToString();
                await m_viewModel.LoadData(e.Parameter.ToString());
            }
        }

        private void pivotRight_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivotRight.SelectedIndex == 0 && splitView.IsPaneOpen && (repost.UserDynamicRepostViewModel.DynamicItems == null || repost.UserDynamicRepostViewModel.DynamicItems.Count == 0))
            {
                repost.LoadData(dynamic_id);
            }
        }
    }
}
