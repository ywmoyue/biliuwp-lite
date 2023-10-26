using System.Threading.Tasks;
using BiliLite.Player.Controllers;
using BiliLite.Services;

namespace BiliLite.Player.States.ScreenStates
{
    public class IScreenState
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        protected readonly BasePlayerController m_playerController;
        protected readonly ScreenStateHandler m_stateHandler;

        public IScreenState(BasePlayerController playerController, ScreenStateHandler stateHandler)
        {
            m_playerController = playerController;
            m_stateHandler = stateHandler;
        }

        public virtual bool IsFullscreen => false;

        public virtual async Task Fullscreen()
        {
            _logger.Error($"全屏状态错误调用");
        }

        public virtual async Task CancelFullscreen()
        {
            _logger.Error($"全屏状态错误调用");
        }
    }
}
