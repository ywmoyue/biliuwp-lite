using Newtonsoft.Json;

namespace BiliLite.Models.Common.Video
{
    public class BiliVideoTag
    {
        [JsonProperty("tag_id")]
        public string TagId { get; set; }

        [JsonProperty("tag_name")]
        public string TagName { get; set; }
    }
}
