using System;
using Windows.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using CommunityToolkit.WinUI.Helpers;

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
                    color = stringValue.ToColor();// Microsoft.Toolkit.Uwp.Helpers.ColorHelper.ToColor(stringValue);
                }
                catch (Exception)
                {
                    color = Colors.Transparent;
                }
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
