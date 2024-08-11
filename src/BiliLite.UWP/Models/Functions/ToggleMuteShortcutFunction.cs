using BiliLite.Extensions;
using BiliLite.Pages;
using System.Threading.Tasks;

namespace BiliLite.Models.Functions
{
    public class ToggleMuteShortcutFunction : BaseShortcutFunction
    {
        public override string Name { get; } = "静音/满音量";

        public override async Task Action(object param)
        {
            if (!(param is PlayPage page)) return;
            if (ControlsExtensions.CheckFocusTextBoxNow()) return;
            page.ToggleMute();
        }
    }
}
