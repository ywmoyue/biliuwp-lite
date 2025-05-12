using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using BiliLite.Models.Common.Player;

namespace BiliLite.Converters;

public class RealPlayerTypeDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not RealPlayerType) return Visibility.Collapsed;
        return value.ToString() == (string)parameter ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}