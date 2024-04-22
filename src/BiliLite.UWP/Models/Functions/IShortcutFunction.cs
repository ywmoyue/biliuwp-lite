using System.Threading.Tasks;

namespace BiliLite.Models.Functions
{
    public interface IShortcutFunction
    {
        public string Name { get; }

        public Task Action(object param);
    }
}
