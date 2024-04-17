﻿using Newtonsoft.Json;

namespace BiliLite.Models.Common.Anime
{
    public class AnimeRankModel : ISeasonItem
    {
        public string Display { get; set; }

        public string Title { get; set; }

        public string Cover { get; set; }

        [JsonProperty("season_id")]
        public int SeasonId { get; set; }

        [JsonProperty("index_show")]
        public string IndexShow { get; set; }

        public long Follow { get; set; }

        public long Danmaku { get; set; }

        public long View { get; set; }

        [JsonProperty("show_badge")]
        public bool ShowBadge => !string.IsNullOrEmpty(Badge);

        public string Badge { get; set; }
    }
}