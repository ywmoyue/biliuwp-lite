using BiliLite.Models.Common.Player;

namespace BiliLite.Services
{
    /// <summary>
    /// 小窗置顶（画中画）能力接口。
    /// UWP 与 WinUI3 两个分支各自实现，接口定义保持一致：
    /// UWP 使用 ApplicationView 的 CompactOverlay 在同一窗口内切换，不需要播放交接；
    /// WinUI3 使用独立的置顶窗口（播放面无法跨窗口搬移，因此需要播放交接）。
    /// 调用方（播放器、页面、快捷键）只依赖本接口。
    /// </summary>
    public interface IPlayerMiniWindowService
    {
        /// <summary>
        /// 当前平台是否支持小窗置顶
        /// </summary>
        bool IsSupported { get; }

        /// <summary>
        /// 小窗是否正在显示
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// 是否需要播放交接。
        /// 独立小窗（WinUI3）为 true，进入小窗前需要主窗口让出播放，避免两路解码抢带宽；
        /// 同窗口切换（UWP）为 false，播放器始终是同一个实例。
        /// </summary>
        bool RequiresPlaybackHandoff { get; }

        /// <summary>
        /// 打开小窗并接管播放。已打开时由实现负责激活已有窗口。
        /// </summary>
        /// <param name="handoff">进入小窗时的播放状态快照，同窗口实现会忽略它</param>
        /// <param name="owner">主窗口的播放器，用于呈现切换与退出后的播放接回</param>
        void Enter(PlayerHandoffInfo handoff, IPlayerMiniWindowHost owner);

        /// <summary>
        /// 关闭小窗，并让 owner 接回播放
        /// </summary>
        void Exit();

        /// <summary>
        /// 关闭小窗且不回调 owner（页面销毁、切换视频等场景）。
        /// 实现需保证：即使 <see cref="Exit"/> 刚被调用、接回播放还没执行，本方法也能取消这次接回。
        /// </summary>
        void CloseWithoutResume();
    }
}
