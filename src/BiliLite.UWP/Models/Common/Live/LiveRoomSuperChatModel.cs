using Newtonsoft.Json;

namespace BiliLite.Modules.LiveRoomDetailModels
{
    public class LiveRoomSuperChatModel
    {
        public int Id { get; set; }

        public long Uid { get; set; }

        [JsonProperty("background_image")]
        public string BackgroundImage { get; set; }

        [JsonProperty("background_color")]
        public string BackgroundColor { get; set; }

        [JsonProperty("background_icon")]
        public string BackgroundIcon { get; set; }

        [JsonProperty("background_bottom_color")]
        public string BackgroundBottomColor { get; set; }

        [JsonProperty("background_price_color")]
        public string BackgroundPriceColor { get; set; }

        [JsonProperty("font_color")]
        public string FontColor { get; set; }

        public int Price { get; set; }

        public int Rate { get; set; }

        public int Time { get; set; }
            
        [JsonProperty("start_time")]
        public int StartTime { get; set; }

        [JsonProperty("end_time")]
        public int EndTime { get; set; }

        public string Message { get; set; }

        [JsonProperty("trans_mark")]
        public int TransMark { get; set; }

        [JsonProperty("message_trans")]
        public string MessageTrans { get; set; }

        public int Ts { get; set; }

        public string Token { get; set; }

        [JsonProperty("user_info")]
        public LiveRoomSuperChatUserInfoModel UserInfo { get; set; }
    }
}