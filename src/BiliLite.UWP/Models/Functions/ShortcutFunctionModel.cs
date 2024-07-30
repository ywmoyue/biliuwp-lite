using System.Collections.Generic;
using Windows.System;

namespace BiliLite.Models.Functions
{
    public class ShortcutFunctionModel
    {
        public string Id { get; set; }

        public string TypeName { get; set; }

        public string Name { get; set; }

        public bool NeedKeyUp { get; set; } = false;

        public List<VirtualKey> Keys { get; set; }
    }
}
