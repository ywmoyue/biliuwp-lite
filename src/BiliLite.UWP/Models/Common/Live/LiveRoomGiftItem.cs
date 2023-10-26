using Newtonsoft.Json;

namespace BiliLite.Models.Common.Live
{
    public class LiveRoomGiftItem
    {
        public int Position { get; set; }

        [JsonProperty("gift_id")]
        public int GiftId { get; set; }

        public int Id { get; set; }

        [JsonProperty("plan_id")]
        public int PlanId { get; set; }
    }
}