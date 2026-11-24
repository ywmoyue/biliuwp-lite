using BiliLite.Models.Common.Player;

namespace BiliLite.Services
{
    /// <summary>
    /// 小窗置顶所需的最小播放器能力。
    /// 由播放器控件实现，小窗窗口实现只依赖本接口而不依赖具体播放器类型，
    /// 这样两个分支可以各自使用自己的播放器/窗口实现而共用同一套置顶逻辑。
    /// </summary>
    public interface IPlayerMiniWindowHost
    {
        /// <summary>
        /// 导出当前播放状态，用于小窗接管播放
        /// </summary>
        PlayerHandoffInfo ExportHandoff();

        /// <summary>
        /// 小窗侧起播前应用交接状态（进度、音量、静音、倍速）
        /// </summary>
        void ApplyHandoff(PlayerHandoffInfo? handoff);

        /// <summary>
        /// 小窗关闭后接回播放
        /// </summary>
        void ResumeFromHandoff(PlayerHandoffInfo? handoff);

        /// <summary>
        /// 切换精简 / 完整控制条
        /// </summary>
        void SetMiniPresentation(bool mini);

        /// <summary>
        /// 是否允许播放器自动回收焦点。
        /// 独立小窗必须关闭，否则播放器会周期性把焦点从其他窗口抢走。
        /// </summary>
        void SetAutoFocusEnabled(bool enabled);
    }
}
