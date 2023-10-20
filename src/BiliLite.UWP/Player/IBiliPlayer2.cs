using System.Threading.Tasks;

namespace BiliLite.Player
{
    public interface IBiliPlayer2
    {
        public Task Load();

        public Task Buff();

        public Task Play();

        public Task Stop();

        public Task Fault();

        public Task Pause();

        public Task Resume();
    }
}
