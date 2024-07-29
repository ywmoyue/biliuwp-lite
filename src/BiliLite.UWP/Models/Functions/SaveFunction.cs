using BiliLite.Pages;
using System.Threading.Tasks;

namespace BiliLite.Models.Functions
{
    public class SaveFunction : BaseShortcutFunction
    {
        public override string Name { get; } = "保存操作";

        public override async Task Action(object param)
        {
            if (!(param is ISavablePage page)) return;
            await page.Save();
        }
    }
}
