using BiliLite.Models.Common.Video.PlayUrlInfos;
using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Data;

namespace BiliLite.Converters;

public class SoundQualitySliderTooltipConverter : IValueConverter
{
    public List<BiliDashAudioPlayUrlInfo> AudioQualites { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double sliderValue)
        {
            return sliderValue >= 0 && sliderValue < AudioQualites.Count
                ? AudioQualites[(int)sliderValue].QualityName
                : sliderValue.ToString();
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value;
    }
}