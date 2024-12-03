using BiliLite.Models.Common.Anime;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Home
{
    public class CinemaHomeHotItem : ISeasonItem
    {
        public string Hat { get; set; }

        public string Cover { get; set; }

        public string Badge { get; set; }

        [JsonProperty("badge_type")]
        public int BadgeType { get; set; }

        public bool ShowBadge => !string.IsNullOrEmpty(Badge);

        public string Desc { get; set; }

        public string Title { get; set; }

        public string Link { get; set; }

        [JsonProperty("season_id")]
        public int SeasonId { get; set; }

        [JsonProperty("season_type")]
        public int SeasonType { get; set; }

        public string Type { get; set; }

        public int Wid { get; set; }

        public CinemaHomeStatModel Stat { get; set; }
    }
}