using BiliLite.Extensions;
using BiliLite.Pages;
using System.Threading.Tasks;

namespace BiliLite.Models.Functions
{
    public class ToggleDanmakuDisplayShortcutFunction : BaseShortcutFunction
    {
        public override string Name { get; } = "打开/关闭弹幕";

        public override async Task Action(object param)
        {
            if (!(param is IPlayPage page)) return;
            if (ControlsExtensions.CheckFocusTextBoxNow()) return;
            page.ToggleDanmakuDisplay();
        }
    }
}
