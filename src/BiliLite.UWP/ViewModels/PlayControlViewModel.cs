using BiliLite.Modules.Player;
using BiliLite.ViewModels.Common;
using System.Collections.Generic;
using BiliLite.Models.Common;
using BiliLite.Services;
using BiliLite.Modules;

namespace BiliLite.ViewModels
{
    public class PlayControlViewModel: BaseViewModel
    {
        public List<InteractionEdgeInfoQuestionModel> Questions { get; set; }

        public bool ShowVideoBottomVirtualProgressBar =>
            SettingService.GetValue(SettingConstants.Player.SHOW_VIDEO_BOTTOM_VIRTUAL_PROGRESS_BAR,
                SettingConstants.Player.DEFAULT_SHOW_VIDEO_BOTTOM_VIRTUAL_PROGRESS_BAR);

        public bool ShowViewPointsView { get; set; }

        public bool ShowViewPointsBtn { get; set; }

        public bool ShowWebPlayerToolbar { get; set; }

        public List<PlayerInfoViewPoint> ViewPoints { get; set; }

        public double Position { get; set; }
    }
}
