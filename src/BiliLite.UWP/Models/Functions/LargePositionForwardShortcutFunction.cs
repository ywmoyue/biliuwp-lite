using BiliLite.Extensions;
using BiliLite.Pages;
using System.Threading.Tasks;

namespace BiliLite.Models.Functions
{
    public class LargePositionForwardShortcutFunction : BaseShortcutFunction
    {
        public override string Name { get; } = "快进90秒";

        public override async Task Action(object param)
        {
            if (!(param is PlayPage page)) return;
            if (ControlsExtensions.CheckFocusTextBoxNow()) return;
            page.PositionForward(90);
        }
    }
}
