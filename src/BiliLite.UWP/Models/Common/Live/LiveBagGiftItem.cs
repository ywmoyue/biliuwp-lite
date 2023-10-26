using Newtonsoft.Json;

namespace BiliLite.Models.Common.Live
{
    public class LiveBagGiftItem
    {
        [JsonProperty("bag_id")]
        public int BagId { get; set; }

        [JsonProperty("gift_name")]
        public string GiftName { get; set; }

        [JsonProperty("gift_id")]
        public int GiftId { get; set; }

        [JsonProperty("gift_type")]
        public int GiftType { get; set; }

        [JsonProperty("gift_num")]
        public int GiftNum { get; set; }

        public int Type { get; set; }

        [JsonProperty("card_gif")]
        public string CardGif { get; set; }

        [JsonProperty("corner_mark")]
        public string CornerMark { get; set; }

        [JsonProperty("expire_at")]
        public long ExpireAt { get; set; }

        public string Img { get; set; }
    }
}