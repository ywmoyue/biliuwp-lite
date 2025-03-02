using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BiliLite.Models.Functions;
using BiliLite.ViewModels.Common;

namespace BiliLite.ViewModels.Settings
{
    public class ShortcutKeySettingsControlViewModel : BaseViewModel
    {
        public ObservableCollection<ShortcutFunctionViewModel> ShortcutFunctions { get; set; }

        public int PressActionDelayTime { get; set; }

        public ShortcutFunctionViewModel AddShortcutFunctionModel { get; set; }

        public List<KeyValuePair<string,string>> Actions
        {
            get
            {
                return DefaultShortcuts.GetDefaultShortcutFunctions()
                    .GroupBy(x => x.TypeName)
                    .Select(group => new KeyValuePair<string, string>(
                        group.Key,
                        group.First().Name
                    ))
                    .ToList();
            }
        }

        public string AddShortcutFunctionTypeName { get; set; }

        public double ControlWidth { get; set; }
    }
}
