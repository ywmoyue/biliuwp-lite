using Newtonsoft.Json;

namespace BiliLite.Models.Common.User.SendDynamic
{
    public class SendImagesModel
    {
        [JsonProperty("img_height")]
        public int ImgHeight { get; set; }
        [JsonProperty("img_src")]
        public string ImgSrc { get; set; }

        [JsonProperty("img_size")]
        public double ImgSize { get; set; }

        [JsonProperty("img_width")]
        public int ImgWidth { get; set; }
    }
}