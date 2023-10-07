using Newtonsoft.Json;

namespace BiliLite.Modules.LiveRoomDetailModels
{
    public class LiveGuardRankItem
    {
        public string Username { get; set; }

        public long Uid { get; set; }

        public long Ruid { get; set; }

        public string Face { get; set; }

        [JsonProperty("guard_level")]
        public int GuardLevel { get; set; }

        [JsonProperty("rank_img")]
        public string RankImg => "ms-appx:///Assets/Live/ic_live_guard_" + GuardLevel + ".png";
    }
}