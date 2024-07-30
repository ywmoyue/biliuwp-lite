using System.Collections.ObjectModel;
using BiliLite.ViewModels.Common;

namespace BiliLite.ViewModels.Settings
{
    public class ShortcutKeySettingsControlViewModel : BaseViewModel
    {
        public ObservableCollection<ShortcutFunctionViewModel> ShortcutFunctions { get; set; }

        public int PressActionDelayTime { get; set; }
    }
}
