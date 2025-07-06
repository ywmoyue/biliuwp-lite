using BiliLite.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using BiliLite.Extensions;
using BiliLite.Models.Common.Settings;
using BiliLite.ViewModels.Settings;
using Microsoft.Extensions.DependencyInjection;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingPage : BasePage, IUpdatePivotLayout
    {
        private readonly SettingPageViewModel m_viewModel;

        public SettingPage()
        {
            m_viewModel = App.ServiceProvider.GetRequiredService<SettingPageViewModel>();
            this.InitializeComponent();
            Title = "设置";
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            try
            {
                txtHelp.Text = await FileIO.ReadTextAsync(await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/Text/help.md")));
            }
            catch (Exception)
            {

                throw;
            }
        }

        private async void txtHelp_LinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            if (e.Link == "OpenLog")
            {
                var path = Windows.Storage.ApplicationData.Current.LocalFolder.Path + @"\log\";
                await Windows.System.Launcher.LaunchFolderPathAsync(path);
            }
            else
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri(e.Link));
            }
        }

        private async void RestartButton_Click(object sender, RoutedEventArgs e) => await CoreApplication.RequestRestartAsync(string.Empty);

        public void UpdatePivotLayout()
        {
            pivot.UseLayoutRounding = !pivot.UseLayoutRounding;
            pivot.UseLayoutRounding = !pivot.UseLayoutRounding;
        }

        private void SearchBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput) return;
            m_viewModel.SuggestSearchContents.Clear();
            var keyword = sender.Text;
            var searchTextList = SettingsSearchMap.Map.Keys.ToList();

            var searchResults = new List<string>();
            
            foreach (var text in searchTextList.Where(text => text.Contains(keyword)))
            {
                searchResults.Add(text);
                if (searchResults.Count > 9) break;
            }

            m_viewModel.SuggestSearchContents.AddRange(searchResults);
        }

        private void SearchBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            SearchBox.Width = 300;
        }

        private void SearchBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            SearchBox.Width = 150;
        }

        private async void SearchBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            // 等待Text实际变更
            await Task.Delay(50);
            if (!SettingsSearchMap.Map.TryGetValue(sender.Text, out var settingFullName))
            {
                return;
            }

            var settings = settingFullName.Split(":")[0];
            var settingName = settingFullName.Split(":")[1];

            var pivotItem = this.FindChildByElementName(settings) as PivotItem;

            if (pivotItem != null)
            {
                pivot.SelectedItem = pivotItem;
            }

            var element = this.FindChildByElementName(settingName);
            if (element == null) return;

            // TODO： 滚动到设置项位置
            //var scrollViewer = element.FindAscendant<ScrollViewer>();
            //var pivot = scrollViewer.FindAscendant<Pivot>();

            //var offset = GetVerticalOffsetFromParent(element, pivot);
            //scrollViewer.ChangeView(null, offset, null);
        }

        public static double GetVerticalOffsetFromParent(FrameworkElement element, FrameworkElement parent)
        {
            double offset = 0;
            FrameworkElement current = element;

            while (current != null && current != parent)
            {
                // 获取当前元素相对于其父元素的垂直位置
                var transform = current.TransformToVisual(current.Parent as UIElement);
                var point = transform.TransformPoint(new Point(0, 0));
                offset += point.Y;

                // 移动到父元素
                current = current.Parent as FrameworkElement;

                // 如果没有父元素或到达ScrollViewer则退出循环
                if (current == null || current == parent)
                    break;
            }

            return offset;
        }
    }
}
