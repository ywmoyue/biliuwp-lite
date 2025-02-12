using BiliLite.Models.Common;
using BiliLite.Models.Theme;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using System.Collections.ObjectModel;
using Windows.UI;
using Windows.UI.ViewManagement;

namespace BiliLite.ViewModels.Settings
{
    public class UISettingsControlViewModel : BaseViewModel
    {
        public ObservableCollection<ColorItemModel> Colors;

        public UISettingsControlViewModel()
        {
            Colors = SettingService.GetValue(SettingConstants.UI.THEME_COLOR_MENU, GetDefaultThemeColorMenu());
        }

        private ObservableCollection<ColorItemModel> GetDefaultThemeColorMenu()
        {
            return
            [
                new("粉色", "#D14E65", Color.FromArgb(255, 209, 78, 101)),
                new("蓝色", "#0092D0", Color.FromArgb(255, 0, 146, 208)),
            ];
        }

        public Color SysColor => new UISettings().GetColorValue(UIColorType.Accent);
    }
}
