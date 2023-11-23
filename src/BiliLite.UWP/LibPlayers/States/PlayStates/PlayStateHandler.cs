using System;
using BiliLite.Player.Controllers;
using BiliLite.Services;

namespace BiliLite.Player.States.PlayStates
{
    public class PlayStateHandler
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly BasePlayerController m_playerController;
        private readonly IdlePlayState m_idlePlayState;
        private readonly LoadingPlayState m_loadingPlayState;
        private readonly BufferingPlayState m_bufferingPlayState;
        private readonly PlayingPlayState m_playingPlayState;
        private readonly StopPlayState m_stopPlayState;
        private readonly FaultPlayState m_faultPlayState;

        public PlayStateHandler(BasePlayerController playerController)
        {
            m_playerController = playerController;
            m_idlePlayState = new IdlePlayState(m_playerController, this);
            m_loadingPlayState = new LoadingPlayState(m_playerController, this);
            m_bufferingPlayState = new BufferingPlayState(m_playerController, this);
            m_playingPlayState = new PlayingPlayState(m_playerController, this);
            m_stopPlayState = new StopPlayState(m_playerController, this);
            m_faultPlayState = new FaultPlayState(m_playerController, this);
            m_playerController.PlayState = m_idlePlayState;
        }

        public event EventHandler<PlayStateChangedEventArgs> PlayStateChanged;

        private void ChangeToStateCore(IPlayState newState)
        {
            if (m_playerController.PlayState == newState)
            {
                return;
            }

            var oldState = m_playerController.PlayState;
            m_playerController.PlayState = newState;
            _logger.Debug($"播放状态变更：{oldState.GetType().Name} => {newState.GetType().Name}");
            PlayStateChanged?.Invoke(this, new PlayStateChangedEventArgs(newState, oldState));
        }

        public void ChangeToLoading()
        {
            ChangeToStateCore(m_loadingPlayState);
        }

        public void ChangeToBuffering()
        {
            ChangeToStateCore(m_bufferingPlayState);
        }

        public void ChangeToPlaying()
        {
            ChangeToStateCore(m_playingPlayState);
        }

        public void ChangeToStoppd()
        {
            ChangeToStateCore(m_stopPlayState);
        }

        public void ChangeToFault()
        {
            ChangeToStateCore(m_faultPlayState);
        }
    }
}
