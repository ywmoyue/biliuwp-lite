using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using BiliLite.ViewModels.User;
using Microsoft.Extensions.DependencyInjection;
using BiliLite.Models.Common;
using BiliLite.Services;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages.User
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class CollectedPage : Page, IRefreshablePage
    {
        private readonly CollectedPageViewModel m_viewModel;

        public CollectedPage()
        {
            m_viewModel = App.ServiceProvider.GetRequiredService<CollectedPageViewModel>();
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await m_viewModel.LoadCollected();
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

        private async void BtnRefresh_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            await Refresh();
        }

        public async Task Refresh()
        {
            await m_viewModel.LoadCollected();
        }

        private async void BtnCollectedDel_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is MenuFlyoutItem menuItem)) return;
            var data = menuItem.DataContext as FavoriteItemViewModel;
            await m_viewModel.CancelCollected(data);
        }
    }
}
