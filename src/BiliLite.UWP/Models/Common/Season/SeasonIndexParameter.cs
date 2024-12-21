namespace BiliLite.Models.Common.Season;

public class SeasonIndexParameter
{
    public IndexSeasonType Type { get; set; } = IndexSeasonType.Anime;

    public string Area { get; set; } = "-1";

    public string Style { get; set; } = "-1";

    public string Year { get; set; } = "-1";

    public string Month { get; set; } = "-1";

    public string Order { get; set; } = "3";
}