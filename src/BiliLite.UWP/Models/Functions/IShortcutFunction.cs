using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.System;

namespace BiliLite.Models.Functions
{
    public interface IShortcutFunction
    {
        public string Name { get; }

        public bool NeedKeyUp { get; set; }

        public bool Canceled { get; set; }

        public List<VirtualKey> Keys { get; set; }

        public double DelayTime { get; set; }

        public Task Action(object param);

        public IShortcutFunction ReleaseFunction { get; }
    }
}
