using System.Collections.Generic;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Anime;

public class AnimeTimelineModel
{
    public string Week { get; set; }

    [JsonProperty("day_week")]
    public int DayWeek { get; set; }

    public string Date { get; set; }

    [JsonProperty("is_today")]
    public bool IsToday { get; set; }

    public List<AnimeTimelineItemModel> Seasons { get; set; }
}