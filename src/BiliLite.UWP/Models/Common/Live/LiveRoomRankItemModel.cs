using BiliLite.Models.Common;
using Newtonsoft.Json;

namespace BiliLite.Modules.LiveRoomDetailModels
{
    public class LiveRoomRankItemModel
    {
        public int Rank { get; set; }

        public long Uid { get; set; }

        public string Uname { get; set; }

        public string Face { get; set; }

        public int Score { get; set; }

        public string Icon { get; set; } = Constants.App.TRANSPARENT_IMAGE;

        [JsonProperty("show_right")]
        public bool ShowRight => Score != 0;

        [JsonProperty("medal_name")]
        public string MedalName { get; set; }

        public string Level { get; set; }

        public string Color { get; set; }

        [JsonProperty("show_medal")]
        public bool ShowMedal => !string.IsNullOrEmpty(MedalName);
    }
}