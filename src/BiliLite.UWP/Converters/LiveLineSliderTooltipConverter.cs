using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Data;
using BiliLite.Models.Common.Player;

namespace BiliLite.Converters;

public class LiveLineSliderTooltipConverter : IValueConverter
{
    public List<BasePlayUrlInfo> Lines { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double sliderValue)
        {
            return sliderValue >= 0 && sliderValue < Lines.Count
                ? Lines[(int)sliderValue].Name
                : sliderValue.ToString();
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value;
    }
}