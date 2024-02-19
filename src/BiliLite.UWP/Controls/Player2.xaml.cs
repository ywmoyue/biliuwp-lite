using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using BiliLite.LibPlayers.MediaInfos;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Player;
using BiliLite.Models.Common.Video.PlayUrlInfos;
using BiliLite.Models.Exceptions;
using BiliLite.Player;
using BiliLite.Player.Controllers;
using BiliLite.Player.States.ContentStates;
using BiliLite.Player.States.PauseStates;
using BiliLite.Player.States.PlayStates;
using BiliLite.Player.States.ScreenStates;
using BiliLite.Services;
using BiliLite.ViewModels.Player;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Media;
using BiliLite.Extensions;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板


namespace BiliLite.Controls
{
    public sealed partial class Player2 : UserControl, IPlayer
    {
        private readonly PlayerConfig m_playerConfig;
        private readonly BasePlayerController m_playerController;
        private readonly BiliVideoPlayer m_player;
        private readonly RealPlayInfo m_realPlayInfo;

        public Player2()
        {
            ViewModel = App.ServiceProvider.GetRequiredService<PlayerViewModel>();
            ViewModel.SetPlayer(this);
            this.InitializeComponent();

            m_playerConfig = new PlayerConfig();
            //PreLoadSetting();
            m_playerController = PlayerControllerFactory.Create(PlayerType.Live);
            m_player = new BiliVideoPlayer(m_playerConfig, mediaPlayerVideo, mediaPlayerAudio, m_playerController);
            m_realPlayInfo = new RealPlayInfo();
            m_realPlayInfo.IsAutoPlay = SettingService.GetValue(SettingConstants.Player.AUTO_PLAY, false);
            m_playerController.SetPlayer(m_player);
            m_player.SetRealPlayInfo(m_realPlayInfo);
            InitPlayerEvent();
        }

        public PlayerViewModel ViewModel { get; private set; }

        public PlayState PlayState { get; set; }

        public PlayMediaType PlayMediaType { get; set; }

        public VideoPlayHistoryHelper.ABPlayHistoryEntry ABPlay { get; set; }

        [Obsolete]
        public double Position { get; set; }

        [Obsolete]
        public double Duration { get; set; }

        public double Volume { get; set; }

        public bool Buffering { get; set; }

        public double BufferCache { get; set; }

        [Obsolete]
        public double Rate { get; set; }

        public string MediaInfo { get; set; }

        public bool Opening { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<PlayState> PlayStateChanged;
        public event EventHandler PlayMediaOpened;
        public event EventHandler PlayMediaEnded;
        public event EventHandler<string> PlayMediaError;
        public event EventHandler<ChangePlayerEngine> ChangeEngine;

        private void InitPlayerEvent()
        {
            m_playerController.PlayStateChanged += PlayerController_PlayStateChanged;
            m_playerController.PauseStateChanged += PlayerController_PauseStateChanged;
            m_playerController.ContentStateChanged += PlayerController_ContentStateChanged;
            m_playerController.ScreenStateChanged += PlayerController_ScreenStateChanged;
            m_player.ErrorOccurred += Player_ErrorOccurred;
            m_playerController.MediaInfosUpdated += PlayerController_MediaInfosUpdated;
            m_player.PositionChanged += Player_PositionChanged;
        }

        private void Player_PositionChanged(object sender, double e)
        {
            ViewModel.SourcePosition = e;
        }

        private async void PlayerController_MediaInfosUpdated(object sender, MediaInfo e)
        {
        }

        private async void PlayerController_ScreenStateChanged(object sender, ScreenStateChangedEventArgs e)
        {
        }

        private async void PlayerController_ContentStateChanged(object sender, ContentStateChangedEventArgs e)
        {
        }

        private async void PlayerController_PauseStateChanged(object sender, PauseStateChangedEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                PlayState = e.NewState.IsPaused ? PlayState.Pause : PlayState.Playing;
                PlayStateChanged?.Invoke(this, PlayState);
            });
        }

        private async void Player_ErrorOccurred(object sender, PlayerException e)
        {
        }

        private async void PlayerController_PlayStateChanged(object sender, PlayStateChangedEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (e.NewState.IsPlaying)
                {
                    ViewModel.Duration = m_player.Duration;
                    PlayState = PlayState.Playing;
                    PlayStateChanged?.Invoke(this, PlayState);
                }
                if (e.NewState.IsStopped)
                {
                    PlayState = PlayState.End;
                    PlayStateChanged?.Invoke(this, PlayState);
                }
            });
        }

        #region 弃用

        public async Task<PlayerOpenResult> PlayerDashUseNative(BiliDashPlayUrlInfo dashInfo, string userAgent, string referer, double positon = 0)
        {
            m_realPlayInfo.PlayUrls.DashVideoUrl = dashInfo.Video.Url;
            m_realPlayInfo.PlayUrls.DashAudioUrl = dashInfo.Audio.Url;
            m_playerConfig.UserAgent = userAgent;
            m_playerConfig.Referer = referer;
            await m_playerController.PlayState.Load();
            return new PlayerOpenResult()
            {
                result = true
            };
        }

        public Task<PlayerOpenResult> PlayerSingleMp4UseNativeAsync(string url, double positon = 0, bool needConfig = true, bool isLocal = false)
        {
            throw new NotImplementedException();
        }

        public Task<PlayerOpenResult> PlayDashUseFFmpegInterop(BiliDashPlayUrlInfo dashPlayUrlInfo, string userAgent, string referer, double positon = 0,
            bool needConfig = true, bool isLocal = false)
        {
            throw new NotImplementedException();
        }

        public Task<PlayerOpenResult> PlayDashUrlUseFFmpegInterop(string url, string userAgent, string referer, double positon = 0,
            bool needConfig = true)
        {
            throw new NotImplementedException();
        }

        public Task<PlayerOpenResult> PlaySingleFlvUseFFmpegInterop(string url, string userAgent, string referer, double positon = 0,
            bool needConfig = true)
        {
            throw new NotImplementedException();
        }

        public Task<PlayerOpenResult> PlaySingleFlvUseSYEngine(string url, string userAgent, string referer, double positon = 0, bool needConfig = true,
            string epId = "")
        {
            throw new NotImplementedException();
        }

        public Task<PlayerOpenResult> PlayVideoUseSYEngine(List<BiliFlvPlayUrlInfo> urls, string userAgent, string referer, double positon = 0, bool needConfig = true,
            string epId = "", bool isLocal = false)
        {
            throw new NotImplementedException();
        }

        #endregion

        public void SetRatioMode(PlayerRatioMode mode)
        {
            this.SetRatioModeCore(mode, mediaPlayerVideo, m_realPlayInfo);
        }

        public void SetPosition(double position)
        {
            m_player.Position = position;
        }

        public async Task Load(BasePlayInfo basePlayInfo)
        {
            switch (basePlayInfo.PlayMode)
            {
                case BiliVideoPlayMode.Dash:
                    var playInfo = basePlayInfo as BaseDashPlayInfo;
                    m_realPlayInfo.PlayUrls.DashVideoUrl = playInfo.DashInfo.Video.Url;
                    m_realPlayInfo.PlayUrls.DashAudioUrl = playInfo.DashInfo.Audio.Url;
                    m_playerConfig.UserAgent = playInfo.UserAgent;
                    m_playerConfig.Referer = playInfo.Referer;
                    await m_playerController.PlayState.Load();
                    break;
            }
        }

        public async Task Pause()
        {
            await m_playerController.PauseState.Pause();
        }

        public async Task Play()
        {
            await m_playerController.PauseState.Resume();
        }

        public async Task SetRate(double value)
        {
            await m_player.SetRate(value);
        }

        public async Task ClosePlay()
        {
            await m_playerController.PlayState.Stop();
        }

        public void SetVolume(double volume)
        {
            m_player.Volume = volume;
            ViewModel.SourceVolume = volume;
        }

        public string GetMediaInfo()
        {
            return "";
        }

        public void Dispose()
        {
            ClosePlay();
        }
    }
}
