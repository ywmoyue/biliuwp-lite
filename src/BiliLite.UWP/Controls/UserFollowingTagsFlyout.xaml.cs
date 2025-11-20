using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using BiliLite.ViewModels.User;
using Microsoft.Extensions.DependencyInjection;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls
{
    public sealed partial class UserFollowingTagsFlyout : UserControl
    {
        private readonly UserFollowingTagsFlyoutViewModel m_viewModel;

        public UserFollowingTagsFlyout()
        {
            m_viewModel = App.ServiceProvider.GetRequiredService<UserFollowingTagsFlyoutViewModel>();
            InitializeComponent();
        }

        public bool HasInit { get; set; }

        private void FollowingTagFlyout_OnClosed(object sender, object e)
        {
            m_viewModel.CancelSaveFollowingTagUser();
        }

        private async void SaveFollowingTagUser_OnClick(object sender, RoutedEventArgs e)
        {
            await m_viewModel.SaveFollowingTagUser();
            FollowingTagFlyout.Hide();
        }

        public async Task Init(string userId)
        {
            await m_viewModel.Init(userId);
            HasInit = true;
        }

        public void ShowAt(DependencyObject target)
        {
            ContextFlyout.ShowAt(target, new FlyoutShowOptions()
            {
                Placement = FlyoutPlacementMode.Bottom
            });
        }
    }
}
