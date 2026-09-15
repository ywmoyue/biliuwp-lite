using System.Threading.Tasks;
using BiliLite.Player.MediaInfos;

namespace BiliLite.Player
{
    public interface IBiliPlayer2
    {
        public double Volume { get; set; }

        public bool IsMuted { get; set; }

        public bool IsBuffering { get; }

        public double BufferCache { get; }

        public double Position { get; }

        public event System.EventHandler<double> PositionChanged;

        public Task SetRate(double value);

        public Task SetMuted(bool muted);

        public Task SetPosition(double value);

        public Task SetRatioMode(int mode);

        public Task SetVideoEnable(bool enable);

        public Task<byte[]> CaptureAsync();

        public Task Load();

        public Task Buff();

        public Task Play();

        public Task Stop();

        public Task Fault();

        public Task Pause();

        public Task Resume();

        public Task FullWindow();

        public Task CancelFullWindow();

        public Task Fullscreen();

        public Task CancelFullscreen();

        public CollectInfo GetCollectInfo();
    }
}
