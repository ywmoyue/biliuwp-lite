using BiliLite.Models.Common;
using BiliLite.Models.Common.Player;
using BiliLite.ViewModels.Common;
using System.Collections.ObjectModel;

namespace BiliLite.Services
{
    public class PlaySpeedMenuService : BaseViewModel
    {
        public ObservableCollection<PlaySpeedMenuItem> MenuItems { get; private set; }

        public PlaySpeedMenuService()
        {
            MenuItems = SettingService.GetValue(SettingConstants.Player.PLAY_SPEED_MENU, GetDefaultPlaySpeedMenu());
        }

        public void SetMenuItems(ObservableCollection<PlaySpeedMenuItem> menuItems)
        {
            MenuItems = menuItems;
            SettingService.SetValue(SettingConstants.Player.PLAY_SPEED_MENU, menuItems);
        }

        private ObservableCollection<PlaySpeedMenuItem> GetDefaultPlaySpeedMenu()
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
