using BiliLite.Models.Theme;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI;
using Windows.UI.ViewManagement;
using BiliLite.Models.Attributes;

namespace BiliLite.ViewModels.Settings
{
    [RegisterTransientViewModel]
    public class UISettingsControlViewModel : BaseViewModel
    {
        private readonly ThemeService m_themeService;

        public UISettingsControlViewModel(ThemeService themeService)
        {
            m_themeService = themeService;
            Colors = new ObservableCollection<ColorItemModel>(m_themeService.GetColorMenu());
        }

        public ObservableCollection<ColorItemModel> Colors { get; set; }

        public Color SysColor => new UISettings().GetColorValue(UIColorType.Accent);

        public void ResetIsActived(int index)
        {
            Colors.ToList().ForEach(item => item.IsActived = false);
            if (index >= 0)
                Colors[index].IsActived = true;
        }
    }
}
