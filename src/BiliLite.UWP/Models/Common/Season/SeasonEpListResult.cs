using System.Collections.Generic;

namespace BiliLite.Models.Common.Season;

public class SeasonEpListResult
{
    public List<SeasonDetailEpisodeModel> Episodes { get; set; }

    public List<SeasonSection> Section { get; set; }
}

public class SeasonSection
{
    public long Id { get; set; }

    public string Title { get; set; }

    public List<SeasonDetailEpisodeModel> Episodes { get; set; }
}