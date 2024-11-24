using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using BiliLite.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLite.Converters
{
    public class ColorSelectedConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var themeService = App.ServiceProvider.GetRequiredService<ThemeService>();
            if (value == null) return new SolidColorBrush((Color)themeService.ThemeResource["TextColor"]);
            return value.ToString() == parameter.ToString()
                ? new SolidColorBrush((Color)themeService.ThemeResource["SystemAccentColor"])
                : new SolidColorBrush((Color)themeService.ThemeResource["TextColor"]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Colors.Black;
        }
    }
}
