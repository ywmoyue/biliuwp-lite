using BiliLite.Pages;
using System.Threading.Tasks;

namespace BiliLite.Models.Functions
{
    public class ToggleMiniWindowShortcutFunction : BaseShortcutFunction
    {
        public override string Name { get; } = "切换小窗播放";

        public override async Task Action(object param)
        {
            if (!(param is PlayPage page)) return;
            page.ToggleMiniWindows();
        }
    }
}
