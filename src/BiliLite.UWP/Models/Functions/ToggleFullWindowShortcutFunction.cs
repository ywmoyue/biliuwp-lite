using BiliLite.Extensions;
using BiliLite.Pages;
using System.Threading.Tasks;

namespace BiliLite.Models.Functions
{
    public class ToggleFullWindowShortcutFunction : BaseShortcutFunction
    {
        public override string Name { get; } = "切换铺满窗口";

        public override async Task Action(object param)
        {
            if (!(param is IPlayPage page)) return;
            if (ControlsExtensions.CheckFocusTextBoxNow()) return;
            page.ToggleFullWindow();
        }
    }
}
