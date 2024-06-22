using System.Threading.Tasks;
using BiliLite.Pages;

namespace BiliLite.Models.Functions
{
    public class AddVolumeFunction : IShortcutFunction
    {
        public string Name { get; } = "音量+";

        public async Task Action(object param)
        {
            if (!(param is PlayPage page)) return;
            page.AddVolume();
        }
    }
}
