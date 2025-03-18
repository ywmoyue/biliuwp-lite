using System;
using Windows.UI.Xaml.Controls;
using BiliLite.ViewModels.UserDynamic;
using Microsoft.Extensions.DependencyInjection;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls
{
    public sealed partial class DynamicRepostControl : UserControl
    {
        public UserDynamicRepostViewModel UserDynamicRepostViewModel { get; }

        public DynamicRepostControl()
        {
            UserDynamicRepostViewModel = App.ServiceProvider.GetRequiredService<UserDynamicRepostViewModel>();
            InitializeComponent();
        }

        public async void LoadData(string id)
        {
            UserDynamicRepostViewModel.ID = id;
            await UserDynamicRepostViewModel.GetDynamicItemReposts();
        }
    }
}
