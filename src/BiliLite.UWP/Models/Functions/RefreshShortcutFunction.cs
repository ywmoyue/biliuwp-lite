using System.Threading.Tasks;
using BiliLite.Pages;

namespace BiliLite.Models.Functions
{
    public class RefreshShortcutFunction : BaseShortcutFunction
    {
        public override string Name { get; } = "刷新";

        public override async Task Action(object param)
        {
            if (!(param is IRefreshablePage page)) return;
            await page.Refresh();
        }
    }
}
