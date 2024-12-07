using Newtonsoft.Json;

namespace BiliLite.Models.Common.Favorites;

public class FavoriteInfoModel
{
    public string Cover { get; set; }

    public int Attr { get; set; }

    public bool Privacy => Attr == 2;

    public string Fid { get; set; }

    public string Id { get; set; }

    [JsonProperty("like_state")]
    public int LikeState { get; set; }

    [JsonProperty("fav_state")]
    public int FavState { get; set; }

    public string Mid { get; set; }

    public string Title { get; set; }
    public int Type { get; set; }

    [JsonProperty("media_count")]
    public int MediaCount { get; set; }

    public FavoriteInfoVideoItemUpperModel Upper { get; set; }
}