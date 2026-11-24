namespace BiliLite.Models.Common.Player
{
    /// <summary>
    /// 小窗呈现参数：把各分支不同的"小窗视觉策略"抽成数据，
    /// 使播放器侧的 <c>SetMiniPresentation</c> 实现可以在两个分支间共用一份。
    /// 具体数值由各分支的 IPlayerMiniWindowService 实现提供：
    /// UWP（同窗口 CompactOverlay）需要铺满 + 抵消标题栏的负边距 + 缩小弹幕/字幕；
    /// WinUI3（独立窗口）窗口本身自带走带布局，通常只切控制条。
    /// </summary>
    public class MiniWindowPresentation
    {
        /// <summary>
        /// 是否把播放器切成铺满窗口
        /// </summary>
        public bool FullWindow { get; set; }

        /// <summary>
        /// 播放器顶部负边距，用于抵消小窗标题栏高度；0 表示不调整
        /// </summary>
        public double ContentTopOffset { get; set; }

        /// <summary>
        /// 小窗弹幕字号倍率，&lt;=0 表示不调整
        /// </summary>
        public double DanmakuFontZoom { get; set; }

        /// <summary>
        /// 小窗弹幕速度，&lt;=0 表示不调整
        /// </summary>
        public int DanmakuSpeed { get; set; }

        /// <summary>
        /// 小窗字幕字号，&lt;=0 表示不调整
        /// </summary>
        public double SubtitleSize { get; set; }
    }
}
