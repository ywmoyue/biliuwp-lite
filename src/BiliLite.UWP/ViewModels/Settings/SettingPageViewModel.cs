using System.Collections.ObjectModel;
using BiliLite.Models.Attributes;

namespace BiliLite.ViewModels.Settings
{
    [RegisterTransientViewModel]
    public class SettingPageViewModel
    {
        public ObservableCollection<string> SuggestSearchContents { get; set; } = new ObservableCollection<string>();
    }
}
