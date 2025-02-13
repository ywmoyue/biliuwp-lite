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
                new("粉色", "#D14E65", Microsoft.Toolkit.Uwp.Helpers.ColorHelper.ToColor("#D14E65")),
                new("蓝色", "#0092D0", Microsoft.Toolkit.Uwp.Helpers.ColorHelper.ToColor("#0092D0")),
                new("黄色", "#C5963C", Microsoft.Toolkit.Uwp.Helpers.ColorHelper.ToColor("#C5963C")),
                new("绿色", "#5B8F30", Microsoft.Toolkit.Uwp.Helpers.ColorHelper.ToColor("#5B8F30")),
                new("淡紫色", "#9664DB", Microsoft.Toolkit.Uwp.Helpers.ColorHelper.ToColor("#9664DB")),
            ];
        }

        public Color SysColor => new UISettings().GetColorValue(UIColorType.Accent);
    }
}
