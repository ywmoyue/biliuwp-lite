using System.Threading.Tasks;
using BiliLite.Pages;

namespace BiliLite.Models.Functions
{
    public class MinusVolumeFunction : BaseShortcutFunction
    {
        public override string Name { get; } = "音量-";

        public override async Task Action(object param)
        {
            if (!(param is PlayPage page)) return;
            page.MinusVolume();
        }
    }
}
