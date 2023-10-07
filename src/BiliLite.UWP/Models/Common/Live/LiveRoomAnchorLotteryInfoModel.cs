using Newtonsoft.Json;

namespace BiliLite.Modules.LiveRoomDetailModels
{
    public class LiveRoomAnchorLotteryInfoModel
    {
        [JsonProperty("asset_icon")]
        public string AssetIcon { get; set; }

        [JsonProperty("award_image")]
        public string AwardImage { get; set; }

        [JsonProperty("award_name")]
        public string AwardName { get; set; }

        [JsonProperty("award_num")]
        public int AwardNum { get; set; }

        [JsonProperty("cur_gift_num")]
        public int CurGiftNum { get; set; }

        [JsonProperty("current_time")]
        public long CurrentTime { get; set; }

        public string Danmu { get; set; }

        [JsonProperty("gift_id")]
        public int GiftId { get; set; }

        [JsonProperty("show_gift")]
        public bool ShowGift => GiftId != 0;

        [JsonProperty("gift_name")]
        public string GiftName { get; set; }

        [JsonProperty("gift_num")]
        public int GiftNum { get; set; }

        [JsonProperty("gift_price")]
        public int GiftPrice { get; set; }

        [JsonProperty("goaway_time")]
        public int GoawayTime { get; set; }

        public int Id { get; set; }

        [JsonProperty("join_type")]
        public int JoinType { get; set; }

        [JsonProperty("lot_status")]
        public int LotStatus { get; set; }

        [JsonProperty("max_time")]
        public int MaxTime { get; set; }

        [JsonProperty("require_text")]
        public string RequireText { get; set; }

        [JsonProperty("require_type")]
        public int RequireType { get; set; }

        [JsonProperty("require_value")]
        public int RequireValue { get; set; }

        [JsonProperty("room_id")]
        public int RoomId { get; set; }

        [JsonProperty("send_gift_ensure")]
        public int SendGiftEnsure { get; set; }

        [JsonProperty("show_panel")]
        public int ShowPanel { get; set; }

        public int Status { get; set; }

        public int Time { get; set; }

        public string Url { get; set; }

        [JsonProperty("web_url")]
        public string WebUrl { get; set; }
    }
}