using System.Collections.Generic;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Video
{
    public class VideoUgcSeason
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string Cover { get; set; }

        public string Intro { get; set; }

        public List<VideoUgcSeasonSection> Sections { get; set; }

        public VideoUgcSeasonStat Stat { get; set; }
    }

    public class VideoUgcSeasonStat
    {
        [JsonProperty("season_id")]
        public long SeasonId { get; set; }

        public long View { get; set; }

        public long Danmaku { get; set; }

        public long Reply { get; set; }
        
        public long Favorite { get; set; }

        public long Coin { get; set; }

        public long Share { get; set; }

        public long Like { get; set; }

        public long Mtime { get; set; }
    }
}
