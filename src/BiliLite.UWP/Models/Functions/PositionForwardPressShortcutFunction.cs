using System;
using BiliLite.Extensions;
using BiliLite.Pages;
using System.Threading.Tasks;
using System.Timers;
using Windows.UI.Core;

namespace BiliLite.Models.Functions
{
    public class PositionForwardPressShortcutFunction : BaseShortcutFunction
    {
        private const int DEFAULT_STEP_TIME_SECOND = 3; // 默认步进时间为1秒
        private int m_currentStepTimeSecond = DEFAULT_STEP_TIME_SECOND;
        private const int INTERVAL_TIME = 500;
        private int m_stepPosition = 1; // 当前档位
        private const int STEP_POSITION_UP_TIMES = 5; // 每5次步进，挡位提升1档
        private static readonly int[] _stepTimesForPositions = { 3, 5, 10, 15, 30, 60, 600, 1800, 3600 };

        public override string Name { get; } = "持续快进";

        public override async Task Action(object param)
        {
            if (!(param is PlayPage page)) return;
            if (ControlsExtensions.CheckFocusTextBoxNow()) return;

            var timer = new Timer(INTERVAL_TIME);
            var elapsedTimes = 0;
            timer.Elapsed += (sender, e) =>
            {
                page.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    // 根据档位设置步进时间
                    SetStepTimeBasedOnPosition();
                    page.PositionForward(m_currentStepTimeSecond);
                });
                elapsedTimes++;
                if ((1f * elapsedTimes) % STEP_POSITION_UP_TIMES == 0)
                {
                    m_stepPosition += 1;
                }
            };
            timer.AutoReset = true;
            (ReleaseFunction as PositionForwardReleaseShortcutFunction).Excute = ()=>
            {
                timer.Stop();
                m_stepPosition = 1;
            };
            timer.Start();
        }

        private void SetStepTimeBasedOnPosition()
        {
            m_currentStepTimeSecond = m_stepPosition > 0 && m_stepPosition <= _stepTimesForPositions.Length
                ? _stepTimesForPositions[m_stepPosition - 1]
                : _stepTimesForPositions[_stepTimesForPositions.Length - 1];
        }

        public override IShortcutFunction ReleaseFunction { get; } = new PositionForwardReleaseShortcutFunction();
    }

    public class PositionForwardReleaseShortcutFunction : BaseShortcutFunction
    {
        public Action Excute { get; set; }

        public override string Name { get; } = "停止持续快进";

        public override async Task Action(object param)
        {
            Excute?.Invoke();
        }
    }
}
