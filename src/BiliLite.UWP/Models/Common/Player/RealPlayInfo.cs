using System.Collections.Generic;

namespace BiliLite.Models.Common.Player
{
    public class RealPlayInfo
    {
        public RealPlayUrls PlayUrls { get; set; } = new RealPlayUrls();

        public bool IsAutoPlay { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }
    }

    public class RealPlayUrls
    {
        public List<BasePlayUrlInfo> HlsUrls { get; set; }

        public List<BasePlayUrlInfo> FlvUrls { get; set; }

        public string DashVideoUrl { get; set; }

        public string DashAudioUrl { get; set; }

        public List<VideoFlvUrlInfo> VideoFlvUrls { get; set; }
    }
}
