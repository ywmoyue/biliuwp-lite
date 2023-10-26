using System.Threading.Tasks;
using BiliLite.Player.Controllers;
using BiliLite.Services;

namespace BiliLite.Player.States.PlayStates
{
    public abstract class IPlayState
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        protected readonly BasePlayerController m_playerController;
        protected readonly PlayStateHandler m_stateHandler;

        public IPlayState(BasePlayerController playerController,PlayStateHandler stateHandler)
        {
            m_playerController = playerController;
            m_stateHandler = stateHandler;
        }

        public virtual bool IsPlaying => false;

        public virtual bool IsIdle => false;

        public virtual bool IsLoading => false;

        public virtual bool IsBuffering => false;

        public virtual bool IsStopped => false;

        public virtual bool IsFault => false;

        public virtual async Task Load()
        {
            _logger.Error($"播放状态错误调用");
        }

        public virtual async Task Buff()
        {
            _logger.Error($"播放状态错误调用");
        }

        public virtual async Task Play()
        {
            _logger.Error("播放状态错误调用");
        }

        public virtual async Task Stop()
        {
            _logger.Error("播放状态错误调用");
        }

        public virtual async Task Fault()
        {
            _logger.Error("播放状态错误调用");
        }
    }
}
