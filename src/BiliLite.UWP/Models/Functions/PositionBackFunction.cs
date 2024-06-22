using System.Threading.Tasks;
using BiliLite.Pages;

namespace BiliLite.Models.Functions
{
    public class PositionBackFunction : IShortcutFunction
    {
        public string Name { get; } = "回退3秒";

        public async Task Action(object param)
        {
            if (!(param is PlayPage page)) return;
            page.PositionBack();
        }
    }
}