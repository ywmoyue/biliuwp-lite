using Newtonsoft.Json;

namespace BiliLite.Models.Common.Live
{
    public class LiveRoomRankItemModel
    {
        public int Rank { get; set; }

        public long Uid { get; set; }

        private string _name { get; set; }

        public string Uname
        {
            get => _name;
            set => _name = value;
        }

        [JsonProperty("name")]
        public string Name
        {
            get => _name;
            set => _name = value;
        }

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