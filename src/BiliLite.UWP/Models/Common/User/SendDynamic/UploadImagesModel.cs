using Newtonsoft.Json;

namespace BiliLite.Models.Common.User.SendDynamic
{
    public class UploadImagesModel
    {
        [JsonProperty("image_height")]
        public int ImageHeight { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("image_size")]
        public double ImageSize { get; set; }

        public string Image => ImageUrl + "@120w_120h_1e_1c.jpg";

        [JsonProperty("image_width")]
        public int ImageWidth { get; set; }
    }
}