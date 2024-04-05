using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using BiliLite.ViewModels.UserDynamic;
using Microsoft.Extensions.DependencyInjection;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BiliLite.Pages.User
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DynamicSpacePage : Page
    {
        private readonly UserDynamicSpaceViewModel m_viewModel;

        public DynamicSpacePage()
        {
            try
            {
                m_viewModel = App.ServiceProvider.GetRequiredService<UserDynamicSpaceViewModel>();
                this.InitializeComponent();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            m_viewModel.UserId = e.Parameter as string;
            await m_viewModel.GetDynamicItems();
        }
    }
}
