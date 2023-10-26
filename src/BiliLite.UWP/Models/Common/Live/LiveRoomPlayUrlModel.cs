using Newtonsoft.Json;

namespace BiliLite.Models.Common.Live
{
    public class LiveRoomPlayUrlModel
    {
        [JsonProperty("playurl_info")]
        public LiveRoomWebUrlPlayurlInfoItemModel PlayUrlInfo { get; set; }
    }
}