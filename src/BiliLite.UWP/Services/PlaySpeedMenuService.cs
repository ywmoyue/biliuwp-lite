using System.Collections.Generic;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Player;
using System.Collections.ObjectModel;
using System.Linq;

namespace BiliLite.Services
{
    public class PlaySpeedMenuService
    {
        public List<PlaySpeedMenuItem> MenuItems { get; private set; }

        public PlaySpeedMenuService()
        {
            MenuItems = SettingService.GetValue(SettingConstants.Player.PLAY_SPEED_MENU, GetDefaultPlaySpeedMenu());
            MenuItems = MenuItems.OrderBy(x => x.Value).ToList();
        }

        public void SetMenuItems(List<PlaySpeedMenuItem> menuItems)
        {
            MenuItems = menuItems;
            SettingService.SetValue(SettingConstants.Player.PLAY_SPEED_MENU, menuItems);
        }

        public int SpeedIndexOf(double speed)
        {
            return MenuItems.FindIndex(x => x.Value == speed);
        }

        public List<PlaySpeedMenuItem> GetDefaultPlaySpeedMenu()
        {
            return [.. new ObservableCollection<PlaySpeedMenuItem>()
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
