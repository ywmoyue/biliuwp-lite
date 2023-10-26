using Newtonsoft.Json;

namespace BiliLite.Models.Common.Live
{
    public class LiveAnchorProfileGloryInfo
    {
        public string Gid { get; set; }

        public string Name { get; set; }

        [JsonProperty("activity_name")]
        public string ActivityName { get; set; }

        [JsonProperty("activity_date")]
        public string ActivityDate { get; set; }

        [JsonProperty("pic_url")]
        public string PicUrl { get; set; }

        [JsonProperty("jump_url")]
        public string JumpUrl { get; set; }
    }
}