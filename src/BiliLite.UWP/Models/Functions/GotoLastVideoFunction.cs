using BiliLite.Extensions;
using BiliLite.Pages;
using System.Threading.Tasks;

namespace BiliLite.Models.Functions
{
    public class GotoLastVideoFunction : BaseShortcutFunction
    {
        public override string Name { get; } = "上一P";

        public override async Task Action(object param)
        {
            if (!(param is PlayPage page)) return;
            if (ControlsExtensions.CheckFocusTextBoxNow()) return;
            page.GotoLastVideo();
        }
    }
}
