using BiliLite.Extensions;
using BiliLite.Pages;
using System.Threading.Tasks;
using System.Timers;
using Windows.UI.Core;

namespace BiliLite.Models.Functions
{
    public class PositionForwardPressShortcutFunction : BaseShortcutFunction
    {
        private const int DefaultStepTimeMs = 1000; // 默认步进时间为1秒
        private int _currentStepTimeMs = DefaultStepTimeMs;
        private int _stepPosition = 1; // 当前档位

        public override string Name { get; } = "持续快进";

        public override async Task Action(object param)
        {
            if (!(param is PlayPage page)) return;
            if (ControlsExtensions.CheckFocusTextBoxNow()) return;

            // 根据档位设置步进时间
            SetStepTimeBasedOnPosition();

            var timer = new Timer(_currentStepTimeMs);
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

    private void SetStepTimeBasedOnPosition()
    {
        if (_stepPosition >= 1 && _stepPosition <= 4)
        {
            // 对于前四档，使用线性增长
            _currentStepTimeMs = new[] { 1000, 3000, 5000, 10000 }[_stepPosition - 1];
        }
        else if (_stepPosition >= 5 && _stepPosition <= 9)
        {
            // 对于后五档，使用指数增长
            _currentStepTimeMs = new[] { 30000, 60000, 600000, 1800000, 3600000 }[_stepPosition - 5];
        }
        else
        {
            _currentStepTimeMs = DefaultStepTimeMs;
        }
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
