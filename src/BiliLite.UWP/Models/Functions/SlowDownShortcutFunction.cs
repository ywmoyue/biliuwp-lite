using BiliLite.Extensions;
using BiliLite.Pages;
using System.Threading.Tasks;

namespace BiliLite.Models.Functions
{
    public class SlowDownShortcutFunction : BaseShortcutFunction
    {
        public override string Name { get; } = "降低播放速度";

        public override async Task Action(object param)
        {
            if (!(param is PlayPage page)) return;
            if (ControlsExtensions.CheckFocusTextBoxNow()) return;
            page.SlowDown();
        }
    }
}
