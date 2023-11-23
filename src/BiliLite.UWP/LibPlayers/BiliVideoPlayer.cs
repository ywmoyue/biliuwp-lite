using System;
using System.Threading.Tasks;
using BiliLite.Models.Common.Player;
using BiliLite.Player.Controllers;
using BiliLite.Player.MediaInfos;
using BiliLite.Player.SubPlayers;
using Windows.UI.Xaml.Controls;
using BiliLite.Extensions;
using BiliLite.Models.Exceptions;
using BiliLite.Services;

namespace BiliLite.Player
{
    public class BiliVideoPlayer : IBiliPlayer2
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private ISubPlayer m_subPlayer;
        private RealPlayInfo m_realPlayInfo;
        private BasePlayerController m_playerController;
        private PlayerConfig m_playerConfig;

        public BiliVideoPlayer(PlayerConfig playerConfig, MediaPlayerElement videoElement, MediaPlayerElement audioElement, BasePlayerController playerController)
        {
            m_playerConfig = playerConfig;
            m_playerController = playerController;
            m_subPlayer = new DashFFmpegPlayer(playerConfig, videoElement, audioElement);
            InitPlayerEvents(m_subPlayer);
        }

        public event EventHandler<PlayerException> ErrorOccurred;
        public event EventHandler BufferingEnded;

        public double Volume { get; set; }

        private void InitPlayerEvents(ISubPlayer subPlayer)
        {
            subPlayer.PlayerErrorOccurred += SubPlayerOnPlayerErrorOccurred;
            subPlayer.MediaOpened += SubPlayer_MediaOpened;
            subPlayer.MediaEnded += SubPlayer_MediaEnded;
            subPlayer.BufferingEnded += SubPlayer_BufferingEnded;
        }

        private async void SubPlayer_BufferingEnded(object sender, EventArgs e)
        {
            await m_playerController.PlayState.Play();
        }

        private async void SubPlayerOnPlayerErrorOccurred(object sender, PlayerException e)
        {
            if (e.RetryStrategy == PlayerError.RetryStrategy.NoError)
            {
                await m_playerController.PlayState.Stop();
                return;
            }
            await m_playerController.PlayState.Fault();
            Notify.ShowMessageToast($"播放失败: {e.Description}");
            _logger.Error($"播放失败: {e.Description}");
            if (e.RetryStrategy == PlayerError.RetryStrategy.Normal)
            {
                await m_playerController.PlayState.Stop();
                await m_playerController.PlayState.Load();
            }
            ErrorOccurred?.Invoke(this, e);
        }

        private async void SubPlayer_MediaOpened(object sender, EventArgs e)
        {
            await m_playerController.PlayState.Buff();
            if (m_realPlayInfo.IsAutoPlay)
            {
                await m_playerController.PauseState.Resume();
            }
        }

        private async void SubPlayer_MediaEnded(object sender, EventArgs e)
        {
            await m_playerController.PlayState.Stop();
        }

        public void SetRealPlayInfo(RealPlayInfo realPlayInfo)
        {
            m_realPlayInfo = realPlayInfo;
            m_subPlayer.SetRealPlayInfo(realPlayInfo);
        }

        public async Task Load()
        {
            await m_subPlayer.Load();
        }

        public async Task Buff()
        {
            await m_subPlayer.Buff();
        }

        public async Task Play()
        {
            await m_subPlayer.Play();
        }

        public async Task Stop()
        {
            await m_subPlayer.Stop();
        }

        public async Task Fault()
        {
            await m_subPlayer.Fault();
        }

        public async Task Pause()
        {
            await m_subPlayer.Pause();
        }

        public async Task Resume()
        {
            await m_subPlayer.Resume();
        }

        public async Task FullWindow()
        {
        }

        public async Task CancelFullWindow()
        {
        }

        public async Task Fullscreen()
        {
        }

        public async Task CancelFullscreen()
        {
        }

        public CollectInfo GetCollectInfo()
        {
            return m_subPlayer.GetCollectInfo();
        }
    }
}
