using System.Collections.Generic;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Rank;
using BiliLite.ViewModels.Common;
using PropertyChanged;

namespace BiliLite.ViewModels.Rank
{
    public class RankRegionViewModel : BaseViewModel
    {
        public RankRegionViewModel(int id, string rname, RankRegionType type = RankRegionType.All)
        {
            this.Rid = id;
            this.Name = rname;
            this.Type = type;
        }

        [DoNotNotify]
        public string Name { get; set; }

        [DoNotNotify]
        public int Rid { get; set; }

        public string ToolTip { get; set; }

        [DoNotNotify]
        public RankRegionType Type { get; set; }

        public List<RankItemModel> Items { get; set; }
    }
}