using BiliLite.Models.Common;
using BiliLite.Services;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BiliLite.Extensions
{
    public static class AppExtensions
    {
        private static readonly UISettings uISettings = new();

        private static readonly Dictionary<ElementTheme, Action<ApplicationViewTitleBar>> _handleTitleThemeActions
            = new()
            {
                {
                    ElementTheme.Default, (titleBar) =>
                    {
                        titleBar.ButtonForegroundColor = uISettings.GetColorValue(UIColorType.Foreground);
                        uISettings.ColorValuesChanged += (setting, args) =>
                        {
                            var settingTheme = SettingService.GetValue<int>(SettingConstants.UI.THEME, 0);
                            if (settingTheme == 0)
                                titleBar.ButtonForegroundColor = uISettings.GetColorValue(UIColorType.Foreground);
                        };
                    }
                },
                { ElementTheme.Light, HandleTitleLightTheme },
                { ElementTheme.Dark, HandleTitleDarkTheme },
            };

        private static void HandleTitleLightTheme(ApplicationViewTitleBar titleBar) => titleBar.ButtonForegroundColor = Colors.Black;
        private static void HandleTitleDarkTheme(ApplicationViewTitleBar titleBar) => titleBar.ButtonForegroundColor = Colors.White;

        public static void HandleTitleTheme()
        {
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;

            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ButtonBackgroundColor = Colors.Transparent;
            titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            var rootFrame = Window.Current.Content as Frame;
            var theme = rootFrame.RequestedTheme = (ElementTheme)SettingService.GetValue<int>(SettingConstants.UI.THEME, 0);
            if (!_handleTitleThemeActions.TryGetValue(theme, out var action))
            {
                return;
            }
            action(titleBar);
            ApplicationView.GetForCurrentView().VisibleBoundsChanged += AppExtensions_VisibleBoundsChanged;
        }

        private static void AppExtensions_VisibleBoundsChanged(ApplicationView sender, object args)
        {
            var titleBar = ApplicationView.GetForCurrentView().TitleBar;
            if (sender.IsFullScreenMode)
            {
                titleBar.ButtonBackgroundColor = uISettings.GetColorValue(UIColorType.Accent);
            }
            else
            {
                titleBar.ButtonBackgroundColor = Colors.Transparent;
            }
        }
    }
}
