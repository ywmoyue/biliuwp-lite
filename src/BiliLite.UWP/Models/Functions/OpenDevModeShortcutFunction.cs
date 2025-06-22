using BiliLite.Extensions;
using BiliLite.Pages;
using System.Threading.Tasks;

namespace BiliLite.Models.Functions
{
    public class OpenDevModeShortcutFunction : BaseShortcutFunction
    {
        public override string Name { get; } = "打开播放器开发者模式";

        public override async Task Action(object param)
        {
            if (!(param is PlayPage page)) return;
            if (ControlsExtensions.CheckFocusTextBoxNow()) return;
            page.OpenDevMode();
        }
    }
}
