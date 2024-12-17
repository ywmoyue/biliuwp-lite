using System.Collections.Generic;

namespace BiliLite.Models.Common.Season;

public class SeasonIndexConditionFilterModel
{
    public string Field { get; set; }

    public string Name { get; set; }

    public SeasonIndexConditionFilterItemModel Current { get; set; }

    public List<SeasonIndexConditionFilterItemModel> Values { get; set; }
}