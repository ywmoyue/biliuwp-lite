using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace BiliLite.Models.Functions
{
    internal class RestartAppFunction : BaseShortcutFunction
    {
        public override string Name { get; } = "重启应用";

        public override async Task Action(object param) => await CoreApplication.RequestRestartAsync(string.Empty);
    }
}
