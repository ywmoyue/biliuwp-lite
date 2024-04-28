using System.Threading.Tasks;
using BiliLite.Pages;

namespace BiliLite.Models.Functions
{
    public class RefreshShortcutFunction : IShortcutFunction
    {
        public string Name { get; } = "刷新";

        public async Task Action(object param)
        {
            if (!(param is IRefreshablePage page)) return;
            await page.Refresh();
        }
    }
}
