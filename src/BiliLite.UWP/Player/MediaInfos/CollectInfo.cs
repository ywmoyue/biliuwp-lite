using BiliLite.Models.Common.Player;

namespace BiliLite.Player.MediaInfos
{
    public class CollectInfo
    {
        public string Type { get; set; }

        public RealPlayInfo RealPlayInfo { get; set; }

        public string Url { get; set; }

        public object Data { get; set; }
    }
}
