using System.Threading.Tasks;

namespace BiliLite.Models.Functions
{
    public class NewTapFunction : BaseShortcutFunction
    {
        public override string Name { get; } = "新标签页（暂不支持设置）";

        public override async Task Action(object param)
        {
            return;
        }
    }
}
