using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using BiliLite.Models.Common;
using BiliLite.Services;
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
        private bool m_isStaggered = false;

        public DynamicSpacePage()
        {
            m_viewModel = App.ServiceProvider.GetRequiredService<UserDynamicSpaceViewModel>();
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
    }
}
