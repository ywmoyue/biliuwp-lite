using System.Collections.Generic;
using BiliLite.Models.Common.Video;

namespace BiliLite.Models.Common.Player
{
    /// <summary>
    /// 播放交接信息：主窗口与小窗之间传递的播放状态快照。
    /// 播放面（WebView2 / MediaPlayerElement）无法跨窗口搬移，所以小窗使用独立的播放器实例，
    /// 进入小窗时由主窗口导出、退出小窗时由小窗导出，双方各自 seek 到交接位置继续播放。
    /// </summary>
    public class PlayerHandoffInfo
    {
        /// <summary>
        /// 播放列表（分P / 剧集）。小窗会重新走一次取流，因此只交接列表与序号，不交接播放地址，
        /// 避免使用已经过期或鉴权失效的旧地址。
        /// </summary>
        public List<PlayInfo>? PlayInfos { get; set; }

        /// <summary>
        /// 当前播放项在 <see cref="PlayInfos"/> 中的序号
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 交接时的播放进度（秒）
        /// </summary>
        public double Position { get; set; }

        /// <summary>
        /// 播放速度倍率
        /// </summary>
        public double Speed { get; set; } = 1;

        /// <summary>
        /// 音量（0~1）
        /// </summary>
        public double Volume { get; set; }

        /// <summary>
        /// 是否静音
        /// </summary>
        public bool IsMuted { get; set; }

        /// <summary>
        /// 标题，用于小窗标题栏展示
        /// </summary>
        public string? Title { get; set; }
    }
}
