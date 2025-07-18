using System.Threading.Tasks;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Pages;
using BiliLite.Services;

namespace BiliLite.Models.Functions
{
    public class PositionBackFunction : BaseShortcutFunction
    {
        public override string Name { get; } = "回退3秒";

        public override async Task Action(object param)
        {
            if (!(param is PlayPage page)) return;
            if (ControlsExtensions.CheckFocusTextBoxNow()) return;
            var positionMoveLength = SettingService.GetValue(SettingConstants.ShortcutKey.POSITION_MOVE_LENGTH,
                SettingConstants.ShortcutKey.DEFAULT_POSITION_MOVE_LENGTH);
            page.PositionBack(positionMoveLength);
        }
    }
}