using Newtonsoft.Json;

namespace BiliLite.Models.Common.User.UserDetails
{
    public class UserCenterInfoVipLabelModel
    {
        public string Path { get; set; }

        public string Text { get; set; }

        [JsonProperty("label_theme")]
        public string LabelTheme { get; set; }
    }
}