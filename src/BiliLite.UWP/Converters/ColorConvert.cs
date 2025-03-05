using System;
using System.Diagnostics;
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
                return new SolidColorBrush(Colors.Red);
            }

            Color color = new();
            if (value is Color colorValue)
            {
                color = colorValue;
            }
            else
            {
                var stringValue = value.ToString();
                try
                {
                    if (!stringValue.Contains("#"))
                    {
                        if (long.TryParse(stringValue, out var c))
                        {
                            stringValue = c.ToString("X2");
                        }
                        int desiredLength = stringValue.Length <= 6 ? 6 : 8;
                        stringValue = stringValue.PadLeft(desiredLength, '0');

                        stringValue = "#" + stringValue;
                    }
                    color = Microsoft.Toolkit.Uwp.Helpers.ColorHelper.ToColor(stringValue);
                }
                catch (Exception)
                {
                    color = Colors.Red;
                }
            }

            if (parameter != null)
            {
                Debug.WriteLine(color);
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
