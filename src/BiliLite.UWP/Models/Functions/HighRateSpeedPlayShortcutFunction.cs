using BiliLite.Extensions;
using BiliLite.Pages;
using System.Threading.Tasks;

namespace BiliLite.Models.Functions
{
    public class StartHighRateSpeedPlayShortcutFunction : BaseShortcutFunction
    {
        public override string Name { get; } = "倍速播放";

        public override async Task Action(object param)
        {
            if (!(param is PlayPage page)) return;
            if (ControlsExtensions.CheckFocusTextBoxNow()) return;
            page.StartHighRateSpeedPlay();
        }

        public override IShortcutFunction ReleaseFunction { get; } = new StopHighRateSpeedPlayShortcutFunction();
    }

    public class StopHighRateSpeedPlayShortcutFunction : BaseShortcutFunction
    {
        public override string Name { get; } = "停止倍速播放";

        public override async Task Action(object param)
        {
            if (!(param is PlayPage page)) return;
            page.StopHighRateSpeedPlay();
        }
    }
}
