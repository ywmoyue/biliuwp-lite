using System.Collections.Generic;
using System.Threading.Tasks;

namespace BiliLite.Models.Functions
{
    public interface IShortcutFunction
    {
        public string Id { get; set; }

        public string TypeName { get; }

        public string Name { get; }

        public bool NeedKeyUp { get; set; }

        public bool Enable { get; set; }

        public bool Canceled { get; set; }

        public List<InputKey> Keys { get; set; }

        public Task Action(object param);

        public IShortcutFunction ReleaseFunction { get; }
    }
}
