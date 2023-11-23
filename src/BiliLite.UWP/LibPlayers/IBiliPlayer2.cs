using System.Threading.Tasks;
using BiliLite.Player.MediaInfos;

namespace BiliLite.Player
{
    public interface IBiliPlayer2
    {
        public double Volume { get; set; }

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
