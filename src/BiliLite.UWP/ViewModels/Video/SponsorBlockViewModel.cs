using System;
using System.Collections.Generic;
using System.Linq;

namespace BiliLite.ViewModels.Video
{
    public class SponsorBlockSegment
    {
        public float[] Segment { get; set; }
        public string Cid { get; set; }
        public string Uuid { get; set; }
        public string Category { get; set; }
        public string ActionType { get; set; }
        public int Locked { get; set; }
        public int Votes { get; set; }
        public float VideoDuration { get; set; }
    }

    public class SponsorBlockVideo
    {
        public string VideoId { get; set; }
        public List<SponsorBlockSegment> Segments { get; set; }
    }

    public class SponsorBlockViewModel
    {
        public List<SponsorBlockVideo> Videos { get; set; }
    }
}
