using BiliLite.Models.Theme;
using BiliLite.ViewModels.Common;
using System.Collections.ObjectModel;
using Windows.UI;

namespace BiliLite.ViewModels.Settings
{
    public class UISettingsControlViewModel : BaseViewModel
    {
        public ObservableCollection<ColorItemModel> Colors { get; set; }

        public UISettingsControlViewModel()
        {
            Colors =
            [
                new("粉色", "#D14E65", Color.FromArgb(255, 209, 78, 101)),
                new("蓝色", "#0092D0", Color.FromArgb(255, 0, 146, 208)),
            ];
        }
    }
}
