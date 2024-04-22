using System.Linq;
using System.Threading.Tasks;
using BiliLite.Services;
using BiliLite.Models.Common;
using BiliLite.Models.Requests.Api;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using BiliLite.Extensions;
using BiliLite.ViewModels.UserDynamic;
using Microsoft.Extensions.DependencyInjection;
using BiliLite.Models.Common.Comment;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages.Home
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class UserDynamicAllPage : Page,IRefreshablePage
    {
        readonly UserDynamicAllViewModel m_viewModel;
        private bool m_isStaggered = false;
        private UserDynamicShowType m_currentShowType;

        public UserDynamicAllPage()
        {
            m_viewModel = App.ServiceProvider.GetRequiredService<UserDynamicAllViewModel>();
            m_viewModel.OpenCommentEvent += UserDynamicViewModelOpenCommentEvent;
            this.InitializeComponent();
            m_currentShowType = (UserDynamicShowType)DynPivot.SelectedIndex;
        }

        protected  override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SetStaggered();
            if (e.NavigationMode == NavigationMode.New && m_viewModel.DynamicItems == null)
            {
                await m_viewModel.GetDynamicItems();
                if (SettingService.GetValue<bool>("动态切换提示", true) && SettingService.GetValue<int>(SettingConstants.UI.DYNAMIC_DISPLAY_MODE, 0) != 1)
                {
                    SettingService.SetValue("动态切换提示", false);
                    Notify.ShowMessageToast("右下角可以切换成瀑布流显示哦~", 5);
                }
            }
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

            //顶部
            GridTopBar.MaxWidth = double.MaxValue;
            GridTopBar.Margin = new Thickness(0, 0, 0, 4);
            BorderTopBar.CornerRadius = new CornerRadius(0);
            BorderTopBar.Margin = new Thickness(0);
        }

        private void SetListCore()
        {
            m_isStaggered = false;
            //右下角按钮
            BtnGrid.Visibility = Visibility.Visible;
            BtnList.Visibility = Visibility.Collapsed;
            //XAML
            ListDyn.ItemsPanel = (ItemsPanelTemplate)this.Resources["ListPanel"];

            //顶部
            GridTopBar.MaxWidth = 800;
            GridTopBar.Margin = new Thickness(8, 0, 8, 0);
            BorderTopBar.CornerRadius = new CornerRadius(4);
            BorderTopBar.Margin = new Thickness(12, 4, 12, 4);
        }

        private async void BtnRefreshDynamic_OnClick(object sender, RoutedEventArgs e)
        {
            await Refresh();
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

        private async void Pivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var showType = (UserDynamicShowType)DynPivot.SelectedIndex;
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
    }
}
