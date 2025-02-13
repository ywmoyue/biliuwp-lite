using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace BiliLite.Converters
{
    public class ColorConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return new SolidColorBrush(Colors.Transparent);
            }

            Color color = new();
            if (value is string string1)
            {
                try
                {
                    color = Microsoft.Toolkit.Uwp.Helpers.ColorHelper.ToColor(string1);
                }
                catch (Exception)
                {
                    color = Colors.Transparent;
                }
            }
            if (value is Color color1)
            {
                color = color1;
            }

            if (parameter != null)
            {
                return color;
            }
            return new SolidColorBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
