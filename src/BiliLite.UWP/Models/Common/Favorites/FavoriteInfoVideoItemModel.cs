using Newtonsoft.Json;

namespace BiliLite.Models.Common.Favorites;

public class FavoriteInfoVideoItemModel
{
    public string Id { get; set; }

    public string Cover { get; set; }

    public string Title { get; set; }

    public long Duration { get; set; }

    public FavoriteInfoVideoItemUpperModel Upper { get; set; }

    [JsonProperty("cnt_info")]
    public FavoriteInfoVideoItemStatModel CntInfo { get; set; }
}