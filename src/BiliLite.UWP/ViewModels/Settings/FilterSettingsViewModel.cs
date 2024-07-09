using System.Collections.ObjectModel;
using BiliLite.ViewModels.Common;

namespace BiliLite.ViewModels.Settings
{
    public class FilterSettingsViewModel : BaseViewModel
    {
        public ObservableCollection<FilterRuleViewModel> RecommendFilterRules { get; set; }

        public ObservableCollection<FilterRuleViewModel> SearchFilterRules { get; set; }

        public ObservableCollection<FilterRuleViewModel> DynamicFilterRules { get; set; }
    }
}
