using BiliLite.Modules.Player;
using BiliLite.Player.States.PlayStates;
using BiliLite.ViewModels.Common;
using System.Collections.Generic;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Player;
using BiliLite.Services;
using BiliLite.Modules;
using PropertyChanged;

namespace BiliLite.ViewModels
{
    public class PlayControlViewModel : BaseViewModel
    {
        public List<InteractionEdgeInfoQuestionModel> Questions { get; set; }

        /// <summary>
        /// 当前真实播放器类型，驱动 PlayerHost 中真实播放器控件的显示切换。
        /// </summary>
        public RealPlayerType RealPlayerType { get; set; }

        [DependsOn(nameof(RealPlayerType))]
        public bool ShowMediaPlayer => RealPlayerType is RealPlayerType.Native or RealPlayerType.FFmpegInterop;

        [DependsOn(nameof(RealPlayerType))]
        public bool ShowWebPlayer => RealPlayerType is RealPlayerType.ShakaPlayer or RealPlayerType.Mpegts;

        // 同步播放器状态机当前状态，供界面和逻辑统一读取。
        public IPlayState CurrentPlayState { get; set; }

        [DependsOn(nameof(CurrentPlayState))]
        public bool IsMediaLoading => CurrentPlayState?.IsLoading == true;

        public bool ShowVideoBottomVirtualProgressBar =>
            SettingService.GetValue(SettingConstants.Player.SHOW_VIDEO_BOTTOM_VIRTUAL_PROGRESS_BAR,
                SettingConstants.Player.DEFAULT_SHOW_VIDEO_BOTTOM_VIRTUAL_PROGRESS_BAR);

        public bool ShowViewPointsView { get; set; }

        public bool ShowViewPointsBtn { get; set; }

        public bool ShowWebPlayerToolbarButton { get; set; }

        public bool ShowWebPlayerToolbar { get; set; }

        public List<PlayerInfoViewPoint> ViewPoints { get; set; }

        public double Position { get; set; }

        public double Duration { get; set; }
    }
}
