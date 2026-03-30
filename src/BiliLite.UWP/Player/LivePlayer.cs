
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common.Player;
using BiliLite.Models.Exceptions;
using BiliLite.Player.Controllers;
using BiliLite.Player.MediaInfos;
using BiliLite.Player.SubPlayers;
using BiliLite.Player.WebPlayer;
using BiliLite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;

namespace BiliLite.Player
{
    public class LivePlayer : IBiliPlayer2
    {
        private RealPlayInfo m_realPlayInfo;
        private ISubPlayer m_subPlayer;
        private BasePlayerController m_playerController;
        private PlayerConfig m_playerConfig;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private List<RealPlayerType> m_triedPlayers = new();
        private double m_rate = 1.0;
        private readonly MediaPlayerElement m_playerElement;
        private readonly ShakaPlayerControl m_shakaPlayerControl;
        private readonly MpegtsPlayerControl m_mpegtsPlayerControl;

        public LivePlayer(PlayerConfig playerConfig, MediaPlayerElement playerElement,
            BasePlayerController playerController, ShakaPlayerControl shakaPlayerControl,
            MpegtsPlayerControl mpegtsPlayerControl)
        {
            m_playerConfig = playerConfig;
            m_playerController = playerController;
            m_playerElement = playerElement;
            m_shakaPlayerControl = shakaPlayerControl;
            m_mpegtsPlayerControl = mpegtsPlayerControl;
            if (playerConfig.PlayerType == RealPlayerType.FFmpegInterop)
            {
                m_subPlayer = new LiveHlsPlayer(playerConfig, playerElement);
            }
            else if (playerConfig.PlayerType == RealPlayerType.ShakaPlayer)
            {
                m_subPlayer = new LiveShakaPlayer(playerConfig, shakaPlayerControl);
            }
            else if (playerConfig.PlayerType == RealPlayerType.Mpegts)
            {
                m_subPlayer = new LiveMpegtsPlayer(playerConfig, mpegtsPlayerControl);
            }

            InitPlayerEvents(m_subPlayer);
        }

        public BaseWebPlayer WebPlayer
        {
            get
            {
                if (m_subPlayer is ISubWebPlayer subWebPlayer)
                {
                    return subWebPlayer.WebPlayer;
                }

                return null;
            }
        }

        public double Volume
        {
            get => m_subPlayer.Volume;
            set => m_subPlayer.Volume = value;
        }

        public bool IsMuted
        {
            get => m_subPlayer?.IsMuted == true;
            set => _ = m_subPlayer?.SetMuted(value);
        }

        public bool IsBuffering => m_subPlayer?.IsBuffering == true;

        public double BufferCache => m_subPlayer?.BufferCache ?? 0;

        public double Position => m_subPlayer?.Position ?? 0;

        public event EventHandler<PlayerException> ErrorOccurred;
        public event EventHandler BufferingEnded;
        public event EventHandler<RealPlayerType> NeedReplacePlayer;
        public event EventHandler<double> PositionChanged;

        private void InitPlayerEvents(ISubPlayer subPlayer)
        {
            subPlayer.PlayerErrorOccurred += SubPlayerOnPlayerErrorOccurred;
            subPlayer.MediaOpened += SubPlayer_MediaOpened;
            subPlayer.MediaEnded += SubPlayer_MediaEnded;
            subPlayer.BufferingStarted += SubPlayer_BufferingStarted;
            subPlayer.BufferingEnded += SubPlayer_BufferingEnded;
            subPlayer.PositionChanged += SubPlayer_PositionChanged;
        }

        private void UnLoadPlayerEvents(ISubPlayer subPlayer)
        {
            subPlayer.PlayerErrorOccurred -= SubPlayerOnPlayerErrorOccurred;
            subPlayer.MediaOpened -= SubPlayer_MediaOpened;
            subPlayer.MediaEnded -= SubPlayer_MediaEnded;
            subPlayer.BufferingStarted -= SubPlayer_BufferingStarted;
            subPlayer.BufferingEnded -= SubPlayer_BufferingEnded;
            subPlayer.PositionChanged -= SubPlayer_PositionChanged;
        }

        private void SubPlayer_PositionChanged(object sender, double e)
        {
            PositionChanged?.Invoke(this, e);
        }

        private async void SubPlayer_BufferingStarted(object sender, EventArgs e)
        {
            // 状态流转约定：MediaOpened -> BufferingStarted 期间统一推进到 Buffering。
            await m_playerController.PlayState.Buff();
        }

        private async void SubPlayer_BufferingEnded(object sender, EventArgs e)
        {
            // 状态流转约定：BufferingEnded 后进入 Playing；若非自动播放则立即切回 PauseState。
            if (m_realPlayInfo?.IsAutoPlay == true)
            {
                if (m_playerController.PauseState.IsPaused)
                {
                    await m_playerController.PauseState.Resume();
                }

                await m_playerController.PlayState.Play();
                return;
            }

            await m_playerController.PlayState.Play();

            if (!m_playerController.PauseState.IsPaused)
            {
                await m_playerController.PauseState.Pause();
            }
        }

        private async void SubPlayerOnPlayerErrorOccurred(object sender, PlayerException e)
        {
            // 错误统一上报到 Controller 进行可观测记录（ErrorList / ErrorPushed）。
            m_playerController.PushError(e);

            if (e.RetryStrategy == PlayerError.RetryStrategy.NoError)
            {
                await m_playerController.PlayState.Stop();
                return;
            }

            if (e.Code == PlayerError.PlayerErrorCode.NeedUseOtherPlayerError)
            {
                m_triedPlayers.Add(m_subPlayer.Type);

                var fallbackChain = BuildFallbackChain();
                var nextType = fallbackChain.FirstOrDefault(x => !m_triedPlayers.Contains(x));
                if (fallbackChain.Contains(nextType) && !m_triedPlayers.Contains(nextType))
                {
                    NeedReplacePlayer?.Invoke(this, nextType);
                    return;
                }
            }
            await m_playerController.PlayState.Fault();
            NotificationShowExtensions.ShowMessageToast($"播放失败: {e.Description}");
            _logger.Error($"播放失败: {e.Description}");
            if (e.RetryStrategy == PlayerError.RetryStrategy.Normal)
            {
                // 可重试错误统一走 Stop -> Load，触发 RetryCount 统计。
                await m_playerController.PlayState.Stop();
                await m_playerController.PlayState.Load();
            }
            ErrorOccurred?.Invoke(this, e);
        }

        private async void SubPlayer_MediaOpened(object sender, EventArgs e)
        {
            // 状态流转约定：Load 后 MediaOpened 先进入 Buffering。
            await m_playerController.PlayState.Buff();

            if (m_realPlayInfo?.IsAutoPlay == true && m_playerController.PauseState.IsPaused)
            {
                await m_playerController.PauseState.Resume();
            }
        }

        private async void SubPlayer_MediaEnded(object sender, EventArgs e)
        {
            await m_playerController.PlayState.Stop();
        }

        public void SetRealPlayInfo(RealPlayInfo realPlayInfo)
        {
            m_realPlayInfo = realPlayInfo;
            m_triedPlayers.Clear();
            m_subPlayer.SetRealPlayInfo(realPlayInfo);
        }

        private List<RealPlayerType> BuildFallbackChain()
        {
            if (m_realPlayInfo?.FallbackPlayerTypes != null && m_realPlayInfo.FallbackPlayerTypes.Count > 0)
            {
                return m_realPlayInfo.FallbackPlayerTypes;
            }

            var preferred = m_realPlayInfo?.PreferredPlayerType ?? m_playerConfig.PlayerType;
            var defaultChain = new List<RealPlayerType>
            {
                RealPlayerType.ShakaPlayer,
                RealPlayerType.Mpegts,
                RealPlayerType.FFmpegInterop,
            };

            var orderedChain = new List<RealPlayerType> { preferred };
            orderedChain.AddRange(defaultChain.Where(x => x != preferred));
            return orderedChain.Distinct().ToList();
        }

        public CollectInfo GetCollectInfo()
        {
            return m_subPlayer.GetCollectInfo();
        }

        public async Task Load()
        {
            await m_subPlayer.SetRate(m_rate);
            await m_subPlayer.Load();
        }

        public async Task Buff()
        {
            await m_subPlayer.Buff();
        }

        public async Task Play()
        {
            await m_subPlayer.Play();
        }

        public async Task Stop()
        {
            await m_subPlayer.Stop();
        }

        public async Task Fault()
        {
            await m_subPlayer.Fault();
        }

        public async Task Pause()
        {
            await m_subPlayer.Pause();
        }

        public async Task Resume()
        {
            await m_subPlayer.Resume();
        }

        public async Task SetRate(double value)
        {
            m_rate = value;
            await m_subPlayer.SetRate(value);
        }

        public async Task SetMuted(bool muted)
        {
            if (m_subPlayer == null)
            {
                return;
            }

            await m_subPlayer.SetMuted(muted);
        }

        public async Task SetPosition(double value)
        {
            await m_subPlayer.SetPosition(value);
        }

        public async Task SetRatioMode(int mode)
        {
            await m_subPlayer.SetRatioMode(mode);
        }

        public async Task SetVideoEnable(bool enable)
        {
            await m_subPlayer.SetVideoEnable(enable);
        }

        public async Task<byte[]> CaptureAsync()
        {
            return await m_subPlayer.CaptureAsync();
        }

        public async Task UnLoad()
        {
            UnLoadPlayerEvents(m_subPlayer);
        }

        public async Task FullWindow()
        {
            await RunOnUiThreadAsync(() =>
            {
                if (m_playerElement != null)
                {
                    m_playerElement.HorizontalAlignment = HorizontalAlignment.Stretch;
                    m_playerElement.VerticalAlignment = VerticalAlignment.Stretch;
                    m_playerElement.Width = double.NaN;
                    m_playerElement.Height = double.NaN;
                }

                if (m_shakaPlayerControl != null)
                {
                    m_shakaPlayerControl.HorizontalAlignment = HorizontalAlignment.Stretch;
                    m_shakaPlayerControl.VerticalAlignment = VerticalAlignment.Stretch;
                    m_shakaPlayerControl.Width = double.NaN;
                    m_shakaPlayerControl.Height = double.NaN;
                }

                if (m_mpegtsPlayerControl != null)
                {
                    m_mpegtsPlayerControl.HorizontalAlignment = HorizontalAlignment.Stretch;
                    m_mpegtsPlayerControl.VerticalAlignment = VerticalAlignment.Stretch;
                    m_mpegtsPlayerControl.Width = double.NaN;
                    m_mpegtsPlayerControl.Height = double.NaN;
                }
            });
        }

        public async Task CancelFullWindow()
        {
            await RunOnUiThreadAsync(() =>
            {
                if (m_playerElement != null)
                {
                    m_playerElement.HorizontalAlignment = HorizontalAlignment.Stretch;
                    m_playerElement.VerticalAlignment = VerticalAlignment.Center;
                    m_playerElement.Width = double.NaN;
                    m_playerElement.Height = double.NaN;
                }

                if (m_shakaPlayerControl != null)
                {
                    m_shakaPlayerControl.HorizontalAlignment = HorizontalAlignment.Stretch;
                    m_shakaPlayerControl.VerticalAlignment = VerticalAlignment.Center;
                    m_shakaPlayerControl.Width = double.NaN;
                    m_shakaPlayerControl.Height = double.NaN;
                }

                if (m_mpegtsPlayerControl != null)
                {
                    m_mpegtsPlayerControl.HorizontalAlignment = HorizontalAlignment.Stretch;
                    m_mpegtsPlayerControl.VerticalAlignment = VerticalAlignment.Center;
                    m_mpegtsPlayerControl.Width = double.NaN;
                    m_mpegtsPlayerControl.Height = double.NaN;
                }
            });
        }

        public async Task Fullscreen()
        {
            await FullWindow();
        }

        public async Task CancelFullscreen()
        {
            await CancelFullWindow();
        }

        private static async Task RunOnUiThreadAsync(Action action)
        {
            if (action == null)
            {
                return;
            }

            var dispatcher = CoreApplication.MainView?.CoreWindow?.Dispatcher;
            if (dispatcher == null)
            {
                action();
                return;
            }

            if (dispatcher.HasThreadAccess)
            {
                action();
                return;
            }

            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
        }
    }
}
