using System.Collections.ObjectModel;
using BiliLite.Models.Common;
using BiliLite.Services;
using BiliLite.ViewModels.Common;

namespace BiliLite.ViewModels.Settings
{
    public class LiveSettingsControlViewModel : BaseViewModel
    {
        public LiveSettingsControlViewModel()
        {
            LoadShieldSetting();
        }

        public ObservableCollection<string> LiveShieldWords { get; set; }

        public void LoadShieldSetting()
        {
            LiveShieldWords = SettingService.GetValue<ObservableCollection<string>>(SettingConstants.Live.SHIELD_WORD, new ObservableCollection<string>() { });
        }
    }
}
