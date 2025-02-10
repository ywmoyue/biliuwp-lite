using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BiliLite.Models.Common;
using BiliLite.Services;
using Windows.ApplicationModel.Core;

namespace BiliLite.Extensions
{
    public static class AppExtensions
    {
        private static readonly Dictionary<ElementTheme, Action<ApplicationViewTitleBar>> _handleTitleThemeActions
            = new Dictionary<ElementTheme, Action<ApplicationViewTitleBar>>()
            {
                {
                    ElementTheme.Default, (titleBar) =>
                    {
                        var rootTheme = App.Current.RequestedTheme;
                        if (rootTheme == ApplicationTheme.Light)
                        {
                            HandleTitleLightTheme(titleBar);
                        }
                        else
                        {
                            HandleTitleDarkTheme(titleBar);
                        }
                    }
                },
                { ElementTheme.Light, HandleTitleLightTheme },
                { ElementTheme.Dark, HandleTitleDarkTheme },
            };

        private static void HandleTitleLightTheme(ApplicationViewTitleBar titleBar)
        {
            titleBar.ButtonForegroundColor = Colors.Black;
            titleBar.ButtonHoverForegroundColor = Colors.White;
            titleBar.ButtonPressedForegroundColor = Colors.Black;
        }

        private static void HandleTitleDarkTheme(ApplicationViewTitleBar titleBar)
        {
            titleBar.ButtonForegroundColor = Colors.White;
            titleBar.ButtonHoverForegroundColor = Colors.Black;
            titleBar.ButtonPressedForegroundColor = Colors.White;
        }

        public static void HandleTitleTheme()
        {
            CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
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
        }
    }
}
