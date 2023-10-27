using System;
using BiliLite.Player.Controllers;
using BiliLite.Services;

namespace BiliLite.Player.States.ScreenStates
{
    public class ScreenStateHandler
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly BasePlayerController m_playerController;
        private readonly FullscreenState m_fullscreenState;
        private readonly NormalScreenState m_normalScreenState;

        public ScreenStateHandler(BasePlayerController playerController)
        {
            m_playerController = playerController;
            m_fullscreenState = new FullscreenState(m_playerController, this);
            m_normalScreenState = new NormalScreenState(m_playerController, this);
            m_playerController.ScreenState = m_normalScreenState;
        }

        public event EventHandler<ScreenStateChangedEventArgs> ScreenStateChanged;

        private void ChangeToStateCore(IScreenState newState)
        {
            if (m_playerController.ScreenState == newState)
            {
                return;
            }

            var oldState = m_playerController.ScreenState;
            m_playerController.ScreenState = newState;
            _logger.Debug($"全屏状态变更：{oldState.GetType().Name} => {newState.GetType().Name}");
            ScreenStateChanged?.Invoke(this, new ScreenStateChangedEventArgs(newState, oldState));
        }

        public void ChangeToFullscreen()
        {
            ChangeToStateCore(m_fullscreenState);
        }

        public void ChangeToNormalScreen()
        {
            ChangeToStateCore(m_normalScreenState);
        }
    }
}
