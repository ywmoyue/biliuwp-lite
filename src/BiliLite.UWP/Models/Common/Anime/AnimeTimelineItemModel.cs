using Newtonsoft.Json;

namespace BiliLite.Models.Common.Anime;

public class AnimeTimelineItemModel : ISeasonItem
{
    [JsonProperty("season_id")]
    public int SeasonId { get; set; }

    public string Cover { get; set; }

    [JsonProperty("square_cover")]
    public string SquareCover { get; set; }

    [JsonProperty("pub_index")]
    public string PubIndex { get; set; }

    [JsonProperty("pub_time")]
    public string PubTime { get; set; }

    public string Title { get; set; }
}