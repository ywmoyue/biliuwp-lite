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

        // 是否启用音量均衡（依赖 PreloadFullAudioBeforePlay）
        public bool EnableVolumeNormalization { get; set; } = false;

        // 目标响度（LUFS），推荐 -14
        public double VolumeNormalizationLoudness { get; set; } = -14d;
    }
}
