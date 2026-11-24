using System.Collections.Generic;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common.Player;
using BiliLite.Models.Common.Video;
using BiliLite.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;

namespace BiliLite.Controls
{
    /// <summary>
    /// 小窗置顶（画中画）在播放器侧的接入代码。
    /// 具体的窗口/置顶实现由 <see cref="IPlayerMiniWindowService"/> 按分支提供，
    /// 本文件只依赖接口，因此 UWP 分支（CompactOverlay 同窗口切换）与
    /// WinUI3 分支（独立置顶窗口 + 播放交接）可以共用同一份播放器接入代码。
    /// 对 PlayerControl.xaml.cs 的侵入只剩两处单行调用：
    /// SetPlayItem 里的 ApplyHandoffPositionIfNeeded()、Dispose 里的 CloseMiniWindowIfPlaying()。
    /// </summary>
    public sealed partial class PlayerControl : IPlayerMiniWindowHost
    {
        /// <summary>
        /// 是否禁止自动回收焦点
        /// </summary>
        private bool m_autoFocusDisabled;

        /// <summary>
        /// 交接播放时，首次取流完成后要跳转到的位置（秒），-1 表示不跳转
        /// </summary>
        private double m_handoffPosition = -1;

        private static IPlayerMiniWindowService MiniWindowService =>
            App.ServiceProvider.GetRequiredService<IPlayerMiniWindowService>();

        /// <summary>
        /// 进入 / 退出小窗置顶播放
        /// </summary>
        public void MiniWidnows(bool mini)
        {
            var service = MiniWindowService;

            if (mini)
            {
                if (!service.IsSupported)
                {
                    this.ShowMessageToastV2("当前平台不支持小窗置顶");
                    return;
                }

                if (PlayInfos == null || PlayInfos.Count == 0 || CurrentPlayItem == null)
                {
                    this.ShowMessageToastV2("当前没有正在播放的视频");
                    return;
                }

                var pausedByUs = false;
                if (!service.IsActive)
                {
                    // 主窗口先暂停：两路同时解码会抢带宽，还会出现回声
                    Pause();
                    pausedByUs = true;
                }

                service.Enter(ExportHandoff(), this);
                miniWin = service.IsActive;

                // 小窗没起来就别把主窗口停在那里
                if (!miniWin && pausedByUs)
                {
                    _ = Play();
                }

                return;
            }

            if (service.IsActive)
            {
                service.Exit();
            }
        }

        public void ToggleMiniWindows()
        {
            MiniWidnows(!miniWin);
        }

        /// <summary>
        /// 播放器销毁前关闭小窗，且不回接播放
        /// </summary>
        internal void CloseMiniWindowIfPlaying()
        {
            if (!miniWin)
            {
                return;
            }

            CloseMiniWindowWithoutResume();
        }

        /// <summary>
        /// 消费交接位置。由取流流程（SetPlayItem）调用，
        /// 保证进度跟随播放地址一起生效，而不是等起播后再补一次 seek。
        /// </summary>
        internal void ApplyHandoffPositionIfNeeded()
        {
            if (m_handoffPosition < 0)
            {
                return;
            }

            _postion = m_handoffPosition;
            m_handoffPosition = -1;
        }

        private void CloseMiniWindowWithoutResume()
        {
            miniWin = false;
            MiniWindowService.CloseWithoutResume();
        }

        #region IPlayerMiniWindowHost

        PlayerHandoffInfo IPlayerMiniWindowHost.ExportHandoff()
        {
            return ExportHandoff();
        }

        /// <summary>
        /// 导出当前播放状态，供小窗接管播放
        /// </summary>
        private PlayerHandoffInfo ExportHandoff()
        {
            return new PlayerHandoffInfo
            {
                // 两个播放器实例共用同一批 PlayInfo 对象会互相改写状态，这里复制一层列表
                PlayInfos = PlayInfos == null ? new List<PlayInfo>() : new List<PlayInfo>(PlayInfos),
                Index = CurrentPlayIndex,
                Position = GetCurrentPosition(),
                Speed = GetPlaySpeed(),
                Volume = m_videoPlayer?.Volume ?? 1,
                IsMuted = m_videoPlayer?.IsMuted ?? false,
                Title = CurrentPlayItem?.title,
            };
        }

        void IPlayerMiniWindowHost.ApplyHandoff(PlayerHandoffInfo? handoff)
        {
            if (handoff == null)
            {
                return;
            }

            // 位置交给取流流程消费，避免刚起播就 seek 造成的花屏/卡顿
            m_handoffPosition = handoff.Position;

            if (m_videoPlayer != null)
            {
                if (handoff.Volume > 0)
                {
                    UpdatePlayerVolume(handoff.Volume);
                }

                m_videoPlayer.IsMuted = handoff.IsMuted;
            }

            if (handoff.Speed > 0)
            {
                SetPlaySpeed(handoff.Speed);
            }
        }

        void IPlayerMiniWindowHost.ResumeFromHandoff(PlayerHandoffInfo? handoff)
        {
            miniWin = false;
            if (handoff == null)
            {
                return;
            }

            // 暂停态直接 seek；若已 Stop/Idle，Play() 会重新 Load，
            // 这时要登记待处理位置，否则位置会被重置回 0
            var playState = m_playerController?.PlayState;
            if (playState?.IsStopped == true || playState?.IsIdle == true)
            {
                m_pendingSeekPosition = handoff.Position;
            }
            else
            {
                SetPosition(handoff.Position);
            }

            if (handoff.Speed > 0)
            {
                SetPlaySpeed(handoff.Speed);
            }

            _ = Play();
        }

        void IPlayerMiniWindowHost.SetMiniPresentation(bool mini)
        {
            StandardControl.Visibility = mini ? Visibility.Collapsed : Visibility.Visible;
            MiniControl.Visibility = mini ? Visibility.Visible : Visibility.Collapsed;
        }

        void IPlayerMiniWindowHost.SetAutoFocusEnabled(bool enabled)
        {
            m_autoFocusDisabled = !enabled;
            if (enabled)
            {
                return;
            }

            timer_focus?.Stop();
            // 焦点定时器在 Loaded 里启动，若本次调用早于 Loaded，Loaded 后补停一次
            Loaded -= PlayerControl_MiniWindowLoaded;
            Loaded += PlayerControl_MiniWindowLoaded;
        }

        private void PlayerControl_MiniWindowLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= PlayerControl_MiniWindowLoaded;
            if (m_autoFocusDisabled)
            {
                timer_focus?.Stop();
            }
        }

        #endregion
    }
}
