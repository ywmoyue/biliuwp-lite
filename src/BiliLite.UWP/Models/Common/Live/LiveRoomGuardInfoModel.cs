using Newtonsoft.Json;

namespace BiliLite.Models.Common.Live
{
    public class LiveRoomGuardInfoModel
    {
        public int Count { get; set; }

        [JsonProperty("achievement_level")]
        public int AchievementLevel { get; set; }
    }
}