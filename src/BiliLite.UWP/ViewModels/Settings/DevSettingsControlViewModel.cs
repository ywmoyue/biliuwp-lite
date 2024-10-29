using System.Collections.ObjectModel;
using System.Linq;
using BiliLite.ViewModels.Common;
using BiliLite.ViewModels.Plugins;
using PropertyChanged;

namespace BiliLite.ViewModels.Settings
{
    public class DevSettingsControlViewModel : BaseViewModel
    {
        public ObservableCollection<WebSocketPluginViewModel> Plugins { get; set; } =
            new ObservableCollection<WebSocketPluginViewModel>();

        [DependsOn(nameof(Plugins))]
        public bool ShowPluginList => Plugins.Any ();

        public void AddPlugin(WebSocketPluginViewModel plugin)
        {
            var oldPlugin = Plugins.FirstOrDefault(x => x.Name == plugin.Name);
            if (oldPlugin != null)
            {
                oldPlugin.CheckUrl = plugin.CheckUrl;
                oldPlugin.WakeProto = plugin.WakeProto;
                oldPlugin.WebSocketUrl = plugin.WebSocketUrl;
            }
            else
            {
                Plugins.Add(plugin);
                Set(nameof(ShowPluginList));
            }
        }

        public void RemovePlugin(WebSocketPluginViewModel plugin)
        {
            Plugins.Remove(plugin);
            Set(nameof(ShowPluginList));
        }
    }
}
