using Newtonsoft.Json;

namespace BiliLite.Models.Common.Home
{
    public class CinemaHomeStatModel
    {
        public long View { get; set; }

        [JsonProperty("follow_view")]
        public string FollowView { get; set; }

        public long Follow { get; set; }

        public long Danmaku { get; set; }
    }
}