using BiliLite.Models.Common.Video.PlayUrlInfos;

namespace BiliLite.Models.Common.Player
{
    public class BasePlayInfo
    {
        public virtual BiliVideoPlayMode PlayMode { get; }
    }

    public class BaseDashPlayInfo : BasePlayInfo
    {
        public override BiliVideoPlayMode PlayMode => BiliVideoPlayMode.Dash;

        public BiliDashPlayUrlInfo DashInfo { get; set; }

        public string UserAgent { get; set; }

        public string Referer { get; set; }

        public double Positon { get; set; } = 0;
    }
}
