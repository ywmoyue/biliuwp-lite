using BiliLite.Models.Common.Player;
using BiliLite.ViewModels.Common;
using System.Collections.ObjectModel;

namespace BiliLite.ViewModels.Settings
{
    public class EditPlaySpeedMenuViewModel : BaseViewModel
    {
        public ObservableCollection<PlaySpeedMenuItem> PlaySpeedMenuItems { get; set; }

        public double AddPlaySpeedValue { get; set; } = 3;
    }
}
