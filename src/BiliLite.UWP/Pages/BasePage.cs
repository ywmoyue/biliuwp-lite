using BiliLite.Controls;
using BiliLite.Models.Common;
using BiliLite.Services;
using CommunityToolkit.WinUI;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using BiliLite.Controls.Common;

namespace BiliLite.Pages
{
    public class BasePage : Page
    {
        public string Title { get; set; }
        public object NavigationParameter { get; set; }
        public BasePage()
        {
            this.NavigationCacheMode = (SettingService.GetValue<int>(SettingConstants.UI.DISPLAY_MODE, 0) == 1) ? Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled : Windows.UI.Xaml.Navigation.NavigationCacheMode.Disabled;
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back || e.SourcePageType == typeof(BlankPage))
            {
                this.NavigationCacheMode = NavigationCacheMode.Disabled;
            }
            else
            {
                this.Visibility = Visibility.Collapsed;
            }

            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            NavigationParameter = e.Parameter;
            this.Visibility = Visibility.Visible;
        }
    }

    public class PlayPage : BasePage, IPlayPage
    {
        private CustomTabViewItem m_customTabViewItem;
        private bool m_isLoaded;

        public PlayPage() : base()
        {
            Loaded += PlayPage_Loaded;
        }

        private void PlayPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (m_isLoaded) return;
            m_isLoaded = true;
            m_customTabViewItem = this.FindParent<CustomTabViewItem>();
            if (m_customTabViewItem != null)
            {
                m_customTabViewItem.IsPlayButtonVisible = true;
                m_customTabViewItem.PlayButtonClick += CustomTabViewItem_PlayButtonClick;
                Player.PlayerInstance.PlayStateChanged += PlayerInstance_PlayStateChanged;
            }
        }

        private void CustomTabViewItem_PlayButtonClick(object sender, RoutedEventArgs e)
        {
            if (m_customTabViewItem != null)
            {
                if (IsPlaying) Pause();
                else Play();
            }
        }

        private void PlayerInstance_PlayStateChanged(object sender, PlayState e)
        {
            if (m_customTabViewItem != null)
            {
                m_customTabViewItem.IsPlaying = IsPlaying;
            }
        }

        public PlayerControl Player { get; set; }

        public bool IsPlaying => Player.IsPlaying;

        public void Pause()
        {
            Player.Pause();
        }
        public void Play()
        {
            Player.Play();
        }

        public void GotoLastVideo()
        {
            Player.GotoLastVideo();
        }

        public void GotoNextVideo()
        {
            Player.GotoNextVideo();
        }

        public void SlowDown()
        {
            Player.SlowDown();
        }

        public void FastUp()
        {
            Player.FastUp();
        }

        public double GetPlaySpeed()
        {
            return Player.GetPlaySpeed();
        }

        public void SetPlaySpeed(double speed)
        {
            Player.SetPlaySpeed(speed);
        }

        public void ToggleMute()
        {
            Player.ToggleMute();
        }

        public void OpenDevMode()
        {
            Player.OpenDevMode();
        }

        public void StartHighRateSpeedPlay()
        {
            Player.StartHighRateSpeedPlay();
        }

        public void StopHighRateSpeedPlay()
        {
            Player.StopHighRateSpeedPlay();
        }

        public void SetPosition(double position)
        {
            Player.SetPosition(position);
        }

        public void PositionBack(double progress = 3)
        {
            Player.PositionBack(progress);
        }

        public void PositionForward(double progress = 3)
        {
            Player.PositionForward(progress);
        }

        public void ToggleMiniWindows()
        {
            Player.ToggleMiniWindows();
        }

        public void ToggleFullWindow()
        {
            Player.ToggleFullWindow();
        }

        public void ToggleFullscreen()
        {
            Player.ToggleFullscreen();
        }

        public async Task CaptureVideo()
        {
            await Player.CaptureVideo();
        }

        public void ToggleDanmakuDisplay()
        {
            Player.ToggleDanmakuDisplay();
        }

        public void ToggleSubtitle()
        {
            Player.ToggleSubtitle();
        }

        public void ToggleVideoEnable()
        {
            Player.ToggleVideoEnable();
        }

        public void AddVolume()
        {
            Player.AddVolume();
        }

        public void MinusVolume()
        {
            Player.MinusVolume();
        }

        public void CancelFullscreenOrFullWindow()
        {
            Player.CancelFullscreenOrFullWindow();
        }

        public void DisposePlayer()
        {
            Player?.Dispose();
        }

        public void Seek(double position)
        {
            Player.SetPosition(position);
        }

        public async Task ReportHistory()
        {
            if (Player == null) return;
            await Player.ReportHistory();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back || e.SourcePageType == typeof(BlankPage))
            {
                // (this.Content as Grid).Children.Clear();
                //  GC.Collect();
            }
            base.OnNavigatingFrom(e);
        }
    }

    public interface IPlayPage
    {
        public bool IsPlaying { get; }

        public void Pause();

        public void Play();

        public void AddVolume();

        public void MinusVolume();

        public void CancelFullscreenOrFullWindow();

        public Task CaptureVideo();

        public void ToggleDanmakuDisplay();

        public void ToggleFullscreen();

        public void ToggleFullWindow();

        public void ToggleMiniWindows();

        public void ToggleSubtitle();
    }
}
