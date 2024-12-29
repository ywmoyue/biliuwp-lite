using BiliLite.Extensions;
using BiliLite.Pages;
using System.Threading.Tasks;
using BiliLite.Models.Common;
using BiliLite.Services;

namespace BiliLite.Models.Functions
{
    public class TogglePlaySpeedFunction : BaseShortcutFunction
    {
        public override string Name { get; } = "切换播放速度";

        public override async Task Action(object param)
        {
            if (!(param is PlayPage page)) return;
            if (ControlsExtensions.CheckFocusTextBoxNow()) return;
            var currentSpeed = page.GetPlaySpeed();
            if (currentSpeed == 1)
            {
                var speed = (float)SettingService.GetValue(SettingConstants.Player.TOGGLE_PLAY_SPEED_VALUE, SettingConstants.Player.TOGGLE_PLAY_SPEED_DEFAULT_VALUE);
                page.SetPlaySpeed(speed);
            }
            else
            {
                SettingService.SetValue(SettingConstants.Player.TOGGLE_PLAY_SPEED_VALUE, currentSpeed);
                page.SetPlaySpeed(1);
            }
        }
    }
}
