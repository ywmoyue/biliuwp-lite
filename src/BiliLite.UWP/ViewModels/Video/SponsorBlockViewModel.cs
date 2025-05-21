using System;
using System.Collections.Generic;
using System.Linq;

namespace BiliLite.ViewModels.Video
{
    public class SponsorBlockSegment
    {
        /// <summary>
        /// 片段的起止时间（秒），通常为长度为2的数组，表示开始和结束时间。
        /// </summary>
        public float[] Segment { get; set; }
        /// <summary>
        /// 视频分段的CID（内容ID）。
        /// </summary>
        public string Cid { get; set; }
        /// <summary>
        /// 该分段的唯一标识符（UUID）。
        /// </summary>
        public string Uuid { get; set; }
        /// <summary>
        /// 分段所属的类别（如广告、片头等）。
        /// </summary>
        public string Category { get; set; }
        /// <summary>
        /// 推荐的操作类型（如跳过、静音等）。
        /// </summary>
        public string ActionType { get; set; }
        /// <summary>
        /// 是否被锁定（1为锁定，0为未锁定）。
        /// </summary>
        public int Locked { get; set; }
        /// <summary>
        /// 该分段的投票数。
        /// </summary>
        public int Votes { get; set; }
        /// <summary>
        /// 视频总时长（秒）。
        /// </summary>
        public float VideoDuration { get; set; }
    }

    public class SponsorBlockVideo
    {
        /// <summary>
        /// 视频的bv号
        /// </summary>
        public string VideoId { get; set; }

        /// <summary>
        /// 片段列表
        /// </summary>
        public List<SponsorBlockSegment> Segments { get; set; }
    }

    public class SponsorBlockViewModel
    {
        public List<SponsorBlockVideo> Videos { get; set; }
    }
}
