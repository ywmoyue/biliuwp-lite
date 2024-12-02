using Newtonsoft.Json;

namespace BiliLite.Models.Common.Home
{
    public class LivePendentItemModel
    {
        [JsonProperty("bg_pic")]
        public string BgPic { get; set; }

        [JsonProperty("bg_color")]
        public string BgColor { get; set; }

        public string Text { get; set; }

        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }
}