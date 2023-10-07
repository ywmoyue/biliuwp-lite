using Newtonsoft.Json;

namespace BiliLite.Models.Common.Live
{
    public class LiveBuvidModel
    {
        [JsonProperty("b_3")]
        public string B3 { get; set; }

        [JsonProperty("b_4")]
        public string B4 { get; set; }
    }
}