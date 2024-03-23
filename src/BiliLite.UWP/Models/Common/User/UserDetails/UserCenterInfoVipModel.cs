using Newtonsoft.Json;

namespace BiliLite.Models.Common.User.UserDetails
{
    public class UserCenterInfoVipModel
    {
        public int Type { get; set; }

        public int Status { get; set; }

        [JsonProperty("theme_type")]
        public int ThemeType { get; set; }

        public UserCenterInfoVipLabelModel Label { get; set; }

        [JsonProperty("avatar_subscript")]
        public int AvatarSubscript { get; set; }

        [JsonProperty("nickname_color")]
        public string NicknameColor { get; set; }
    }
}