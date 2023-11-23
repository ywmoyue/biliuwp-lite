using System.Threading.Tasks;
using BiliLite.Player.Controllers;
using BiliLite.Services;

namespace BiliLite.Player.States.PauseStates
{
    public abstract class IPauseState
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        protected readonly BasePlayerController m_playerController;
        protected readonly PauseStateHandler m_stateHandler;

        public IPauseState(BasePlayerController playerController, PauseStateHandler stateHandler)
        {
            m_playerController = playerController;
            m_stateHandler = stateHandler;
        }

        public virtual bool IsPaused => true;

        public virtual async Task Resume()
        {
            _logger.Error($"暂停状态错误调用");
        }

        public virtual async Task Pause()
        {
            _logger.Error($"暂停状态错误调用");
        }
    }
}
