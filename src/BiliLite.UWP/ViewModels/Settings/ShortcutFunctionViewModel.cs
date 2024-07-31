using System.Collections.ObjectModel;
using System.Linq;
using BiliLite.ViewModels.Common;
using Windows.System;
using PropertyChanged;

namespace BiliLite.ViewModels.Settings
{
    public class ShortcutFunctionViewModel : BaseViewModel
    {
        [DoNotNotify]
        public string Id { get; set; }

        [DoNotNotify]
        public string TypeName { get; set; }

        public string Name { get; set; }

        public bool IsPressAction { get; set; }

        public string Description => IsPressAction ? "按住行为" : "点击行为";

        public bool Enable { get; set; }

        public bool NeedKeyUp { get; set; } = false;

        public ObservableCollection<VirtualKey> Keys { get; set; }

        [DependsOn(nameof(Keys))]
        public string KeysString
        {
            get
            {
                return string.Join('+', Keys.Select(x => x.ToString()));
            }
        }

        public void UpdateKeysString()
        {
            Set(nameof(KeysString));
        }
    }
}
