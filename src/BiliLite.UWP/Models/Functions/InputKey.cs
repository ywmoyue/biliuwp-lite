using System.Collections.Generic;
using Windows.System;

namespace BiliLite.Models.Functions
{
    public class InputKey
    {
        private static readonly Dictionary<int, string> _boardKeyToCharacterMap = new Dictionary<int, string>
        {
            { 188, "<" },
            { 190, ">" },
            { 186, ";" },
            { 222, "'" },
            { 189, "-" },
            { 187, "=" },
            { 220, "\\" },
            { 221, "]" }, 
            { 219, "[" },
            { 192, "`" }
        };

        public VirtualKey BoardKey { get; set; }

        public InputKey() { }

        public InputKey(VirtualKey virtualKey)
        {
            BoardKey = virtualKey;
        }

        public override string ToString()
        {
            return _boardKeyToCharacterMap.TryGetValue((int)BoardKey, out var character) ? character : BoardKey.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is InputKey inputKey)
            {
                return inputKey.BoardKey == BoardKey;
            }

            return false;
        }
    }
}
