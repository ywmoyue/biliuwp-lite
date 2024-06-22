using System.Collections.ObjectModel;
using BiliLite.Models.Common.Player;
using BiliLite.ViewModels.Common;

namespace BiliLite.ViewModels.Settings
{
    public class EditPlaySpeedMenuViewModel : BaseViewModel
    {
        public ObservableCollection<PlaySpeedMenuItem> PlaySpeedMenuItems { get; set; }

        public double AddPlaySpeedValue { get; set; } = 1;
    }
}
