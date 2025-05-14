using System;
using Windows.UI.Xaml.Data;

namespace BiliLite.Converters
{
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter == null)
            {
                return !(bool)value;
            }
            else
            {
                return value.ToString() != parameter.ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}
