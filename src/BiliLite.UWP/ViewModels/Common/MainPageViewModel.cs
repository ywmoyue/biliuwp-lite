using System;
using BiliLite.Models.Common;
using BiliLite.Services;
using PropertyChanged;

namespace BiliLite.ViewModels.Common
{
    public class MainPageViewModel : BaseViewModel
    {
        [DoNotNotify]
        public bool EnableTabItemFixedWidth => SettingService.GetValue(
            SettingConstants.UI.ENABLE_TAB_ITEM_FIXED_WIDTH,
            SettingConstants.UI.DEFAULT_ENABLE_TAB_ITEM_FIXED_WIDTH);

        [DoNotNotify]
        public double TabItemFixedWidth => EnableTabItemFixedWidth
            ? SettingService.GetValue(
                SettingConstants.UI.TAB_ITEM_FIXED_WIDTH,
                SettingConstants.UI.DEFAULT_TAB_ITEM_FIXED_WIDTH)
            : Double.NaN;

        [DoNotNotify]
        public double TabItemMaxWidth => EnableTabItemFixedWidth
            ? TabItemFixedWidth
            : SettingService.GetValue(
                SettingConstants.UI.TAB_ITEM_MAX_WIDTH,
                SettingConstants.UI.DEFAULT_TAB_ITEM_MAX_WIDTH);

        [DoNotNotify]
        public double TabItemMinWidth => EnableTabItemFixedWidth
            ? TabItemFixedWidth
            : SettingService.GetValue(
                SettingConstants.UI.TAB_ITEM_MIN_WIDTH,
                SettingConstants.UI.DEFAULT_TAB_ITEM_MIN_WIDTH);
    }
}
