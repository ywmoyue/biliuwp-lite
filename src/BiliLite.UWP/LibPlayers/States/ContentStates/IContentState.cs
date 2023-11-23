using System.Threading.Tasks;
using BiliLite.Player.Controllers;
using BiliLite.Services;

namespace BiliLite.Player.States.ContentStates
{
    public class IContentState
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        protected readonly BasePlayerController m_playerController;
        protected readonly ContentStateHandler m_stateHandler;

        public IContentState(BasePlayerController playerController, ContentStateHandler stateHandler)
        {
            m_playerController = playerController;
            m_stateHandler = stateHandler;
        }

        public virtual bool IsFullWindow => false;

        public virtual async Task FullWindow()
        {
            _logger.Error($"铺满状态错误调用");
        }

        public virtual async Task CancelFullWindow()
        {
            _logger.Error($"铺满状态错误调用");
        }
    }
}
