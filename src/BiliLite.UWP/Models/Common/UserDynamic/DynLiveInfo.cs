using Newtonsoft.Json;

namespace BiliLite.Models.Common.UserDynamic
{
    public class DynLiveInfo
    {
        [JsonProperty("live_play_info")]
        public DynLivePlayInfo PlayInfo { get; set; }
    }

    public class DynLivePlayInfo
    {
        [JsonProperty("room_id")]
        public long RoomId { get; set; }

        [JsonProperty("uid")]
        public long UserId { get; set; }

        [JsonProperty("live_status")]
        public int LiveStatus { get; set; }

        public string Title { get; set; }

        public string Cover { get; set; }

        [JsonProperty("area_name")]
        public string AreaName { get; set; }

        [JsonProperty("watched_show")]
        public DynLiveWatchedShow WatchedShow { get; set; }
    }

    public class DynLiveWatchedShow
    {

        [JsonProperty("text_large")]
        public string TextLarge { get; set; }
    }
}
