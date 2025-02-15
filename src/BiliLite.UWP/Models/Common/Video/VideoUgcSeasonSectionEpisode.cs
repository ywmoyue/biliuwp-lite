﻿using BiliLite.Models.Common.Video.Detail;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Video
{
    public class VideoUgcSeasonSectionEpisode
    {
        private string m_cover;

        public long Id { get; set; }

        [JsonProperty("section_id")]
        public long SectionId { get; set; }

        public string Aid { get; set; }

        public string Cid { get; set; }

        public string Title { get; set; }

        public string Cover
        {
            get => m_cover ?? Arc.Pic;
            set => m_cover = value;
        }

        [JsonProperty("cover_right_text")]
        public string CovverRightText { get; set; }

        //public int Page { get; set; }

        public string Part { get; set; }

        public long Duration { get; set; }

        public string Vid { get; set; }

        public string Bvid { get; set; }

        public VideoAuthor Author { get; set; }

        [JsonProperty("author_desc")]
        public string AuthorDesc { get; set; }

        [JsonProperty("first_frame")]
        public string FirstFrame { get; set; }

        [JsonProperty("arc")]
        public VideoDetailModel Arc { get; set; } // 简单复用一下

        public VideoUgcSeasonSectionEpisodePage Page { get; set; }
    }

    public class VideoUgcSeasonSectionEpisodePage
    {
        public long Duration { get; set; }
    }
}
