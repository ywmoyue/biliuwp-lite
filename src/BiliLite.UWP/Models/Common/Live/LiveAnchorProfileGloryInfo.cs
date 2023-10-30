using BiliLite.Extensions;
using BiliLite.Services;
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

        private string jumpUrl;
        [JsonProperty("jump_url")]
        public string JumpUrl
        {
            get => jumpUrl;
            set
            {
                jumpUrl = value.IsUrl() ? value : ApiHelper.NOT_FOUND_URL;
            }
        }
    }
}