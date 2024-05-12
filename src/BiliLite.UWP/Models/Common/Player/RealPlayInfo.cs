using System.Collections.Generic;

namespace BiliLite.Models.Common.Player
{
    public class RealPlayInfo
    {
        public RealPlayUrls PlayUrls { get; set; } = new RealPlayUrls();

        public bool IsAutoPlay { get; set; }

        public string ManualPlayUrl { get; set; }
    }

    public class RealPlayUrls
    {
        public List<BasePlayUrlInfo> HlsUrls { get; set; }

        public List<BasePlayUrlInfo> FlvUrls { get; set; }
    }
}
