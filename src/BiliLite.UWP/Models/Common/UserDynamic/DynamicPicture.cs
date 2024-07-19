using Newtonsoft.Json;

namespace BiliLite.Models.Common.UserDynamic
{
    public class DynamicPicture
    {
        [JsonProperty("image_height")]
        public double ImageHeight { get; set; }

        [JsonProperty("image_width")]
        public double ImageWidth { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("img_size")]
        public double ImgSize { get; set; }
    }
}
