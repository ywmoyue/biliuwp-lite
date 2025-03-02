using System;

namespace BiliLite.Converters;

public static class TimeSpanStrFormatConverter
{
    public static string Convert(string timeSpanStr)
    {
        // 解析输入字符串
        var parts = timeSpanStr.Split(':');

        // 根据分部的数量解析时间
        int hours = 0, minutes = 0, seconds = 0;

        switch (parts.Length)
        {
            case 1: // 只有秒数
                seconds = int.Parse(parts[0]);
                break;
            case 2: // 分钟和秒数
                minutes = int.Parse(parts[0]);
                seconds = int.Parse(parts[1]);
                break;
            case 3: // 小时、分钟和秒数
                hours = int.Parse(parts[0]);
                minutes = int.Parse(parts[1]);
                seconds = int.Parse(parts[2]);
                break;
            default:
                throw new ArgumentException("输入字符串格式不正确，应为 '秒'、'分钟:秒' 或 '小时:分钟:秒'。");
        }

        // 创建 TimeSpan 对象
        var timeSpan = new TimeSpan(hours, minutes, seconds);

        // 根据小时是否为0来动态调整格式字符串
        var format = timeSpan.Hours == 0 ? @"mm\:ss" : @"hh\:mm\:ss";
        // 根据指定格式格式化 TimeSpan
        return timeSpan.ToString(format);
    }

    public static string Convert(TimeSpan timeSpan)
    {
        // 根据小时是否为0来动态调整格式字符串
        var format = timeSpan.Hours == 0 ? @"mm\:ss" : @"hh\:mm\:ss";
        // 根据指定格式格式化 TimeSpan
        return timeSpan.ToString(format);
    }
}