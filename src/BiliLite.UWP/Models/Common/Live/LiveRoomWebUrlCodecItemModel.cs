using System.Collections.Generic;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Live
{
    public class LiveRoomWebUrlCodecItemModel
    {
        [JsonProperty("current_qn")]
        public int CurrentQn { get; set; }

        [JsonProperty("base_url")]
        public string BaseUrl { get; set; }

        [JsonProperty("codec_name")]
        public string CodecName { get; set; }

        [JsonProperty("accept_qn")]
        public List<int> AcceptQn { get; set; }

        [JsonProperty("url_info")]
        public List<LiveRoomWebUrlUrlinfoItemModel> UrlInfo { get; set; }
    }
}