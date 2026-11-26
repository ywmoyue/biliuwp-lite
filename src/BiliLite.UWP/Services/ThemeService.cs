using BiliLite.Extensions;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Settings;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI;
using CommunityToolkit.WinUI.Helpers;
using System;

namespace BiliLite.Services
{
	[RegisterSingletonUIServiceAttribute]
	public class ThemeService
	{
		private ResourceDictionary m_defaultColorsResource = App.Current.Resources.MergedDictionaries.FirstOrDefault(x => x.Source.AbsoluteUri.Contains("Colors.xaml"));
		private XamlControlsResources m_accentColorsResource = GetXamlControlsResources();
		private readonly Frame rootFrame = MainWindow.Current.Content as Frame;
		private readonly SettingSqlService m_settingSqlService;
		private readonly object m_lightPageBackgroundBrush;
		private readonly object m_darkPageBackgroundBrush;
		private bool m_systemBackdropApplied;
		private Color? m_customPageBackgroundColor;
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
			// 保存默认的页面背景笔刷，关闭材质时用于恢复
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

		/// <summary>
		/// 按设置初始化窗口背景材质。
		/// 必须在页面加载前调用，这样页面解析 PageBackgroundMicaBrush 时取到的就是最终值，
		/// 不需要再强制刷新主题。
		/// </summary>
		public void InitSystemBackdrop()
		{
			var enable = SettingService.GetValue(SettingConstants.UI.ENABLE_MICA_BACKGROUND_SOURCE,
				SettingConstants.UI.DEFAULT_ENABLE_MICA_BACKGROUND_SOURCE);
			if (!enable)
			{
				SetSystemBackdrop(null, false);
				return;
			}

			var source = (SystemBackdropSource)SettingService.GetValue<int>(
				SettingConstants.UI.MICA_BACKGROUND_SOURCE,
				SettingConstants.UI.DEFAULT_MICA_BACKGROUND_SOURCE);
			SetSystemBackdrop(source, false);
		}

		/// <summary>
		/// 应用自定义界面字体。
		/// 必须在页面加载前调用，已加载的控件不会重新解析字体资源，修改后需重启生效。
		/// </summary>
		public void InitFontFamily()
		{
			var fontFamilyName = SettingService.GetValue<string>(SettingConstants.UI.FONT_FAMILY, "");
			if (string.IsNullOrWhiteSpace(fontFamilyName)) return;

			try
			{
				var fontFamily = new FontFamily(fontFamilyName.Trim());

				// WinUI 控件样式里写死了 {ThemeResource ContentControlThemeFontFamily}，
				// 只设置 Frame 字体覆盖不到按钮、列表项、导航等控件，这里在应用资源里覆盖同名资源
				App.Current.Resources["ContentControlThemeFontFamily"] = fontFamily;

				if (rootFrame != null)
					rootFrame.FontFamily = fontFamily;
			}
			catch (Exception)
			{
				// 字体无效时保持默认字体
			}
		}

		/// <summary>
		/// 应用窗口背景材质。
		/// 材质不可用（例如 Windows 10 上的云母）时不会把页面背景改成透明，
		/// 而是保留原来的纯色笔刷作为兜底，避免出现整窗透明的异常表现。
		/// </summary>
		/// <param name="source">要应用的材质，null 表示关闭材质</param>
		/// <param name="isNeedRefreshTheme">是否强制刷新主题，页面已加载后切换材质时需要</param>
		/// <returns>是否成功应用了材质</returns>
		public bool SetSystemBackdrop(SystemBackdropSource? source, bool isNeedRefreshTheme = true)
		{
			var backdrop = source.HasValue ? CreateSystemBackdrop(source.Value) : null;
			m_systemBackdropApplied = backdrop != null;

			if (MainWindow.Current != null)
				MainWindow.Current.SystemBackdrop = backdrop;

			ApplyPageBackgroundBrush();

			if (isNeedRefreshTheme)
				RefreshTheme();

			return m_systemBackdropApplied;
		}

		/// <summary>
		/// 按设置初始化自定义页面背景颜色，需在页面加载前调用。
		/// </summary>
		public void InitPageBackgroundColor()
		{
			var colorText = SettingService.GetValue<string>(SettingConstants.UI.PAGE_BACKGROUND_COLOR, "");
			if (string.IsNullOrWhiteSpace(colorText)) return;

			try
			{
				SetPageBackgroundColor(colorText.ToColor(), false);
			}
			catch (Exception)
			{
				// 颜色值异常时保持默认背景
			}
		}

		/// <summary>
		/// 设置自定义页面背景颜色，传入 null 时恢复默认背景。
		/// 自定义颜色优先于窗口背景材质，设置后页面不再透出材质。
		/// </summary>
		/// <param name="color">自定义颜色，null 表示恢复默认</param>
		/// <param name="isNeedRefreshTheme">是否强制刷新主题，页面已加载后修改时需要</param>
		public void SetPageBackgroundColor(Color? color, bool isNeedRefreshTheme = true)
		{
			m_customPageBackgroundColor = color;

			ApplyPageBackgroundBrush();

			if (isNeedRefreshTheme)
				RefreshTheme();
		}

		/// <summary>
		/// 按当前状态决定页面背景笔刷，是 PageBackgroundMicaBrush 的唯一写入者。
		/// 优先级：自定义背景色 &gt; 窗口材质透明 &gt; 主题字典里的原始纯色笔刷。
		/// </summary>
		private void ApplyPageBackgroundBrush()
		{
			if (m_customPageBackgroundColor.HasValue)
			{
				SetPageBackgroundBrush(new SolidColorBrush(m_customPageBackgroundColor.Value));
			}
			else if (m_systemBackdropApplied)
			{
				SetPageBackgroundBrush(new SolidColorBrush(Colors.Transparent));
			}
			else
			{
				// 两者都没有时恢复主题字典里的原始纯色笔刷
				SetPageBackgroundBrush(null);
			}
		}

		/// <summary>
		/// 当前窗口材质是否已生效
		/// </summary>
		public bool IsSystemBackdropApplied => m_systemBackdropApplied;

		private static SystemBackdrop CreateSystemBackdrop(SystemBackdropSource source)
		{
			try
			{
				if (source == SystemBackdropSource.Acrylic)
				{
					if (!DesktopAcrylicController.IsSupported()) return null;
					return new DesktopAcrylicBackdrop();
				}

				if (!MicaController.IsSupported()) return null;
				return new MicaBackdrop { Kind = MicaKind.Base };
			}
			catch (Exception)
			{
				// 材质初始化失败时回落为纯色背景
				return null;
			}
		}

		/// <summary>
		/// 切换页面背景笔刷，传入 null 时恢复系统默认笔刷
		/// </summary>
		private void SetPageBackgroundBrush(Brush brush)
		{
			var lightBrush = (object)brush ?? m_lightPageBackgroundBrush;
			var darkBrush = (object)brush ?? m_darkPageBackgroundBrush;

			SetThemeResource("Light", "PageBackgroundMicaBrush", lightBrush);
			SetThemeResource("Dark", "PageBackgroundMicaBrush", darkBrush);
			// 高对比度等场景按亮色兜底
			SetThemeResource("Default", "PageBackgroundMicaBrush", lightBrush);
			SetThemeResource("HighContrast", "PageBackgroundMicaBrush", lightBrush);
		}

		private object GetPageBackgroundBrush(string themeKey)
		{
			if (m_defaultColorsResource?.ThemeDictionaries == null) return null;
			if (!m_defaultColorsResource.ThemeDictionaries.TryGetValue(themeKey, out var value)) return null;
			if (!(value is ResourceDictionary themeDictionary)) return null;
			return themeDictionary.TryGetValue("PageBackgroundMicaBrush", out var brush) ? brush : null;
		}

		private void SetThemeResource(string themeKey, string key, object value)
		{
			if (value == null) return;
			if (m_defaultColorsResource?.ThemeDictionaries == null) return;
			if (!m_defaultColorsResource.ThemeDictionaries.TryGetValue(themeKey, out var existing)) return;
			if (!(existing is ResourceDictionary themeDictionary)) return;
			themeDictionary[key] = value;
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

		/// <summary>
		/// 强制刷新主题
		/// </summary>
		/// <returns></returns>
		public void RefreshTheme()
		{
			//Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Wait, 1);
			rootFrame.RequestedTheme = ElementTheme.Light;
			rootFrame.RequestedTheme = ElementTheme.Dark;
			rootFrame.RequestedTheme = (ElementTheme)SettingService.GetValue<int>(SettingConstants.UI.THEME, 0);
			//Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
		}

		public List<ColorItemModel> GetDefaultThemeColorMenu()
		{
			return
			[
				new(true, "少女粉", "#D14E65","#D14E65".ToColor()),
				new(false, "胖次蓝", "#0092D0", "#0092D0".ToColor()),
				new(false, "咸蛋黄", "#C5963C", "#C5963C".ToColor()),
				new(false, "早苗绿", "#5B8F30", "#5B8F30".ToColor()),
				new(false, "基佬紫", "#9664DB", "#9664DB".ToColor()),
				new(false, "绅士灰", "#6D8AA6", "#6D8AA6".ToColor()),
				new(false, "高能红", "#D63F41", "#D63F41".ToColor())
			];
		}

		public List<ColorItemModel> GetColorMenu() => m_settingSqlService.GetValue(SettingConstants.UI.THEME_COLOR_MENU, GetDefaultThemeColorMenu());
	}
}
