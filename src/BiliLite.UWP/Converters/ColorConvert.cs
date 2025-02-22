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
                        string1 = "#" + string1;
                    }
                    switch (string1.Length)
                    {
                        case < 7:
                            string1 = "#00000000";
                            break;
                        case 8:
                            string1 = string1.Remove(string1.Length - 1);
                            break;
                        case > 9:
                            string1 = string1.Substring(0, 9);
                            break;
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
