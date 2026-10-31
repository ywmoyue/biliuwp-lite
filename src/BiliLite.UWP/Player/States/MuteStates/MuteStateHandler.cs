using System;
using BiliLite.Player.Controllers;
using BiliLite.Services;

namespace BiliLite.Player.States.MuteStates
{
    public class MuteStateHandler
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly BasePlayerController m_playerController;
        private readonly MutedState m_mutedState;
        private readonly UnMutedState m_unMutedState;

        public MuteStateHandler(BasePlayerController playerController)
        {
            m_playerController = playerController;
            m_mutedState = new MutedState(m_playerController, this);
            m_unMutedState = new UnMutedState(m_playerController, this);
            m_playerController.MuteState = m_unMutedState;
        }

        public event EventHandler<MuteStateChangedEventArgs> MuteStateChanged;

        private void ChangeToStateCore(IMuteState newState)
        {
            if (m_playerController.MuteState == newState)
            {
                return;
            }

            var oldState = m_playerController.MuteState;
            m_playerController.MuteState = newState;
            _logger.Debug($"静音状态变更：{oldState.GetType().Name} => {newState.GetType().Name}");
            MuteStateChanged?.Invoke(this, new MuteStateChangedEventArgs(newState, oldState));
        }

        public void ChangeToMuted()
        {
            ChangeToStateCore(m_mutedState);
        }

        public void ChangeToUnMuted()
        {
            ChangeToStateCore(m_unMutedState);
        }
    }
}
