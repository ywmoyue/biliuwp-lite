using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Data;
using BiliLite.Models.Common.Live;

namespace BiliLite.Converters;

public class LiveQualitySliderTooltipConverter : IValueConverter
{
    public List<LiveRoomWebUrlQualityDescriptionItemModel> Qualites { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double sliderValue)
        {
            return sliderValue >= 0 && sliderValue < Qualites.Count
                ? Qualites[(int)sliderValue].Desc
                : sliderValue.ToString();
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value;
    }
}