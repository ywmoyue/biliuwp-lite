using System;
using BiliLite.Models.Common.Player;
using Windows.UI.ViewManagement;

namespace BiliLite.Services
{
    /// <summary>
    /// UWP 分支的小窗置顶实现：ApplicationView 的 CompactOverlay 同窗口切换。
    /// 播放器始终是同一个实例，因此不需要播放交接（<see cref="RequiresPlaybackHandoff"/> 为 false），
    /// 只负责切换窗口呈现形态并通知播放器切换到小窗布局。
    /// WinUI3 分支用独立的置顶窗口实现同一接口。
    /// </summary>
    public class UwpMiniPlayerWindowService : IPlayerMiniWindowService
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        /// <summary>
        /// 同窗口小窗的呈现策略：铺满 + 抵消标题栏高度 + 缩小弹幕与字幕
        /// </summary>
        private static readonly MiniWindowPresentation Presentation = new MiniWindowPresentation
        {
            FullWindow = true,
            ContentTopOffset = -40,
            DanmakuFontZoom = 0.5,
            DanmakuSpeed = 6,
            SubtitleSize = 14,
        };

        private IPlayerMiniWindowHost m_owner;

        public bool IsSupported =>
            ApplicationView.GetForCurrentView().IsViewModeSupported(ApplicationViewMode.CompactOverlay);

        public bool IsActive { get; private set; }

        public bool RequiresPlaybackHandoff => false;

        public async void Enter(PlayerHandoffInfo handoff, IPlayerMiniWindowHost owner)
        {
            if (owner == null || IsActive)
            {
                return;
            }

            m_owner = owner;
            // 先置位：调用方在 Enter 返回后会立即读取 IsActive
            IsActive = true;

            try
            {
                owner.SetMiniPresentation(true, Presentation);
                await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay);
                MessageCenter.SetMiniWindow(true);
            }
            catch (Exception ex)
            {
                _logger.Error("进入小窗失败", ex);
                IsActive = false;
                owner.SetMiniPresentation(false, Presentation);
            }
        }

        public async void Exit()
        {
            if (!IsActive)
            {
                return;
            }

            IsActive = false;

            try
            {
                MessageCenter.SetMiniWindow(false);
                await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
            }
            catch (Exception ex)
            {
                _logger.Error("退出小窗失败", ex);
            }

            if (m_owner != null)
            {
                m_owner.SetMiniPresentation(false, Presentation);
            }
        }

        public void CloseWithoutResume()
        {
            // 同窗口实现没有独立窗口需要销毁，退出小窗即可
            Exit();
        }
    }
}
