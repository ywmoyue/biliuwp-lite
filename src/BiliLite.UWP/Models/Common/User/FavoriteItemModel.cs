using Newtonsoft.Json;

namespace BiliLite.Models.Common.User
{
    public class FavoriteItemModel
    {
        public string Cover { get; set; }

        public int Attr { get; set; }

        public string Intro { get; set; }

        public string Fid { get; set; }

        public string Id { get; set; }

        [JsonProperty("like_state")]
        public int LikeState { get; set; }

        public string Mid { get; set; }

        public string Title { get; set; }

        public int Type { get; set; }

        [JsonProperty("media_count")]
        public int MediaCount { get; set; }

        [JsonProperty("fav_state")]
        public int FavState { get; set; }
    }
}