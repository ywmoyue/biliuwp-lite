using Newtonsoft.Json;

namespace BiliLite.Models.Common.Season;

public class SeasonRankItemNewEPModel
{
    public string Cover { get; set; }

    [JsonProperty("index_show")]
    public string IndexShow { get; set; }
}