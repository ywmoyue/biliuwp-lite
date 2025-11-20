using System;
using BiliLite.Services;
using Microsoft.UI.Xaml.Data;

namespace BiliLite.Converters;

public class PlaySpeedSliderTooltipConverter : IValueConverter
{
    private readonly PlaySpeedMenuService m_playSpeedMenuService;

    public PlaySpeedSliderTooltipConverter(PlaySpeedMenuService playSpeedMenuService)
    {
        m_playSpeedMenuService = playSpeedMenuService;
    }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is double sliderValue)
        {
            return sliderValue >= 0 && sliderValue < m_playSpeedMenuService.MenuItems.Count
                ? m_playSpeedMenuService.MenuItems[(int)sliderValue].Content
                : sliderValue.ToString();
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value;
    }
}