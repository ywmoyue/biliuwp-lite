using System.Linq;

namespace BiliLite.Models.Common.Player
{
    public static class PlayerRatioModeOptions
    {
        public static PlayerRatioModeOption[] Options = new[]
        {
            new PlayerRatioModeOption(){Name = "默认",Value = PlayerRatioMode.Default},
            new PlayerRatioModeOption(){Name = "填充",Value = PlayerRatioMode.Fill},
            new PlayerRatioModeOption(){Name = "16:9",Value = PlayerRatioMode.Ratio16To9},
            new PlayerRatioModeOption(){Name = "4:3",Value = PlayerRatioMode.Ratio4To3},
            new PlayerRatioModeOption(){Name = "兼容",Value = PlayerRatioMode.Compatible},
        };

        public const PlayerRatioMode DEFAULT_PLAYER_RATIO_MODE = PlayerRatioMode.Default;

        public static PlayerRatioModeOption GetOption(PlayerRatioMode type)
        {
            return Options.FirstOrDefault(x => x.Value == type);
        }
    }

    public class PlayerRatioModeOption
    {
        public string Name { get; set; }

        public PlayerRatioMode Value { get; set; }
    }

    public enum PlayerRatioMode
    {
        Default,
        Fill,
        Ratio16To9,
        Ratio4To3,
        Compatible,
    }
}
