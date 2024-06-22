using System.Threading.Tasks;
using BiliLite.Pages;

namespace BiliLite.Models.Functions
{
    public class CancelFullscreenFunction : IShortcutFunction
    {
        public string Name { get; } = "退出全屏";

        public async Task Action(object param)
        {
            if (!(param is PlayPage page)) return;
            page.CancelFullscreen();
        }
    }
}
