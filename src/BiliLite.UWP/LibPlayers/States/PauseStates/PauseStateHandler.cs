using BiliLite.Player.Controllers;
using BiliLite.Services;
using System;

namespace BiliLite.Player.States.PauseStates
{
    public class PauseStateHandler
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly BasePlayerController m_playerController;
        private readonly PausedState m_pausedState;
        private readonly ResumeState m_resumeState;

        public PauseStateHandler(BasePlayerController playerController)
        {
            m_playerController = playerController;
            m_pausedState = new PausedState(m_playerController, this);
            m_resumeState = new ResumeState(m_playerController, this);
            m_playerController.PauseState = m_resumeState;
        }

        public event EventHandler<PauseStateChangedEventArgs> PauseStateChanged;

        private void ChangeToStateCore(IPauseState newState)
        {
            if (m_playerController.PauseState == newState)
            {
                return;
            }

            var oldState = m_playerController.PauseState;
            m_playerController.PauseState = newState;
            _logger.Debug($"暂停状态变更：{oldState.GetType().Name} => {newState.GetType().Name}");
            PauseStateChanged?.Invoke(this, new PauseStateChangedEventArgs(newState, oldState));
        }

        public void ChangeToPaused()
        {
            ChangeToStateCore(m_pausedState);
        }

        public void ChangeToResume()
        {
            ChangeToStateCore(m_resumeState);
        }
    }
}
