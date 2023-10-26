using Newtonsoft.Json;

namespace BiliLite.Models.Common.Live
{
    public class LiveAnchorInfoLiveInfoModel
    {
        public int Level { get; set; }

        [JsonProperty("level_color")]
        public int LevelColor { get; set; }
    }
}