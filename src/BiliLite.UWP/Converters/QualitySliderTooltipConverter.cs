using System;
using BiliLite.Models.Common.Video.PlayUrlInfos;
using System.Collections.Generic;
using Windows.UI.Xaml.Data;

namespace BiliLite.Converters;

public class QualitySliderTooltipConverter : IValueConverter
{
    public List<BiliPlayUrlInfo> Qualites { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double sliderValue)
        {
            return sliderValue >= 0 && sliderValue < Qualites.Count
                ? Qualites[(int)sliderValue].QualityName
                : sliderValue.ToString();
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value;
    }
}