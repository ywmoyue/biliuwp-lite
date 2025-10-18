namespace BiliLite.Models.Common.Danmaku;

public class DanmakuSimpleItem
{
    public string Id { get; set; }

    public string Content { get; set; }

    public string MidHash { get; set; }

    public int ProgressMs { get; set; }

    public int ProgressS => ProgressMs / 1000;
}