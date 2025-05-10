using System;
using System.Collections.Generic;
using BiliLite.Models.Common.Player;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using BiliLite.Extensions;
using BiliLite.Models.Exceptions;
using BiliLite.Player.Controllers;
using BiliLite.Player.SubPlayers;
using BiliLite.Services;
using BiliLite.Player.MediaInfos;
using BiliLite.Player.WebPlayer;

namespace BiliLite.Player
{
    public class LivePlayer : IBiliPlayer2
    {
        private RealPlayInfo m_realPlayInfo;
        private ISubPlayer m_subPlayer;
        private BasePlayerController m_playerController;
        private PlayerConfig m_playerConfig;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private List<RealPlayerType> m_triedPlayers = new();

        public LivePlayer(PlayerConfig playerConfig, MediaPlayerElement playerElement,
            BasePlayerController playerController, ShakaPlayerControl shakaPlayerControl,
            MpegtsPlayerControl mpegtsPlayerControl)
        {
            m_playerConfig = playerConfig;
            m_playerController = playerController;
            if (playerConfig.PlayerType == RealPlayerType.FFmpegInterop)
            {
                m_subPlayer = new LiveHlsPlayer(playerConfig, playerElement);
            }
            else if (playerConfig.PlayerType == RealPlayerType.ShakaPlayer)
            {
                m_subPlayer = new LiveShakaPlayer(playerConfig, shakaPlayerControl);
            }
            else if (playerConfig.PlayerType == RealPlayerType.Mpegts)
            {
                m_subPlayer = new LiveMpegtsPlayer(playerConfig, mpegtsPlayerControl);
            }

            InitPlayerEvents(m_subPlayer);
        }

        public BaseWebPlayer WebPlayer
        {
            get
            {
                if (m_subPlayer is ISubWebPlayer subWebPlayer)
                {
                    return subWebPlayer.WebPlayer;
                }

                return null;
            }
        }

        public double Volume
        {
            get => m_subPlayer.Volume;
            set => m_subPlayer.Volume = value;
        }

        public event EventHandler<PlayerException> ErrorOccurred;
        public event EventHandler BufferingEnded;
        public event EventHandler<RealPlayerType> NeedReplacePlayer;

        private void InitPlayerEvents(ISubPlayer subPlayer)
        {
            subPlayer.PlayerErrorOccurred += SubPlayerOnPlayerErrorOccurred;
            subPlayer.MediaOpened += SubPlayer_MediaOpened;
            subPlayer.MediaEnded += SubPlayer_MediaEnded;
            subPlayer.BufferingStarted += SubPlayer_BufferingStarted;
            subPlayer.BufferingEnded += SubPlayer_BufferingEnded;
        }

        private void UnLoadPlayerEvents(ISubPlayer subPlayer)
        {
            subPlayer.PlayerErrorOccurred -= SubPlayerOnPlayerErrorOccurred;
            subPlayer.MediaOpened -= SubPlayer_MediaOpened;
            subPlayer.MediaEnded -= SubPlayer_MediaEnded;
            subPlayer.BufferingStarted -= SubPlayer_BufferingStarted;
            subPlayer.BufferingEnded -= SubPlayer_BufferingEnded;
        }

        private async void SubPlayer_BufferingStarted(object sender, EventArgs e)
        {
            await m_playerController.PlayState.Buff();
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

            if (e.Code == PlayerError.PlayerErrorCode.NeedUseOtherPlayerError)
            {
                m_triedPlayers.Add(m_subPlayer.Type);

                if (!m_triedPlayers.Contains(RealPlayerType.ShakaPlayer))
                {
                    NeedReplacePlayer?.Invoke(this, RealPlayerType.ShakaPlayer);
                    return;
                }
                else if (!m_triedPlayers.Contains(RealPlayerType.Mpegts))
                {
                    NeedReplacePlayer?.Invoke(this, RealPlayerType.Mpegts);
                    return;
                }
                else if (!m_triedPlayers.Contains(RealPlayerType.FFmpegInterop))
                {
                    NeedReplacePlayer?.Invoke(this, RealPlayerType.FFmpegInterop);
                    return;
                }
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

        public CollectInfo GetCollectInfo()
        {
            return m_subPlayer.GetCollectInfo();
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

        public async Task UnLoad()
        {
            UnLoadPlayerEvents(m_subPlayer);
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
    }
}
