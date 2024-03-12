using Newtonsoft.Json;

namespace BiliLite.Models.Common.User.UserDetails
{
    public class UserCenterInfoNameplateModel
    {
        public int Nid { get; set; }

        public string Name { get; set; }

        public string Image { get; set; }

        [JsonProperty("image_small")]
        public string ImageSmall { get; set; }

        public string Level { get; set; }

        public string Condition { get; set; }
    }
}