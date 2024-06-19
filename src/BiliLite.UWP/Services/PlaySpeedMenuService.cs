using BiliLite.Models.Common.Player;
using System.Collections.Generic;
using System.Linq;
using BiliLite.Models.Common;

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
            return new List<PlaySpeedMenuItem>()
            {
                new PlaySpeedMenuItem(0.5),
                new PlaySpeedMenuItem(0.75),
                new PlaySpeedMenuItem(1.0),
                new PlaySpeedMenuItem(1.25),
                new PlaySpeedMenuItem(1.5),
                new PlaySpeedMenuItem(2.0),
            }.OrderByDescending(x => x.Value).ToList();
        }
    }
}
