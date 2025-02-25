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
                    if (!string1.Contains("#"))
                    {
                        if (long.TryParse(string1, out var c))
                        {
                            string1 = c.ToString("X2");
                        }
                        int desiredLength = string1.Length <= 6 ? 6 : 8;
                        string1 = string1.PadLeft(desiredLength, '0');

                        string1 = "#" + string1;
                    }

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
