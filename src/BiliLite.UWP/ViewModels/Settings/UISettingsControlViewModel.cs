using System.Collections.Generic;
using BiliLite.Models.Common.Settings;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI;
using Windows.UI.ViewManagement;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common.Settings;
//using MicaForUWP.Media;
using PropertyChanged;
using BiliLite.Models.Common;
using Microsoft.UI.Xaml.Media;

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
            //MicaBackgroundSources = new MicaBackgroundSources().Options;

            //MicaBackgroundSource = (BackgroundSource)SettingService.GetValue<int>(SettingConstants.UI.MICA_BACKGROUND_SOURCE, SettingConstants.UI.DEFAULT_MICA_BACKGROUND_SOURCE);
            EnableMicaBackground = SettingService.GetValue(SettingConstants.UI.ENABLE_MICA_BACKGROUND_SOURCE, SettingConstants.UI.DEFAULT_ENABLE_MICA_BACKGROUND_SOURCE);
        }

        public ObservableCollection<ColorItemModel> Colors { get; set; }

        public Color SysColor => new UISettings().GetColorValue(UIColorType.Accent);

        //[DoNotNotify]
        //public List<KeyValueOption<BackgroundSource>> MicaBackgroundSources { get; } 

        //public BackgroundSource MicaBackgroundSource { get; set; }

        public bool EnableMicaBackground { get; set; }

        public void ResetIsActived(int index)
        {
            Colors.ToList().ForEach(item => item.IsActived = false);
            if (index >= 0)
                Colors[index].IsActived = true;
        }
    }
}
