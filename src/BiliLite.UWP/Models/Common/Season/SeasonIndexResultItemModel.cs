using Newtonsoft.Json;

namespace BiliLite.Models.Common.Season;

public class SeasonIndexResultItemModel
{
    [JsonProperty("season_id")]
    public int SeasonId { get; set; }

    public string Title { get; set; }

    public string Badge { get; set; }

    [JsonProperty("badge_type")]
    public int BadgeType { get; set; }

    public bool ShowBadge => !string.IsNullOrEmpty(Badge);

    public string Cover { get; set; }

    [JsonProperty("index_show")]
    public string IndexShow { get; set; }

    [JsonProperty("is_finish")]
    public int IsFinish { get; set; }

    public string Link { get; set; }

    [JsonProperty("media_id")]
    public int MediaId { get; set; }

    public string Order { get; set; }

    [JsonProperty("order_type")]
    public string OrderType { get; set; }

    public bool ShowScore => 
        OrderType == "score";

    //public SeasonIndexResultItemOrderModel order { get; set; }
}