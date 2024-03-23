using Newtonsoft.Json;

namespace BiliLite.Models.Common.User.UserDetails
{
    public class UserCenterInfoPendantModel
    {
        public int Pid { get; set; }

        public string Name { get; set; }

        public string Image { get; set; }

        public int Expire { get; set; }

        [JsonProperty("image_enhance")]
        public string ImageEnhance { get; set; }
    }
}