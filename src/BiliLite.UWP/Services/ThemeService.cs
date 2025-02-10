using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.ViewModels.Settings;
using Microsoft.UI.Xaml.Controls;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BiliLite.Services
{
    public class ThemeService
    {
        private readonly ResourceDictionary m_defaultColorsResource = Application.Current.Resources.MergedDictionaries.FirstOrDefault(x => x.Source.AbsoluteUri.Contains("Default"));
        private ElementTheme m_theme;

        public ThemeService()
        {
            m_theme = (ElementTheme)SettingService.GetValue<int>(SettingConstants.UI.THEME, 0);
            if (m_theme == ElementTheme.Default)
            {
                m_theme = (ElementTheme)(App.Current.RequestedTheme + 1);
            }
        }

        public ResourceDictionary ThemeResource
        {
            get
            {
                if (m_theme == ElementTheme.Light) return m_defaultColorsResource.ThemeDictionaries["Light"] as ResourceDictionary;
                return m_defaultColorsResource.ThemeDictionaries["Dark"] as ResourceDictionary;
            }
        }

        public void InitTitleBar()
        {
            AppExtensions.HandleTitleTheme();
        }

        public void InitAccentColor()
        {
            var a = SettingService.GetValue(SettingConstants.UI.THEME_COLOR, SettingConstants.UI.DEFAULT_THEME_COLOR);
            var b = new UISettingsControlViewModel();
            var c = b.Colors[a];
            SetColor(c.Color);
        }

        public void SetTheme(ElementTheme theme)
        {
            m_theme = theme;
            SettingService.SetValue(SettingConstants.UI.THEME, (int)theme);
            var rootFrame = Window.Current.Content as Frame;
            switch (theme)
            {
                case ElementTheme.Light:
                    rootFrame.RequestedTheme = ElementTheme.Light;
                    break;
                case ElementTheme.Dark:
                    rootFrame.RequestedTheme = ElementTheme.Dark;
                    break;
                default:
                    rootFrame.RequestedTheme = ElementTheme.Default;
                    break;
            }
            InitTitleBar();
        }

        public void SetColor(Color color)
        {
            var appResources = Application.Current.Resources;
            var accentDictionary = appResources.MergedDictionaries
                .FirstOrDefault(x => x.Source.AbsoluteUri.Contains("Accent.xaml"));

            // 检查是否已经加载了 muxc:XamlControlsResources
            if (accentDictionary.MergedDictionaries.OfType<XamlControlsResources>().FirstOrDefault() is XamlControlsResources xamlControlsResources)
            {
                var resourceDictionary = xamlControlsResources.MergedDictionaries.FirstOrDefault();
                resourceDictionary["SystemAccentColor"] = color;
            }

            // 强制刷新主题
            var rootFrame = Window.Current.Content as Frame;
            var requestedTheme = rootFrame.RequestedTheme;
            rootFrame.RequestedTheme = ElementTheme.Light;
            rootFrame.RequestedTheme = ElementTheme.Dark;
            rootFrame.RequestedTheme = requestedTheme;
        }
    }
}
