using BiliLite.Models.Common;
using BiliLite.Services;
using System;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using BiliLite.Models.Common.Other;
using BiliLite.ViewModels.Other;
using Microsoft.Extensions.DependencyInjection;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages.Other
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FindMorePage : BasePage
    {
        private readonly FindMoreViewModel m_viewModel;
        public FindMorePage()
        {
            m_viewModel = App.ServiceProvider.GetRequiredService<FindMoreViewModel>();
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New && m_viewModel.Items == null)
            {
                m_viewModel.LoadEntrance();
            }
        }

        private async void AdaptiveGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as FindMoreEntranceModel;
            if (item.Type == 0)
            {
                MessageCenter.NavigateToPage(this, new NavigationInfo() { 
                    icon =Symbol.Link,
                    title =item.Name,
                    page=typeof(WebPage),
                    parameters=item.Link
                });
            }
            else if(item.Type == 1)
            {
                await Launcher.LaunchUriAsync(new Uri(item.Link));
            }
           
        }
    }
}
