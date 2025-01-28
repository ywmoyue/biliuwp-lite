using BiliLite.Models.Functions;
using BiliLite.ViewModels.Common;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.Linq;

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

        public string Description => IsPressAction ? "长按执行" : "点击执行";

        public bool Enable { get; set; }

        public bool NeedKeyUp { get; set; } = false;

        public ObservableCollection<InputKey> Keys { get; set; }

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
