using System.Collections.Generic;
using BiliLite.Models.Common.Season;
using BiliLite.ViewModels.Common;
using PropertyChanged;

namespace BiliLite.ViewModels.Season;

public class SeasonRankDataViewModel : BaseViewModel
{
    [DoNotNotify]
    public string Name { get; set; }

    [DoNotNotify]
    public int Type { get; set; }

    public List<SeasonRankItemModel> Items { get; set; }
}