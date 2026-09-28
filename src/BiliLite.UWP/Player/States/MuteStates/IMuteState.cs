using System.Threading.Tasks;
using BiliLite.Player.Controllers;
using BiliLite.Services;

namespace BiliLite.Player.States.MuteStates
{
    public abstract class IMuteState
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        protected readonly BasePlayerController m_playerController;
        protected readonly MuteStateHandler m_stateHandler;

        protected IMuteState(BasePlayerController playerController, MuteStateHandler stateHandler)
        {
            m_playerController = playerController;
            m_stateHandler = stateHandler;
        }

        public virtual bool IsMuted => false;

        public virtual async Task Mute()
        {
            _logger.Error("静音状态错误调用");
        }

        public virtual async Task UnMute()
        {
            _logger.Error("静音状态错误调用");
        }
    }
}
