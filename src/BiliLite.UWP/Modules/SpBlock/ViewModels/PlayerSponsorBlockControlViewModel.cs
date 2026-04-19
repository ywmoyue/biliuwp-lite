using System.Collections.Generic;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common.Player;
using BiliLite.ViewModels.Common;

namespace BiliLite.Modules.SpBlock.ViewModels
{
    [RegisterTransientViewModel]
    public class PlayerSponsorBlockControlViewModel : BaseViewModel
    {
        public bool ShowSponsorBlockBtn { get; set; }

        public List<PlayerSkipItem> SponsorBlockSegmentList { get; set; }
    }
}
