using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Comment;
using BiliLite.Models.Requests.Api;
using BiliLite.Services;
using BiliLite.Services.Interfaces;
using BiliLite.ViewModels.UserDynamic;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages.Home
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class UserDynamicPage : Page, IRefreshablePage, IUpdatePivotLayout
    {
        readonly UserDynamicAllViewModel m_viewModel;
        private bool m_isStaggered = false;
        private UserDynamicShowType m_currentShowType;

        public UserDynamicPage()
        {
            m_viewModel = App.ServiceProvider.GetRequiredService<UserDynamicAllViewModel>();
            m_viewModel.OpenCommentEvent += UserDynamicViewModelOpenCommentEvent;
            this.InitializeComponent();
            m_currentShowType = (UserDynamicShowType)pivot.SelectedIndex;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SetStaggered();
            if (e.NavigationMode == NavigationMode.New && m_viewModel.DynamicItems == null)
            {
                await m_viewModel.GetDynamicItems();
                if (SettingService.GetValue<bool>("动态切换提示", true) && SettingService.GetValue<int>(SettingConstants.UI.DYNAMIC_DISPLAY_MODE, 0) != 1)
                {
                    SettingService.SetValue("动态切换提示", false);
                    NotificationShowExtensions.ShowMessageToast("右下角可以切换成瀑布流显示哦~", 5);
                }
            }
        }

        private void SetStaggered()
        {
            var staggered = SettingService.GetValue<int>(SettingConstants.UI.DYNAMIC_DISPLAY_MODE, 0) == 0;
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

            //顶部
            GridTopBar.MaxWidth = double.MaxValue;
        }

        private void SetListCore()
        {
            m_isStaggered = false;
            //右下角按钮
            BtnGrid.Visibility = Visibility.Visible;
            BtnList.Visibility = Visibility.Collapsed;

            //顶部
            GridTopBar.MaxWidth = 800;
        }

        private async void BtnRefreshDynamic_OnClick(object sender, RoutedEventArgs e)
        {
            await Refresh();
        }

        private void BtnTop_OnClick(object sender, RoutedEventArgs e)
        {
            switch (pivot.SelectedIndex)
            {
                case 0:
                    ListDyn0.ScrollToTop();
                    break;
                case 1:
                    ListDyn1.ScrollToTop();
                    break;
                case 2:
                    ListDyn2.ScrollToTop();
                    break;
                case 3:
                    ListDyn3.ScrollToTop();
                    break;
            }
        }

        private void BtnList_OnClick(object sender, RoutedEventArgs e)
        {
            SettingService.SetValue<int>(SettingConstants.UI.DYNAMIC_DISPLAY_MODE, 1);
            SetListCore();
        }

        private void BtnGrid_OnClick(object sender, RoutedEventArgs e)
        {
            SettingService.SetValue<int>(SettingConstants.UI.DYNAMIC_DISPLAY_MODE, 0);
            SetGridCore();
        }

        private async void Pivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var showType = (UserDynamicShowType)pivot.SelectedIndex;
            if (showType == m_currentShowType) return;
            m_currentShowType = showType;
            await m_viewModel.GetDynamicItems(showType: showType);
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
            var storyboard = (Storyboard)this.Resources["ShowComment"];
            storyboard.Begin();
        }

        private void CommentPanel_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            CloseCommentCore();
        }

        public async Task Refresh()
        {
            await m_viewModel.GetDynamicItems(showType: m_currentShowType);
        }

        public void UpdatePivotLayout()
        {
            pivot.UseLayoutRounding = !pivot.UseLayoutRounding;
            pivot.UseLayoutRounding = !pivot.UseLayoutRounding;
        }
    }
}
