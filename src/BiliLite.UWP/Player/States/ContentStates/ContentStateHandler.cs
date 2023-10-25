using System;
using BiliLite.Player.Controllers;
using BiliLite.Services;

namespace BiliLite.Player.States.ContentStates
{
    public class ContentStateHandler
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly BasePlayerController m_playerController;
        private readonly FullWindowContentState m_fullWindowContentState;
        private readonly NormalWindowContentState m_normalWindowContentState;

        public ContentStateHandler(BasePlayerController playerController)
        {
            m_playerController = playerController;
            m_fullWindowContentState = new FullWindowContentState(m_playerController, this);
            m_normalWindowContentState = new NormalWindowContentState(m_playerController, this);
            m_playerController.ContentState = m_normalWindowContentState;
        }

        public event EventHandler<ContentStateChangedEventArgs> ContentStateChanged;

        private void ChangeToStateCore(IContentState newState)
        {
            if (m_playerController.ContentState == newState)
            {
                return;
            }

            var oldState = m_playerController.ContentState;
            m_playerController.ContentState = newState;
            _logger.Debug($"铺满状态变更：{oldState.GetType().Name} => {newState.GetType().Name}");
            ContentStateChanged?.Invoke(this, new ContentStateChangedEventArgs(newState, oldState));
        }

        public void ChangeToFullWindow()
        {
            ChangeToStateCore(m_fullWindowContentState);
        }

        public void ChangeToNormalWindow()
        {
            ChangeToStateCore(m_normalWindowContentState);
        }
    }
}
