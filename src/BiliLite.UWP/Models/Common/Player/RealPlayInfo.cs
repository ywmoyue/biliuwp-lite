using System.Collections.Generic;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Video.PlayUrlInfos;

namespace BiliLite.Models.Common.Player
{
    public class RealPlayInfo
    {
        public RealPlayUrls PlayUrls { get; set; } = new RealPlayUrls();

        public bool IsAutoPlay { get; set; }

        public string ManualPlayUrl { get; set; }

        /// <summary>
        /// 播放媒体类型（直播/点播统一入口时用于区分）
        /// </summary>
        public PlayMediaType PlayMediaType { get; set; }

        /// <summary>
        /// 点播 Dash 信息
        /// </summary>
        public BiliDashPlayUrlInfo DashInfo { get; set; }

        /// <summary>
        /// 点播单链接（mp4/flv）
        /// </summary>
        public string SingleUrl { get; set; }

        /// <summary>
        /// 点播是否 flv 单段
        /// </summary>
        public bool SingleIsFlv { get; set; }

        /// <summary>
        /// 点播多段 flv
        /// </summary>
        public List<BiliFlvPlayUrlInfo> MultiFlvUrls { get; set; }

        public string UserAgent { get; set; }

        public string Referer { get; set; }

        public bool IsLocal { get; set; }

        public IDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 分段时长（秒）
        /// </summary>
        public List<double> SegmentDurations { get; set; } = new List<double>();

        /// <summary>
        /// 总时长（秒）
        /// </summary>
        public double TotalDuration { get; set; }

        /// <summary>
        /// 默认优先引擎类型
        /// </summary>
        public RealPlayerType PreferredPlayerType { get; set; } = RealPlayerType.Native;

        public List<RealPlayerType> FallbackPlayerTypes { get; set; } = new List<RealPlayerType>();
    }

    public class RealPlayUrls
    {
        public List<BasePlayUrlInfo> HlsUrls { get; set; }

        public List<BasePlayUrlInfo> FlvUrls { get; set; }
    }
}
