using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Comment;
using BiliLite.Services;
using BiliLite.ViewModels.UserDynamic;
using Microsoft.Extensions.DependencyInjection;
using BiliLite.Models.Requests.Api;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BiliLite.Pages.User
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DynamicSpacePage : Page
    {
        private readonly UserDynamicSpaceViewModel m_viewModel;
        private bool m_isStaggered = false;

        public DynamicSpacePage()
        {
            m_viewModel = App.ServiceProvider.GetRequiredService<UserDynamicSpaceViewModel>();
            m_viewModel.OpenCommentEvent += UserDynamicViewModelOpenCommentEvent;
            this.InitializeComponent();
        }

        private void SetStaggered()
        {
            var staggered = SettingService.GetValue<int>(SettingConstants.UI.DYNAMIC_DISPLAY_MODE, 0) == 1;
            if (staggered != m_isStaggered)
            {
                m_isStaggered = staggered;
                if (staggered)
                {
                    SetGridCore();
                }
                else
                {
                    SetListCore();
                }
            }
        }

        private void SetGridCore()
        {
            m_isStaggered = true;
            BtnGrid.Visibility = Visibility.Collapsed;
            BtnList.Visibility = Visibility.Visible;
            //XAML
            ListDyn.ItemsPanel = (ItemsPanelTemplate)this.Resources["GridPanel"];
        }

        private void SetListCore()
        {
            m_isStaggered = false;
            //右下角按钮
            BtnGrid.Visibility = Visibility.Visible;
            BtnList.Visibility = Visibility.Collapsed;
            //XAML
            ListDyn.ItemsPanel = (ItemsPanelTemplate)this.Resources["ListPanel"];
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            SetStaggered();
            m_viewModel.UserId = e.Parameter as string;
            await m_viewModel.GetDynamicItems();
        }

        private async void BtnRefreshDynamic_OnClick(object sender, RoutedEventArgs e)
        {
            await m_viewModel.GetDynamicItems();
        }

        private void BtnTop_OnClick(object sender, RoutedEventArgs e)
        {
            ListDyn.ScrollIntoView(ListDyn.Items.FirstOrDefault());
        }

        private void BtnList_OnClick(object sender, RoutedEventArgs e)
        {
            SettingService.SetValue<int>(SettingConstants.UI.DYNAMIC_DISPLAY_MODE, 0);
            SetListCore();
        }

        private void BtnGrid_OnClick(object sender, RoutedEventArgs e)
        {
            SettingService.SetValue<int>(SettingConstants.UI.DYNAMIC_DISPLAY_MODE, 1);
            SetGridCore();
        }

        private void CloseCommentCore()
        {
            var storyboard = (Storyboard)this.Resources["HideComment"];
            storyboard.Begin();
        }


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
            var storyboard = (Storyboard)this.Resources["ShowComment"];
            storyboard.Begin();
        }

        private void CommentPanel_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            CloseCommentCore();
        }
    }
}
