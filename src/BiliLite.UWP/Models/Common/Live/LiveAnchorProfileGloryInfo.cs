using BiliLite.Services;
using Newtonsoft.Json;
using System;

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
                if (Uri.TryCreate(value, UriKind.Absolute, out Uri _))
                {
                    jumpUrl = value;
                }
                else
                {
                    jumpUrl = ApiHelper.NOT_FOUND_URL;
                }
            }
        }
    }
}