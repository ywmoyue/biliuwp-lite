using Newtonsoft.Json;

namespace BiliLite.Models.Common.Live
{
    public class LiveAnchorProfilePkInfo
    {
        [JsonProperty("season_id")]
        public int SeasonId { get; set; }

        [JsonProperty("season_name")]
        public string SeasonName { get; set; }

        [JsonProperty("season_date_start")]
        public string SeasonDateStart { get; set; }

        [JsonProperty("season_date_end")]
        public string SeasonDateEnd { get; set; }

        [JsonProperty("pk_rank_name")]
        public string PkRankName { get; set; }

        [JsonProperty("first_pic_url")]
        public string FirstPicUrl { get; set; }

        [JsonProperty("pk_rank_star")]
        public int PkRankStar { get; set; }

        [JsonProperty("second_rank_icon")]
        public string SecondRankIcon { get; set; }

        [JsonProperty("rank_info_url_app")]
        public string RankInfoUrlApp { get; set; }
    }
}