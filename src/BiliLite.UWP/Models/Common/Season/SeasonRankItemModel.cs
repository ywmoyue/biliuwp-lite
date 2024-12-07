using Newtonsoft.Json;

namespace BiliLite.Models.Common.Season;

public class SeasonRankItemModel
{
    public int Rank { get; set; }

    public string Badge { get; set; }

    public string Desc { get; set; }

    [JsonProperty("season_id")]
    public string SeasonId { get; set; }

    public string Title { get; set; }

    public string Url { get; set; }

    public string Cover { get; set; }

    [JsonProperty("badge_type")]
    public int BadgeType { get; set; }

    public int Pts { get; set; }

    public bool ShowBadge => !string.IsNullOrEmpty(Badge);

    public bool ShowDanmaku => Stat != null && Stat.Danmaku != 0;

    public SeasonRankItemStatModel Stat { get; set; }

    [JsonProperty("new_ep")]
    public SeasonRankItemNewEPModel NewEp { get; set; }
}