using BiliLite.Extensions;
using BiliLite.Pages;
using System.Threading.Tasks;

namespace BiliLite.Models.Functions
{
    public class CaptureVideoShortcutFunction : BaseShortcutFunction
    {
        public override string Name { get; } = "截图";

        public override async Task Action(object param)
        {
            if (!(param is IPlayPage page)) return;
            if (ControlsExtensions.CheckFocusTextBoxNow()) return;
            await page.CaptureVideo();
        }
    }
}
