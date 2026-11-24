using System;
using BiliLite.Models.Common.Player;
using BiliLite.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;

namespace BiliLite.Pages
{
    /// <summary>
    /// 小窗置顶（画中画）播放页。
    /// 宿主是一个独立的置顶窗口，内部是独立的播放器实例：
    /// 播放面无法跨窗口搬移，所以靠 <see cref="PlayerHandoffInfo"/> 在主窗口与小窗之间交接进度。
    /// 播放器只通过 <see cref="IPlayerMiniWindowHost"/> 交互，窗口只通过
    /// <see cref="IPlayerMiniWindowService"/> 交互，便于两个分支各自实现。
    /// </summary>
    public sealed partial class MiniPlayerPage : Page
    {
        private const int STATE_CAPTURE_INTERVAL_MILLISECONDS = 250;

        /// <summary>
        /// WinUI3 小窗的呈现策略：窗口是独立的置顶窗口，布局由本页自己承担，
        /// 播放器侧只需要切到精简控制条，因此这里用默认值。
        /// </summary>
        private static readonly MiniWindowPresentation MiniPresentation = new MiniWindowPresentation();

        private PlayerHandoffInfo? m_handoff;
        private DispatcherTimer? m_stateTimer;

        public MiniPlayerPage()
        {
            InitializeComponent();
            Loaded += MiniPlayerPage_Loaded;
            Unloaded += MiniPlayerPage_Unloaded;
        }

        /// <summary>
        /// 最近一次捕获的播放状态。窗口关闭时页面可能已经卸载、播放器已经被释放，
        /// 因此退出时读这份快照而不是直接读播放器。
        /// </summary>
        public PlayerHandoffInfo? LastState { get; private set; }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var handoff = e.Parameter as PlayerHandoffInfo;
            var playInfos = handoff?.PlayInfos;
            if (handoff == null || playInfos == null || playInfos.Count == 0)
            {
                return;
            }

            m_handoff = handoff;

            txtTitle.Text = string.IsNullOrEmpty(handoff.Title) ? "小窗播放" : handoff.Title;

            if (player is IPlayerMiniWindowHost host)
            {
                // 小窗是独立窗口：禁用周期性抢焦点，并改用精简控制条
                host.SetAutoFocusEnabled(false);
                host.SetMiniPresentation(true, MiniPresentation);
                // 交接状态在取流前应用，避免起播瞬间音量/倍速不对
                host.ApplyHandoff(handoff);
            }

            player.InitializePlayInfo(playInfos, handoff.Index);

            CaptureState();
            m_stateTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(STATE_CAPTURE_INTERVAL_MILLISECONDS) };
            m_stateTimer.Tick += StateTimer_Tick;
            m_stateTimer.Start();
        }

        private void MiniPlayerPage_Loaded(object sender, RoutedEventArgs e)
        {
            // 自绘标题栏：窗口顶部交给 DragBar 承担拖拽
            var window = this.GetCurrentWindow();
            if (window == null)
            {
                return;
            }

            window.ExtendsContentIntoTitleBar = true;
            window.SetTitleBar(DragBar);

            // 避免自定义按钮与系统标题栏按钮（最小化/关闭）重叠
            var rightInset = window.AppWindow.TitleBar.RightInset;
            if (rightInset > 0)
            {
                btnBackToMain.Margin = new Thickness(0, 0, rightInset, 0);
            }
        }

        private void MiniPlayerPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (m_stateTimer != null)
            {
                m_stateTimer.Stop();
                m_stateTimer.Tick -= StateTimer_Tick;
                m_stateTimer = null;
            }

            // Dispose 之后就取不到播放位置了，关闭前先抓最后一次状态
            CaptureState();

            // 小窗是独立窗口，窗口关闭时显式释放播放器，避免 WebView2 等资源滞留
            player.Dispose();
        }

        private void StateTimer_Tick(object? sender, object e)
        {
            CaptureState();
        }

        private void CaptureState()
        {
            if (m_handoff == null || player is not IPlayerMiniWindowHost host)
            {
                return;
            }

            var state = host.ExportHandoff();
            if (state == null)
            {
                return;
            }

            LastState = new PlayerHandoffInfo
            {
                PlayInfos = m_handoff.PlayInfos,
                Index = state.Index,
                Position = state.Position,
                Speed = state.Speed,
                Volume = state.Volume,
                IsMuted = state.IsMuted,
                Title = m_handoff.Title,
            };
        }

        private void btnBackToMain_Click(object sender, RoutedEventArgs e)
        {
            ExitToMainWindow();
        }

        private void ExitKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            args.Handled = true;
            ExitToMainWindow();
        }

        private void ExitToMainWindow()
        {
            App.ServiceProvider.GetRequiredService<IPlayerMiniWindowService>().Exit();
        }
    }
}
