using System;
using BiliLite.Models.Common.Player;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.UI.Xaml.Controls;
using BiliLite.Models.Exceptions;
using BiliLite.Player.Controllers;
using BiliLite.Player.SubPlayers;
using FFmpegInteropX;

namespace BiliLite.Player
{
    public class LivePlayer : IBiliPlayer2
    {
        private RealPlayInfo m_realPlayInfo;
        private ISubPlayer m_subPlayer;
        private BasePlayerController m_playerController;

        public LivePlayer(MediaPlayerElement playerElement, BasePlayerController playerController)
        {
            m_playerController = playerController;
            m_subPlayer = new LiveHlsPlayer(playerElement);
            InitPlayerEvents(m_subPlayer);
        }

        public event EventHandler<PlayerException> ErrorOccurred;
        public event EventHandler BufferingEnded;

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
            await m_playerController.PlayState.Fault();
            ErrorOccurred?.Invoke(this, e);
        }
        
        private async void SubPlayer_MediaOpened(object sender, EventArgs e)
        {
            await m_playerController.PlayState.Buff();
        }

        private async void SubPlayer_MediaEnded(object sender, EventArgs e)
        {
            await m_playerController.PlayState.Stop();
        }

        public void SetRealPlayInfo(RealPlayInfo realPlayInfo)
        {
            m_realPlayInfo = realPlayInfo;
        }

        public async Task Load()
        {
            await m_subPlayer.Load(m_realPlayInfo.HlsUrl);
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

        }

        public async Task Resume()
        {

        }
    }
}
