using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
namespace BiliLite.Converters
{
    public class ColorSelectedConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return new SolidColorBrush((Color)App.Current.Resources["TextColor"]);
            return value.ToString() == parameter.ToString()
                ? new SolidColorBrush((Color)App.Current.Resources["SystemAccentColor"])
                : new SolidColorBrush((Color)App.Current.Resources["TextColor"]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Colors.Black;
        }
    }
}
