using System.Threading.Tasks;
using BiliLite.Extensions;
using BiliLite.Pages;

namespace BiliLite.Models.Functions
{
    public class PositionBackFunction : BaseShortcutFunction
    {
        public override string Name { get; } = "回退3秒";

        public override async Task Action(object param)
        {
            if (!(param is PlayPage page)) return;
            if (ControlsExtensions.CheckFocusTextBoxNow()) return;
            page.PositionBack();
        }
    }
}