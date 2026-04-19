using System;
using System.Threading.Tasks;
using BiliLite.Models.Common.Player;
using BiliLite.Models.Exceptions;
using BiliLite.Player.MediaInfos;
using Windows.UI.Xaml;
using static BiliLite.Models.Common.Player.PlayerError;

namespace BiliLite.Player.SubPlayers
{
    public abstract class ISubPlayer
    {
        protected RealPlayInfo m_realPlayInfo;
        protected double m_lastVolumeBeforeMuted = 1.0;

        public abstract RealPlayerType Type { get; }

        public abstract double Volume { get; set; }

        public virtual bool IsMuted { get; set; }

        public virtual bool IsBuffering => false;

        public virtual double BufferCache => 0;

        public abstract double Position { get; }

        public abstract FrameworkElement PlayerView { get; }

        public virtual double Duration => m_realPlayInfo?.TotalDuration ?? 0;

        protected double m_rate = 1.0;

        public event EventHandler<PlayerException> PlayerErrorOccurred;

        public virtual event EventHandler MediaOpened;

        public virtual event EventHandler MediaEnded;

        public virtual event EventHandler BufferingStarted;

        public virtual event EventHandler BufferingEnded;

        public virtual event EventHandler<double> BufferCacheChanged;

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

        public abstract Task SetRate(double value);

        public virtual async Task SetMuted(bool muted)
        {
            if (muted && !IsMuted)
            {
                m_lastVolumeBeforeMuted = Volume;
                Volume = 0;
            }
            else if (!muted && IsMuted)
            {
                if (Volume <= 0)
                {
                    Volume = m_lastVolumeBeforeMuted <= 0 ? 1.0 : m_lastVolumeBeforeMuted;
                }
            }

            IsMuted = muted;
        }

        public abstract Task SetPosition(double value);

        public virtual async Task SetRatioMode(int mode)
        {
        }

        public virtual async Task SetVideoEnable(bool enable)
        {
        }

        public virtual async Task<byte[]> CaptureAsync()
        {
            return null;
        }

        protected void EmitPositionChanged(double position)
        {
            PositionChanged?.Invoke(this, position);
        }

        protected void EmitBufferCacheChanged(double progress)
        {
            BufferCacheChanged?.Invoke(this, progress);
        }
    }
}
