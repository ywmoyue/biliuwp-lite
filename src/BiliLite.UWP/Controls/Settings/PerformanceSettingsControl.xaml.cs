using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Services;
using Microsoft.Toolkit.Uwp.UI;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls.Settings
{
    public sealed partial class PerformanceSettingsControl : UserControl
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        public PerformanceSettingsControl()
        {
            try
            {
                InitializeComponent();
                LoadPerformance();
            }
            catch (Exception ex)
            {
                _logger.Warn("性能设置加载失败", ex);
            }
        }

        private void LoadPerformance()
        {
            //加载原图
            swPictureQuality.IsOn = SettingService.GetValue<bool>(SettingConstants.UI.ORTGINAL_IMAGE, false);
            swPictureQuality.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swPictureQuality.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.ORTGINAL_IMAGE, swPictureQuality.IsOn);
                    SettingService.UI.LoadOriginalImage = null;
                });
            });
            //缓存页面
            swHomeCache.IsOn = SettingService.GetValue<bool>(SettingConstants.UI.CACHE_HOME, true);
            swHomeCache.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swHomeCache.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.CACHE_HOME, swHomeCache.IsOn);

                });
            });

            //新窗口浏览图片
            swPreviewImageNavigateToPage.IsOn = SettingService.GetValue<bool>(SettingConstants.UI.NEW_WINDOW_PREVIEW_IMAGE, false);
            swPreviewImageNavigateToPage.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swPreviewImageNavigateToPage.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.NEW_WINDOW_PREVIEW_IMAGE, swPreviewImageNavigateToPage.IsOn);
                });
            });

            swPreviewImageNavigateToPageFully.IsOn = SettingService.GetValue<bool>(SettingConstants.UI.NEW_FULLY_WINDOW_PREVIEW_IMAGE, true);
            swPreviewImageNavigateToPageFully.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swPreviewImageNavigateToPageFully.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.NEW_FULLY_WINDOW_PREVIEW_IMAGE, swPreviewImageNavigateToPageFully.IsOn);
                });
            });

            //启动应用时打开上次浏览的标签页
            SwitchOpenLastPage.IsOn = SettingService.GetValue<bool>(SettingConstants.UI.ENABLE_OPEN_LAST_PAGE, SettingConstants.UI.DEFAULT_ENABLE_OPEN_LAST_PAGE);
            SwitchOpenLastPage.Loaded += (sender, e) =>
            {
                SwitchOpenLastPage.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.ENABLE_OPEN_LAST_PAGE, SwitchOpenLastPage.IsOn);
                };
            };

            //启动应用时打开上次浏览的标签页数量限制
            NumberOpenLastPageCount.Value = SettingService.GetValue(SettingConstants.UI.OPEN_LAST_PAGE_LIMIT_COUNT, SettingConstants.UI.DEFAULT_OPEN_LAST_PAGE_LIMIT_COUNT);
            NumberOpenLastPageCount.Loaded += (sender, e) =>
            {
                NumberOpenLastPageCount.ValueChanged += (obj, args) =>
                {
                    if (args.NewValue % 1 != 0)
                    {
                        NumberOpenLastPageCount.Value = (int)args.NewValue;
                    }
                    SettingService.SetValue(SettingConstants.UI.OPEN_LAST_PAGE_LIMIT_COUNT, (int)NumberOpenLastPageCount.Value);
                };
            };

            //滚动视图加载更多触发偏移量
            NumberScrollViewLoadMoreBottomOffset.Value = SettingService.GetValue(SettingConstants.UI.SCROLL_VIEW_LOAD_MORE_BOTTOM_OFFSET, SettingConstants.UI.DEFAULT_SCROLL_VIEW_LOAD_MORE_BOTTOM_OFFSET);
            NumberScrollViewLoadMoreBottomOffset.Loaded += (_, _) =>
            {
                NumberScrollViewLoadMoreBottomOffset.ValueChanged += (_, _) =>
                {
                    SettingService.SetValue(SettingConstants.UI.SCROLL_VIEW_LOAD_MORE_BOTTOM_OFFSET, (double)NumberScrollViewLoadMoreBottomOffset.Value);
                };
            };

            //浏览器打开无法处理的链接
            swOpenUrlWithBrowser.IsOn = SettingService.GetValue<bool>(SettingConstants.UI.OPEN_URL_BROWSER, false);
            swOpenUrlWithBrowser.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swOpenUrlWithBrowser.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.UI.OPEN_URL_BROWSER, swOpenUrlWithBrowser.IsOn);
                });
            });

        }

        private async void btnCleanImageCache_Click(object sender, RoutedEventArgs e)
        {
            await ImageCache.Instance.ClearAsync();
            NotificationShowExtensions.ShowMessageToast("已清除图片缓存");
        }
    }
}
