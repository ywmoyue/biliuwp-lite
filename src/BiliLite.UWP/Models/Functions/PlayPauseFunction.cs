using System.Threading.Tasks;
using BiliLite.Extensions;
using BiliLite.Pages;

namespace BiliLite.Models.Functions
{
    public class PlayPauseFunction : BaseShortcutFunction
    {
        public override string Name { get; } = "播放/暂停";

        public override async Task Action(object param)
        {
            if (!(param is IPlayPage page)) return;
            if (ControlsExtensions.CheckFocusTextBoxNow()) return;
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
