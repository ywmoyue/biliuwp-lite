using BiliLite.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace BiliLite.Converters
{
    /// <summary>
    /// DataTemplate 不可用
    /// </summary>
    public class ColorSelectedConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var themeService = App.ServiceProvider.GetRequiredService<ThemeService>();
            if (value == null) return new SolidColorBrush((Color)themeService.DefaultThemeResource["TextColor"]);
            return value.ToString() == parameter.ToString()
                ? new SolidColorBrush((Color)themeService.AccentThemeResource["SystemAccentColor"])
                : new SolidColorBrush((Color)themeService.DefaultThemeResource["TextColor"]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Colors.Black;
        }
    }
}
