using System.Threading.Tasks;
using BiliLite.Pages;

namespace BiliLite.Models.Functions
{
    public class PlayPauseFunction : IShortcutFunction
    {
        public string Name { get; } = "播放/暂停";

        public async Task Action(object param)
        {
            if (!(param is PlayPage page)) return;
            if (page.IsPlaying)
            {
                page.Pause();
            }
            else
            {
                page.Play();
            }
        }
    }
}
