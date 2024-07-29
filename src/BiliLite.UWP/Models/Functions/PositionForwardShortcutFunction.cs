using BiliLite.Pages;
using System.Threading.Tasks;

namespace BiliLite.Models.Functions
{
    public class PositionForwardShortcutFunction : BaseShortcutFunction
    {
        public override string Name { get; } = "快进3秒";

        public override async Task Action(object param)
        {
            if (!(param is PlayPage page)) return;
            page.PositionForward();
        }
    }
}
