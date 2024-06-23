using BiliLite.Pages;
using System.Threading.Tasks;

namespace BiliLite.Models.Functions
{
    public class SaveFunction : IShortcutFunction
    {
        public string Name { get; } = "保存操作";

        public async Task Action(object param)
        {
            if (!(param is ISavablePage page)) return;
            await page.Save();
        }
    }
}
