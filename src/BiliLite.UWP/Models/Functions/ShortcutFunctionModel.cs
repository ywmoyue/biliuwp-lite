using System.Collections.Generic;

namespace BiliLite.Models.Functions
{
    public class ShortcutFunctionModel
    {
        public string Id { get; set; }

        public string TypeName { get; set; }

        public string Name { get; set; }

        public bool Enable { get; set; }

        public bool NeedKeyUp { get; set; } = false;

        public List<InputKey> Keys { get; set; }
    }
}
