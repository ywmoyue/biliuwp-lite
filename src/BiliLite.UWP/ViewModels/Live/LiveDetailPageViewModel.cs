using BiliLite.Models.Common;
using BiliLite.Player.States.ContentStates;
using BiliLite.Player.States.PlayStates;
using BiliLite.Player.States.ScreenStates;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using PropertyChanged;
using Windows.UI.Xaml;
using BiliLite.Models.Common.Player;

namespace BiliLite.ViewModels.Live
{
    public class LiveDetailPageViewModel : BaseViewModel
    {
        public bool ShowMediaPlayer { get; set; } = true;

        public bool ShowWebPlayer { get; set; } = false;

        public string DanmakuInput { get; set; }

        public bool IsPaused { get; set; } = true;

        public IPlayState PlayState { get; set; }

        public IContentState ContentState { get; set; }

        public IScreenState ScreenState { get; set; }

        [DependsOn(nameof(IsPaused))]
        public bool ShowResumeBtn => IsPaused;

        [DependsOn(nameof(IsPaused))]
        public bool ShowPauseBtn => !IsPaused;

        [DependsOn(nameof(PlayState))]
        public string LoadText
        {
            get
            {
                if (PlayState == null) return "";
                if (PlayState.IsLoading)
                {
                    return "加载中";
                }

                return PlayState.IsBuffering ? "缓冲中" : "";
            }
        }

        [DependsOn(nameof(PlayState))]
        public bool ShowLoading => PlayState != null && (PlayState.IsLoading || PlayState.IsBuffering);

        [DependsOn(nameof(PlayState))]
        public bool Living => PlayState is { IsStopped: false };

        [DependsOn(nameof(ContentState))]
        public bool ShowFullWindowBtn => !(ContentState is { IsFullWindow: true });

        [DependsOn(nameof(ContentState))]
        public bool ShowExitFullWindowBtn => (ContentState is { IsFullWindow: true });

        [DependsOn(nameof(ScreenState))]
        public bool ShowFullscreenBtn => !(ScreenState is { IsFullscreen: true });

        [DependsOn(nameof(ScreenState))]
        public bool ShowExitFullscreenBtn => (ScreenState is { IsFullscreen: true });

        [DependsOn(nameof(ContentState), nameof(ScreenState))]
        public GridLength RightInfoWidth =>
            (ContentState is { IsFullWindow: true }) || (ScreenState is { IsFullscreen: true })
                ? new GridLength(0, GridUnitType.Pixel)
                : new GridLength(280, GridUnitType.Pixel);

        [DependsOn(nameof(ContentState), nameof(ScreenState))]
        public GridLength BottomInfoHeight =>
            (ContentState is { IsFullWindow: true }) || (ScreenState is { IsFullscreen: true })
                ? new GridLength(0, GridUnitType.Pixel)
                : GridLength.Auto;

        [DependsOn(nameof(ScreenState))]
        public Thickness Margin =>
            (ScreenState is { IsFullscreen: true })
                ? new Thickness(0, SettingService.GetValue(SettingConstants.UI.DISPLAY_MODE, 0) == 0 ? -48 : -48, 0, 0)
                : new Thickness(0);

        public LivePlayerMode LivePlayerMode { get; set; }

        public RealPlayerType RealPlayerType
        {
            set
            {
                if (value == RealPlayerType.FFmpegInterop)
                {
                    ShowMediaPlayer = true;
                    ShowWebPlayer = false;
                }
                else
                {
                    ShowMediaPlayer = false;
                    ShowWebPlayer = true;
                }
            }
        }

        public string LivePlayUrlSource { get; set; }

        public bool ShowBottomGiftBar { get; set; } = true;
    }
}
