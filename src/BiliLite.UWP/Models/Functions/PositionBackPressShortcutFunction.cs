using System.Threading.Tasks;
using System.Timers;
using Windows.UI.Core;
using BiliLite.Pages;
using BiliLite.Extensions;

namespace BiliLite.Models.Functions
{
    public class PositionBackPressShortcutFunction : BaseShortcutFunction
    {
        public override string Name { get; } = "持续回退";

        public override async Task Action(object param)
        {
            if (!(param is PlayPage page)) return;
            if (ControlsExtensions.CheckFocusTextBoxNow()) return;
            var timer = new Timer(100);
            timer.Elapsed += (sender, e) =>
            {
                page.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    page.PositionBack();
                });
            };
            timer.AutoReset = true;
            (ReleaseFunction as PositionBackReleaseShortcutFunction).Timer = timer;
            timer.Start();
        }

        public override IShortcutFunction ReleaseFunction { get; } = new PositionBackReleaseShortcutFunction();
    }

    public class PositionBackReleaseShortcutFunction : BaseShortcutFunction
    {
        public Timer Timer { get; set; }

        public override string Name { get; } = "停止持续回退";

        public override async Task Action(object param)
        {
            Timer?.Stop();
        }
    }
}
