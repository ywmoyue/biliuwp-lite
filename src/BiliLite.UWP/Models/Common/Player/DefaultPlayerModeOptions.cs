namespace BiliLite.Models.Common.Player
{
    public static class DefaultPlayerModeOptions
    {
        public static LivePlayerModeOption[] DefaultLivePlayerModeOption = new LivePlayerModeOption[]
        {
            new LivePlayerModeOption() { Name = "HLS", Value = LivePlayerMode.Hls },
            new LivePlayerModeOption() { Name = "FLV", Value = LivePlayerMode.Flv },
        };

        public const LivePlayerMode DEFAULT_LIVE_PLAYER_MODE = LivePlayerMode.Hls;
    }

    public class LivePlayerModeOption
    {
        public string Name { get; set; }

        public LivePlayerMode Value { get; set; }
    }
}
