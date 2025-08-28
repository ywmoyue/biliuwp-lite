using System;
using Windows.UI.Xaml.Data;

namespace BiliLite.Converters
{
    public class CountOrTextConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ConvertCountOrText(value, parameter?.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return "";
        }

        /// <summary>
        /// 将数字转换为带单位的字符串(如1.2万)或返回默认文本
        /// </summary>
        /// <param name="value">输入值，可以是数字或字符串形式的数字</param>
        /// <param name="defaultText">当数值≤0时显示的默认文本</param>
        /// <returns>格式化后的字符串</returns>
        public static string ConvertCountOrText(object value, string defaultText = "")
        {
            if (value == null)
            {
                return defaultText ?? string.Empty;
            }

            // 处理字符串类型的数值
            if (value is string valueStr)
            {
                if (long.TryParse(valueStr, out var parsedLong))
                {
                    value = parsedLong;
                }
                else if (double.TryParse(valueStr, out var parsedDouble))
                {
                    value = parsedDouble;
                }
                else
                {
                    return defaultText ?? string.Empty;
                }
            }

            // 处理数值类型
            if (value is int || value is long || value is double)
            {
                var number = System.Convert.ToDouble(value);

                if (number <= 0)
                {
                    return defaultText ?? string.Empty;
                }

                if (number >= 10000)
                {
                    return (number / 10000).ToString("0.0") + "万";
                }

                return number.ToString("0");
            }

            return value.ToString();
        }
    }
}

