using System.Collections.Generic;

namespace BiliLite.Models.Common.Player
{
    public enum RealPlayerType
    {
        Native,
        FFmpegInterop,
    }

    public class RealPlayerTypes
    {
        public List<RealPlayerTypeOption> Options = new List<RealPlayerTypeOption>()
        {
            new RealPlayerTypeOption() { Value = RealPlayerType.Native },
            new RealPlayerTypeOption() { Value = RealPlayerType.FFmpegInterop },
        };
    }

    public class RealPlayerTypeOption
    {
        public RealPlayerType Value { get; set; }

        public string Content => Value.ToString();
    }
}
