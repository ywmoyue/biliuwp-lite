namespace BiliLite.Models.Common.Player
{
    public class PlayerConfig
    {
        public bool EnableHw { get; set; }

        public LivePlayerMode PlayMode { get; set; }

        public RealPlayerType PlayerType { get; set; }

        public int SelectedRouteLine { get; set; } = 0;

        // 是否在播放前临时下载完整音频
        public bool PreloadFullAudioBeforePlay { get; set; } = false;
    }
}
