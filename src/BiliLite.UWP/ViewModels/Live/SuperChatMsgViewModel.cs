using BiliLite.ViewModels.Common;
using Newtonsoft.Json;
using PropertyChanged;

namespace BiliLite.ViewModels.Live
{
    public class SuperChatMsgViewModel : BaseViewModel
    {
        [DoNotNotify]
        public string Username { get; set; }

        [DoNotNotify]
        public string Face { get; set; }

        [DoNotNotify]
        [JsonProperty("face_frame")]
        public string FaceFrame { get; set; }

        [DoNotNotify]
        public string Message { get; set; }

        [DoNotNotify]
        [JsonProperty("message_jpn")]
        public string MessageJpn { get; set; }

        [DoNotNotify]
        [JsonProperty("background_image")]
        public string BackgroundImage { get; set; }

        [DoNotNotify]
        [JsonProperty("start_time")]
        public int StartTime { get; set; }

        [DoNotNotify]
        [JsonProperty("end_time")]
        public int EndTime { get; set; }
        public int Time { get; set; }

        [DoNotNotify]
        [JsonProperty("max_time")]
        public int MaxTime { get; set; }

        [DoNotNotify]
        public int Price { get; set; }

        [DoNotNotify]
        [JsonProperty("price_gold")]
        public int PriceGold => Price * 100;

        [DoNotNotify]
        [JsonProperty("background_color")]
        public string BackgroundColor { get; set; }

        [DoNotNotify]
        [JsonProperty("background_bottom_color")]
        public string BackgroundBottomColor { get; set; }

        [DoNotNotify]
        [JsonProperty("font_color")]
        public string FontColor { get; set; }

        [DoNotNotify]
        [JsonProperty("guard_level")]
        public int GuardLevel { get; set; }

        [DoNotNotify]
        [JsonProperty("uid")]
        public long Uid { get; set; }

        [DoNotNotify]
        [JsonProperty("medal_name")]
        public string MedalName { get; set; }

        [DoNotNotify]
        [JsonProperty("medal_level")] 
        public int MedalLevel { get; set; }

        [DoNotNotify]
        [JsonProperty("medal_color")]
        public string MedalColor { get; set; }
    }
}