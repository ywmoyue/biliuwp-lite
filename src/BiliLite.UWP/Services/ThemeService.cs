﻿using BiliLite.Extensions;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common;
using BiliLite.Models.Theme;
using MicaForUWP.Media;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BiliLite.Services
{
    [RegisterSingletonUIServiceAttribute]
    public class ThemeService
    {
        private readonly ResourceDictionary m_defaultColorsResource = App.Current.Resources.MergedDictionaries.FirstOrDefault(x => x.Source.AbsoluteUri.Contains("Default"));
        private readonly ResourceDictionary m_accentColorsResource = GetAccentColorsResourceDictionary();
        private readonly Frame rootFrame = Window.Current.Content as Frame;
        private readonly SettingSqlService m_settingSqlService;
        private ElementTheme m_theme;

        private static ResourceDictionary GetAccentColorsResourceDictionary()
        {
            var appResources = Application.Current.Resources;
            var accentDictionary = appResources.MergedDictionaries
                .FirstOrDefault(x => x.Source.AbsoluteUri.Contains("Accent.xaml"));

            // 检查是否已经加载了 muxc:XamlControlsResources
            if (accentDictionary.MergedDictionaries.OfType<XamlControlsResources>().FirstOrDefault() is XamlControlsResources xamlControlsResources)
            {
                return xamlControlsResources.MergedDictionaries.FirstOrDefault();
            }
            return accentDictionary;
        }

        public ResourceDictionary DefaultThemeResource => m_theme == ElementTheme.Light
            ? m_defaultColorsResource.ThemeDictionaries["Light"] as ResourceDictionary
            : m_defaultColorsResource.ThemeDictionaries["Dark"] as ResourceDictionary;

        public ResourceDictionary AccentThemeResource => m_accentColorsResource;

        public ThemeService(SettingSqlService settingSqlService)
        {
            m_settingSqlService = settingSqlService;
            if (rootFrame != null)
                rootFrame.RequestedTheme =
                    m_theme = (ElementTheme)SettingService.GetValue<int>(SettingConstants.UI.THEME, 0);
            if (m_theme == ElementTheme.Default)
            {
                m_theme = (ElementTheme)(App.Current.RequestedTheme + 1);
            }
        }

        public void InitTitleBar()
        {
            AppExtensions.HandleTitleTheme();
        }

        public void InitAccentColor()
        {
            var themeColorIndex = SettingService.GetValue<int>(SettingConstants.UI.THEME_COLOR, SettingConstants.UI.DEFAULT_THEME_COLOR);
            if (themeColorIndex < 0)
            {
                // 系统色彩
                SetColor(null, false);
            }
            else
            {
                // 根据索引选择自带色彩
                var colors = GetColorMenu();
                if (themeColorIndex >= colors.Count) themeColorIndex = SettingConstants.UI.DEFAULT_THEME_COLOR;
                var selectedItem = GetColorMenu()[themeColorIndex];
                SetColor(selectedItem.Color, false);
            }
        }

        public void InitMicaBrushBackgroundSource()
        {
            var backgroundSource = (BackgroundSource)SettingService.GetValue<int>(SettingConstants.UI.MICA_BACKGROUND_SOURCE, SettingConstants.UI.DEFAULT_MICA_BACKGROUND_SOURCE);
            var enableBackgroundSource = SettingService.GetValue(SettingConstants.UI.ENABLE_MICA_BACKGROUND_SOURCE, SettingConstants.UI.DEFAULT_ENABLE_MICA_BACKGROUND_SOURCE);
            SetMicaBrushBackgroundSource(backgroundSource, !enableBackgroundSource);
        }

        public void InitStyle()
        {

        }

        public void SetTheme(ElementTheme theme)
        {
            m_theme = theme;
            SettingService.SetValue(SettingConstants.UI.THEME, (int)theme);
            rootFrame.RequestedTheme = theme switch
            {
                ElementTheme.Light => ElementTheme.Light,
                ElementTheme.Dark => ElementTheme.Dark,
                _ => ElementTheme.Default,
            };
            InitTitleBar();
        }

        public void SetColor(Color? color = null, bool isNeedRefreshTheme = true)
        {
            if (color.HasValue)
            {
                m_accentColorsResource["SystemAccentColor"] = color;
            }
            else
            {
                m_accentColorsResource["SystemAccentColor"] = new UISettings().GetColorValue(UIColorType.Accent);
                m_accentColorsResource.Remove("SystemAccentColor");
            }

            if (isNeedRefreshTheme)
                RefreshTheme();
        }

        public void SetMicaBrushBackgroundSource(BackgroundSource backgroundSource, bool alwaysUseFallback)
        {
            // 获取当前主题的资源字典
            var currentThemeResource = m_theme == ElementTheme.Light
                ? m_defaultColorsResource.ThemeDictionaries["Light"] as ResourceDictionary
                : m_defaultColorsResource.ThemeDictionaries["Dark"] as ResourceDictionary;

            // 查找PageBackgroundMicaBrush
            if (currentThemeResource.TryGetValue("PageBackgroundMicaBrush", out var brush) &&
                brush is BackdropMicaBrush micaBrush)
            {
                micaBrush.BackgroundSource = backgroundSource;
                micaBrush.AlwaysUseFallback = alwaysUseFallback;

                // 强制刷新UI以应用更改
                RefreshTheme();
            }
        }

        /// <summary>
        /// 强制刷新主题
        /// </summary>
        /// <returns></returns>
        public void RefreshTheme()
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Wait, 1);
            rootFrame.RequestedTheme = ElementTheme.Light;
            rootFrame.RequestedTheme = ElementTheme.Dark;
            rootFrame.RequestedTheme = (ElementTheme)SettingService.GetValue<int>(SettingConstants.UI.THEME, 0);
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
        }

        public List<ColorItemModel> GetDefaultThemeColorMenu()
        {
            return
            [
                new(true, "少女粉", "#D14E65", Microsoft.Toolkit.Uwp.Helpers.ColorHelper.ToColor("#D14E65")),
                new(false, "胖次蓝", "#0092D0", Microsoft.Toolkit.Uwp.Helpers.ColorHelper.ToColor("#0092D0")),
                new(false, "咸蛋黄", "#C5963C", Microsoft.Toolkit.Uwp.Helpers.ColorHelper.ToColor("#C5963C")),
                new(false, "早苗绿", "#5B8F30", Microsoft.Toolkit.Uwp.Helpers.ColorHelper.ToColor("#5B8F30")),
                new(false, "基佬紫", "#9664DB", Microsoft.Toolkit.Uwp.Helpers.ColorHelper.ToColor("#9664DB")),
                new(false, "绅士灰", "#6D8AA6", Microsoft.Toolkit.Uwp.Helpers.ColorHelper.ToColor("#6D8AA6")),
                new(false, "高能红", "#D63F41", Microsoft.Toolkit.Uwp.Helpers.ColorHelper.ToColor("#D63F41"))
            ];
        }

        public List<ColorItemModel> GetColorMenu()
        {
            return m_settingSqlService.GetValue(SettingConstants.UI.THEME_COLOR_MENU, GetDefaultThemeColorMenu());
        }
    }
}
