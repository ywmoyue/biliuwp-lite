using Bilibili.Rpc;
using System;
using Windows.UI.Xaml.Data;

namespace BiliLite.Converters;

public class LiveStatusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) => System.Convert.ToInt32(value) switch
    {
        0 => "未开播",
        1 => "直播中",
        2 => "轮播中",
        _ => "未知"
    };

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}