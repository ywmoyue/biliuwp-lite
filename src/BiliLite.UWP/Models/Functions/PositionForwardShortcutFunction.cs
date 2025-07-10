using BiliLite.Extensions;
using BiliLite.Pages;
using System.Threading.Tasks;
using BiliLite.Models.Common;
using BiliLite.Services;

namespace BiliLite.Models.Functions
{
    public class PositionForwardShortcutFunction : BaseShortcutFunction
    {
        public override string Name { get; } = "快进3秒";

        public override async Task Action(object param)
        {
            if (!(param is PlayPage page)) return;
            if (ControlsExtensions.CheckFocusTextBoxNow()) return;
            var positionMoveLength = SettingService.GetValue(SettingConstants.ShortcutKey.POSITION_MOVE_LENGTH,
                SettingConstants.ShortcutKey.DEFAULT_POSITION_MOVE_LENGTH);
            page.PositionForward(positionMoveLength);
        }
    }
}
