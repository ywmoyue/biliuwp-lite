using BiliLite.Extensions;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Settings;
using MicaForUWP.Media;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace BiliLite.Services
{
	[RegisterSingletonUIServiceAttribute]
	public class ThemeService
	{
		private ResourceDictionary m_defaultColorsResource = App.Current.Resources.MergedDictionaries.FirstOrDefault(x => x.Source.AbsoluteUri.Contains("Colors.xaml"));
		private XamlControlsResources m_accentColorsResource = GetXamlControlsResources();
		private readonly Frame rootFrame = Window.Current.Content as Frame;
		private readonly SettingSqlService m_settingSqlService;
		private readonly object m_lightPageBackgroundBrush;
		private readonly object m_darkPageBackgroundBrush;
		private ElementTheme m_theme;

		public ResourceDictionary DefaultThemeResource => m_theme == ElementTheme.Light
			? m_defaultColorsResource.ThemeDictionaries["Light"] as ResourceDictionary
			: m_defaultColorsResource.ThemeDictionaries["Dark"] as ResourceDictionary;

		public ResourceDictionary AccentThemeResource => m_accentColorsResource?.MergedDictionaries?.FirstOrDefault();

		private static XamlControlsResources GetXamlControlsResources()
		{
			var winVersion = ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 14) ? "Win11" : "Win10";
			var appResourceDictionary = App.Current.Resources.MergedDictionaries.FirstOrDefault(x => x.Source.AbsoluteUri.Contains($"{winVersion}.xaml"));
			var xamlControlsResources = appResourceDictionary.MergedDictionaries.OfType<XamlControlsResources>().FirstOrDefault();
			return xamlControlsResources;
		}

		public ThemeService(SettingSqlService settingSqlService)
		{
			m_settingSqlService = settingSqlService;
			// 保存默认的背景笔刷，用于恢复默认背景色
			m_lightPageBackgroundBrush = GetPageBackgroundBrush("Light");
			m_darkPageBackgroundBrush = GetPageBackgroundBrush("Dark");
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

		/// <summary>
		/// 应用自定义界面字体
		/// 必须在页面加载前调用，已加载的控件不会重新解析字体资源，修改后需重启生效
		/// </summary>
		public void InitFontFamily()
		{
			var fontFamilyName = SettingService.GetValue<string>(SettingConstants.UI.FONT_FAMILY, "");
			if (rootFrame == null || string.IsNullOrWhiteSpace(fontFamilyName)) return;

			try
			{
				var fontFamily = new FontFamily(fontFamilyName.Trim());
				rootFrame.FontFamily = fontFamily;
				// WinUI 控件样式里写死了 {ThemeResource ContentControlThemeFontFamily}，
				// 只设置 Frame 字体覆盖不到按钮、列表项、导航等控件，这里在应用资源里覆盖同名资源
				App.Current.Resources["ContentControlThemeFontFamily"] = fontFamily;
			}
			catch (Exception)
			{
				// 字体无效时保持默认字体
			}
		}

		/// <summary>
		/// 应用自定义页面背景颜色，未设置时保持默认（跟随系统材质）
		/// </summary>
		public void InitPageBackgroundColor()
		{
			var colorText = SettingService.GetValue<string>(SettingConstants.UI.PAGE_BACKGROUND_COLOR, "");
			if (string.IsNullOrWhiteSpace(colorText)) return;

			try
			{
				SetPageBackgroundColor(Microsoft.Toolkit.Uwp.Helpers.ColorHelper.ToColor(colorText), false);
			}
			catch (Exception)
			{
				// 颜色值异常时保持默认背景
			}
		}

		/// <summary>
		/// 设置页面背景颜色，颜色为空时恢复默认的背景材质
		/// </summary>
		public void SetPageBackgroundColor(Color? color, bool isNeedRefreshTheme = true)
		{
			// 页面统一使用 PageBackgroundMicaBrush，替换主题字典里的笔刷即可覆盖所有页面
			var brush = color.HasValue ? (object)new SolidColorBrush(color.Value) : null;
			SetThemeResource("Light", "PageBackgroundMicaBrush", brush ?? m_lightPageBackgroundBrush);
			SetThemeResource("Dark", "PageBackgroundMicaBrush", brush ?? m_darkPageBackgroundBrush);

			if (isNeedRefreshTheme)
				RefreshTheme();
		}

		private object GetPageBackgroundBrush(string themeKey)
		{
			if (m_defaultColorsResource?.ThemeDictionaries == null) return null;
			if (!m_defaultColorsResource.ThemeDictionaries.ContainsKey(themeKey)) return null;
			if (!(m_defaultColorsResource.ThemeDictionaries[themeKey] is ResourceDictionary themeDictionary)) return null;
			return themeDictionary.ContainsKey("PageBackgroundMicaBrush")
				? themeDictionary["PageBackgroundMicaBrush"]
				: null;
		}

		private void SetThemeResource(string themeKey, string key, object value)
		{
			if (value == null) return;
			if (m_defaultColorsResource?.ThemeDictionaries == null) return;
			if (!m_defaultColorsResource.ThemeDictionaries.ContainsKey(themeKey)) return;
			if (!(m_defaultColorsResource.ThemeDictionaries[themeKey] is ResourceDictionary themeDictionary)) return;
			themeDictionary[key] = value;
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
				AccentThemeResource["SystemAccentColor"] = color;
			}
			else
			{
				AccentThemeResource["SystemAccentColor"] = new UISettings().GetColorValue(UIColorType.Accent);
				AccentThemeResource.Remove("SystemAccentColor");
			}

			if (isNeedRefreshTheme)
				RefreshTheme();
		}

		public void SetMicaBrushBackgroundSource(BackgroundSource backgroundSource, bool alwaysUseFallback)
		{

			// 查找PageBackgroundMicaBrush
			if (DefaultThemeResource.TryGetValue("PageBackgroundMicaBrush", out var brush) &&
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

		public List<ColorItemModel> GetColorMenu() => m_settingSqlService.GetValue(SettingConstants.UI.THEME_COLOR_MENU, GetDefaultThemeColorMenu());
	}
}
