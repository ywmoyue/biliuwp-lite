using Newtonsoft.Json;

namespace BiliLite.Models.Common.Live
{
    public class LiveRoomSuperChatUserInfoModel
    {
        public string Uname { get; set; }

        public string Face { get; set; }

        [JsonProperty("face_frame")]
        public string FaceFrame { get; set; }

        [JsonProperty("guard_level")]
        public UserCaptainType GuardLevel { get; set; }

        [JsonProperty("user_level")]
        public int UserLevel { get; set; }

        [JsonProperty("is_vip")]
        public int IsVip { get; set; }

        [JsonProperty("is_svip")]
        public int IsSvip { get; set; }

        [JsonProperty("is_main_vip")]
        public int IsMainVip { get; set; }
    }
}