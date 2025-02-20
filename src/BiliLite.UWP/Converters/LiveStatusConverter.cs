namespace BiliLite.Converters;

public static class LiveStatusConverter
{
    public static string Convert(int status)
    {
        return status switch
        {
            0 => "未开播",
            1 => "直播中",
            2 => "轮播中",
            _ => "未知"
        };
    }
}