using BiliLite.Pages;
using System.Threading.Tasks;

namespace BiliLite.Models.Functions
{
    public class GotoNextVideoFunction : BaseShortcutFunction
    {
        public override string Name { get; } = "下一P";

        public override async Task Action(object param)
        {
            if (!(param is PlayPage page)) return;
            page.GotoNextVideo();
        }
    }
}
