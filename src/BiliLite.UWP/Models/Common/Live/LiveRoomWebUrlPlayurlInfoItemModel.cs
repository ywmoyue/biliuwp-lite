using Newtonsoft.Json;

namespace BiliLite.Models.Common.Live
{
    public class LiveRoomWebUrlPlayurlInfoItemModel
    {
        [JsonProperty("playurl")]
        public LiveRoomWebUrlPlayurlItemModel PlayUrl { get; set; }
    }
}