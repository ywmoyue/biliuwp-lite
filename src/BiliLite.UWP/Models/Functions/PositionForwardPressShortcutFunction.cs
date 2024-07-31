using BiliLite.Pages;
using System.Threading.Tasks;
using System.Timers;
using Windows.UI.Core;

namespace BiliLite.Models.Functions
{
    public class PositionForwardPressShortcutFunction : BaseShortcutFunction
    {
        public override string Name { get; } = "持续快进";

        public override async Task Action(object param)
        {
            if (!(param is PlayPage page)) return;
            var timer = new Timer(100);
            timer.Elapsed += (sender, e) =>
            {
                page.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    page.PositionForward();
                });
            };
            timer.AutoReset = true;
            (ReleaseFunction as PositionForwardReleaseShortcutFunction).Timer = timer;
            timer.Start();
        }

        public override IShortcutFunction ReleaseFunction { get; } = new PositionForwardReleaseShortcutFunction();
    }

    public class PositionForwardReleaseShortcutFunction : BaseShortcutFunction
    {
        public Timer Timer { get; set; }

        public override string Name { get; } = "停止持续快进";

        public override async Task Action(object param)
        {
            Timer?.Stop();
        }
    }
}
