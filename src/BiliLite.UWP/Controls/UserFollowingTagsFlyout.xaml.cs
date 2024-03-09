using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
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
            this.InitializeComponent();
        }

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
