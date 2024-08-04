using BiliLite.Pages;
using System.Threading.Tasks;

namespace BiliLite.Models.Functions
{
    public class CaptureVideoShortcutFunction : BaseShortcutFunction
    {
        public override string Name { get; } = "截图";

        public override async Task Action(object param)
        {
            if (!(param is PlayPage page)) return;
            await page.CaptureVideo();
        }
    }
}
