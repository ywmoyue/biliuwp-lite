using BiliLite.Models.Common;
using BiliLite.Models.Common.Player;
using System.Collections.Generic;

namespace BiliLite.Services
{
    public class PlaySpeedMenuService
    {
        public PlaySpeedMenuService()
        {
            MenuItems = SettingService.GetValue(SettingConstants.Player.PLAY_SPEED_MENU, GetDefaultPlaySpeedMenu());
        }
        public List<PlaySpeedMenuItem> MenuItems { get; private set; }

        public void SetMenuItems(List<PlaySpeedMenuItem> menuItems)
        {
            MenuItems = menuItems;
            SettingService.SetValue(SettingConstants.Player.PLAY_SPEED_MENU, menuItems);
        }

        private List<PlaySpeedMenuItem> GetDefaultPlaySpeedMenu()
        {
            return [.. new List<PlaySpeedMenuItem>()
            {
                new(0.25),
                new(0.5),
                new(0.75),
                new(1.0),
                new(1.25),
                new(1.5),
                new(1.75),
                new(2.0),
            }];
        }
    }
}
