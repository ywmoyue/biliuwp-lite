using BiliLite.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common.Settings;
using BiliLite.ViewModels.Settings;
using CommunityToolkit.WinUI;
using CommunityToolkit.WinUI.Controls;
using Microsoft.Extensions.DependencyInjection;
using BiliLite.Services;
using System.Diagnostics;
using Microsoft.Windows.AppLifecycle;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingPage : BasePage, IUpdatePivotLayout
    {
        private readonly SettingPageViewModel m_viewModel;
        private ThemeService m_themeService;
        private bool m_isSearching = false;

        public SettingPage()
        {
            m_viewModel = App.ServiceProvider.GetRequiredService<SettingPageViewModel>();
            m_themeService = App.ServiceProvider.GetRequiredService<ThemeService>();
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

        private async void txtHelp_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            if (e.Uri.ToString() == "OpenLog")
            {
                var path = Windows.Storage.ApplicationData.Current.LocalFolder.Path + @"\log\";
                await Windows.System.Launcher.LaunchFolderPathAsync(path);
            }
            else
            {
                await Windows.System.Launcher.LaunchUriAsync(e.Uri);
            }
        }

        private async void RestartButton_Click(object sender, RoutedEventArgs e) => AppInstance.Restart("");

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
            if (m_isSearching)
            {
                NotificationShowExtensions.ShowMessageToast("上一个搜索执行中");
                return;
            }

            m_isSearching = true;

            try
            {
                await Task.Delay(50); // 等待Text实际变更

                if (!SettingsSearchMap.Map.TryGetValue(sender.Text, out var settingFullName))
                    return;

                var settingParts = settingFullName.Split(':');
                if (settingParts.Length < 2) return;

                var settings = settingParts[0];
                var settingName = settingParts[1];
                var settingExpandName = settingParts.Length > 2 ? settingParts[2] : null;

                // 导航到对应的PivotItem
                if (this.FindChildByElementName(settings) is PivotItem pivotItem)
                    pivot.SelectedItem = pivotItem;

                await Task.Delay(200); // 等待渲染

                // 处理扩展面板
                if (settingExpandName != null &&
                    this.FindChildByElementName(settingExpandName) is SettingsExpander expandElement &&
                    !expandElement.IsExpanded)
                {
                    expandElement.IsExpanded = true;
                    await Task.Delay(200); // 等待渲染
                }

                // 滚动到目标元素
                var element = this.FindChildByElementName(settingName);
                if (element == null) return;

                var scrollViewer = element.FindAscendant<ScrollViewer>();
                if (scrollViewer == null) return;

                scrollViewer.ChangeView(null, GetVerticalOffsetBetweenElements(element, SearchBox), null);

                // 高亮目标元素
                Control card = element switch
                {
                    SettingsCard sc => sc,
                    SettingsExpander se => se,
                    _ => element.FindAscendant<SettingsCard>()
                };

                if (card != null)
                    await HighlightElementAsync(card);
            }
            catch (Exception ex)
            {
                NotificationShowExtensions.ShowMessageToast("搜索失败");
            }
            finally
            {
                m_isSearching = false;
            }
        }

        private async Task HighlightElementAsync(Control element)
        {
            if (element == null) return;

            // TODO: 针对SettingsExpander的样式修改无法生效
            var accentColor = (Color)m_themeService.AccentThemeResource["SystemAccentColor"];
            var accentBrush = new SolidColorBrush(accentColor);

            var backupBackground = element.Background;

            for (int i = 0; i < 2; i++)
            {
                element.Background = accentBrush;
                await Task.Delay(500);
                element.Background = backupBackground;
                if (i < 1) await Task.Delay(500);
            }
        }

        private double GetVerticalOffsetBetweenElements(FrameworkElement element1, FrameworkElement element2)
        {
            try
            {
                // 获取第一个元素相对于窗口的变换
                var transform1 = element1.TransformToVisual(this.XamlRoot.Content);
                var position1 = transform1.TransformPoint(new Point(0, 0));

                // 获取第二个元素相对于窗口的变换
                var transform2 = element2.TransformToVisual(this.XamlRoot.Content);
                var position2 = transform2.TransformPoint(new Point(0, 0));

                // 计算垂直偏移量
                return position1.Y - position2.Y;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"获取偏移量出错: {ex.Message}");
                return 0;
            }
        }
    }
}
