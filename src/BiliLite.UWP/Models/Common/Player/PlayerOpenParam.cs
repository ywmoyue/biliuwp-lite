using BiliLite.Models.Common.Video.PlayUrlInfos;

namespace BiliLite.Models.Common.Player;

public class PlayerOpenParam
{
    public BiliDashPlayUrlInfo DashInfo { get; set; }
    public string UserAgent { get; set; }
    public string Referer { get; set; }
    public double Positon { get; set; } = 0;
    public bool needConfig { get; set; } = true;
    public bool IsLocal { get; set; } = false;
}
