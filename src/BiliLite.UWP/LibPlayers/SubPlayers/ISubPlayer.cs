using System;
using System.Threading.Tasks;
using BiliLite.Models.Common.Player;
using BiliLite.Models.Exceptions;
using BiliLite.Player.MediaInfos;
using static BiliLite.Models.Common.Player.PlayerError;

namespace BiliLite.Player.SubPlayers
{
    public abstract class ISubPlayer
    {
        protected RealPlayInfo m_realPlayInfo;

        public abstract double Volume { get; set; }

        public abstract double Position { get; set; }

        public abstract double Duration { get; }

        public event EventHandler<PlayerException> PlayerErrorOccurred;

        public virtual event EventHandler MediaOpened;

        public virtual event EventHandler MediaEnded;

        public virtual event EventHandler BufferingEnded;

        public virtual event EventHandler<double> PositionChanged;

        protected void EmitError(PlayerErrorCode errorCode, string description, RetryStrategy retryStrategy = RetryStrategy.NoRetry)
        {
            var error = PlayerException.Create(errorCode, description, retryStrategy);
            PlayerErrorOccurred?.Invoke(this, error);
        }

        public void SetRealPlayInfo(RealPlayInfo realPlayInfo)
        {
            m_realPlayInfo = realPlayInfo;
        }

        public abstract CollectInfo GetCollectInfo();

        public abstract Task Load();

        public abstract Task Buff();

        public abstract Task Play();

        public abstract Task Stop();

        public abstract Task Fault();

        public abstract Task Pause();

        public abstract Task Resume();
    }
}
