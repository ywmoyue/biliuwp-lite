using System.Collections.Generic;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Live
{
    public class LiveRoomEndAnchorLotteryInfoModel
    {
        public int Id { get; set; }

        [JsonProperty("lot_status")]
        public int LotStatus { get; set; }

        [JsonProperty("award_image")]
        public string AwardImage { get; set; }

        [JsonProperty("award_name")]
        public string AwardName { get; set; }

        [JsonProperty("award_num")]
        public int AwardNum { get; set; }

        public string Url { get; set; }

        [JsonProperty("web_url")]
        public string WebUrl { get; set; }

        [JsonProperty("award_users")]
        public List<LiveRoomEndAnchorLotteryInfoUserModel> AwardUsers { get; set; }
    }
}