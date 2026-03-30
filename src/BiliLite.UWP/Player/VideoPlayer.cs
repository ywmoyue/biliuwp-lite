using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Player;
using BiliLite.Models.Exceptions;
using BiliLite.Player.Controllers;
using BiliLite.Player.MediaInfos;
using BiliLite.Player.SubPlayers;
using BiliLite.Player.WebPlayer;
using BiliLite.Services;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace BiliLite.Player
{
    /// <summary>
    /// 点播播放器：负责 SubPlayer 选择、状态机推进与回退通知。
    /// </summary>
    public class VideoPlayer : IBiliPlayer2
    {
        private readonly PlayerConfig m_playerConfig;
        private readonly BasePlayerController m_playerController;
        private readonly MediaPlayerElement m_playerElement;
        private readonly ShakaPlayerControl m_shakaPlayerControl;
        private readonly List<RealPlayerType> m_triedPlayers = new();
        private readonly object m_bufferLock = new();
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        private ISubPlayer m_subPlayer;
        private RealPlayInfo m_realPlayInfo;
        private double m_rate = 1.0;
        private bool m_abSeeking;
        private bool m_isMuted;
        private double m_lastVolume = 1.0;
        private bool m_isBuffering;
        private double m_bufferCache;
        private DateTime m_lastBufferingUiNotifyAt = DateTime.MinValue;
        private DateTime m_lastBufferCacheNotifyAt = DateTime.MinValue;
        private static readonly TimeSpan BufferingNotifyMinInterval = TimeSpan.FromMilliseconds(120);
        private static readonly TimeSpan BufferCacheNotifyMinInterval = TimeSpan.FromMilliseconds(120);

        public VideoPlayer(PlayerConfig playerConfig,
            MediaPlayerElement playerElement,
            BasePlayerController playerController,
            ShakaPlayerControl shakaPlayerControl)
        {
            m_playerConfig = playerConfig;
            m_playerElement = playerElement;
            m_playerController = playerController;
            m_shakaPlayerControl = shakaPlayerControl;
            m_subPlayer = CreateSubPlayer(m_playerConfig.PlayerType, null);
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
            get => m_subPlayer?.Volume ?? 1;
            set
            {
                var normalized = Math.Min(1, Math.Max(0, value));
                if (m_subPlayer != null)
                {
                    m_subPlayer.Volume = normalized;
                    if (!m_isMuted)
                    {
                        m_lastVolume = normalized;
                    }
                }
            }
        }

        public bool IsMuted
        {
            get => m_isMuted;
            set
            {
                _ = SetMuted(value);
            }
        }

        public bool IsBuffering => m_isBuffering;

        public double BufferCache => m_bufferCache;

        public double Position => m_subPlayer?.Position ?? 0;

        public VideoPlayHistoryHelper.ABPlayHistoryEntry ABPlay { get; set; }

        public event EventHandler<PlayerException> ErrorOccurred;
        public event EventHandler<RealPlayerType> NeedReplacePlayer;
        public event EventHandler<double> PositionChanged;
        public event EventHandler<bool> BufferingChanged;
        public event EventHandler<double> BufferCacheChanged;

        public void SetRealPlayInfo(RealPlayInfo realPlayInfo)
        {
            m_realPlayInfo = realPlayInfo;
            m_subPlayer.SetRealPlayInfo(realPlayInfo);
        }

        public CollectInfo GetCollectInfo()
        {
            return m_subPlayer.GetCollectInfo();
        }

        public async Task Load()
        {
            PrepareSubPlayerForLoad();
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
                m_isMuted = muted;
                return;
            }

            if (muted)
            {
                if (!m_isMuted)
                {
                    m_lastVolume = m_subPlayer.Volume;
                }

                m_isMuted = true;
                await m_subPlayer.SetMuted(true);
                return;
            }

            m_isMuted = false;
            await m_subPlayer.SetMuted(false);

            if (m_subPlayer.Volume <= 0 && m_lastVolume > 0)
            {
                m_subPlayer.Volume = m_lastVolume;
            }
        }

        public async Task SetPosition(double value)
        {
            if (m_subPlayer == null)
            {
                return;
            }

            await m_subPlayer.SetPosition(value);
        }

        public async Task SetRatioMode(int mode)
        {
            if (m_subPlayer == null)
            {
                return;
            }

            await m_subPlayer.SetRatioMode(mode);
        }

        public async Task SetVideoEnable(bool enable)
        {
            if (m_subPlayer == null)
            {
                return;
            }

            await m_subPlayer.SetVideoEnable(enable);
        }

        public async Task<byte[]> CaptureAsync()
        {
            if (m_subPlayer == null)
            {
                return null;
            }

            return await m_subPlayer.CaptureAsync();
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
            });
        }

        public async Task Fullscreen()
        {
            await RunOnUiThreadAsync(() =>
            {
                if (m_playerElement != null)
                {
                    m_playerElement.HorizontalAlignment = HorizontalAlignment.Stretch;
                    m_playerElement.VerticalAlignment = VerticalAlignment.Stretch;
                }

                if (m_shakaPlayerControl != null)
                {
                    m_shakaPlayerControl.HorizontalAlignment = HorizontalAlignment.Stretch;
                    m_shakaPlayerControl.VerticalAlignment = VerticalAlignment.Stretch;
                }
            });
        }

        public async Task CancelFullscreen()
        {
            await CancelFullWindow();
        }

        public async Task UnLoad()
        {
            if (m_subPlayer == null)
            {
                return;
            }

            UnLoadPlayerEvents(m_subPlayer);
            await m_subPlayer.Stop();
            EmitBufferingChanged(false, force: true);
            EmitBufferCacheChanged(0, force: true);
        }

        private ISubPlayer CreateSubPlayer(RealPlayerType playerType, RealPlayInfo playInfo)
        {
            if (playInfo?.PlayMediaType == PlayMediaType.MultiFlv)
            {
                return new MultiFlvSYEngineSubPlayer(m_playerElement);
            }

            if (playInfo?.PlayMediaType == PlayMediaType.Single && playInfo.SingleIsFlv)
            {
                return playerType == RealPlayerType.FFmpegInterop
                    ? new FlvFFmpegInteropSubPlayer(m_playerElement)
                    : new FlvSYEngineSubPlayer(m_playerElement);
            }

            if (playInfo?.PlayMediaType == PlayMediaType.Dash)
            {
                return playerType switch
                {
                    RealPlayerType.ShakaPlayer => new DashShakaSubPlayer(m_shakaPlayerControl),
                    RealPlayerType.Native => new DashNativeSubPlayer(m_playerElement),
                    RealPlayerType.FFmpegInterop => new DashFFmpegInteropSubPlayer(m_playerElement),
                    _ => new DashShakaSubPlayer(m_shakaPlayerControl),
                };
            }

            return playerType switch
            {
                RealPlayerType.ShakaPlayer => new DashShakaSubPlayer(m_shakaPlayerControl),
                RealPlayerType.Native => new Mp4NativeSubPlayer(m_playerElement),
                RealPlayerType.FFmpegInterop => new FlvFFmpegInteropSubPlayer(m_playerElement),
                _ => new FlvFFmpegInteropSubPlayer(m_playerElement),
            };
        }

        private void PrepareSubPlayerForLoad()
        {
            var targetType = m_playerConfig.PlayerType;
            if (m_realPlayInfo?.FallbackPlayerTypes?.Count > 0)
            {
                targetType = m_realPlayInfo.FallbackPlayerTypes.First();
            }

            var expectedSubPlayerType = GetSubPlayerRuntimeType(targetType, m_realPlayInfo);

            if (m_subPlayer != null && m_subPlayer.GetType() == expectedSubPlayerType)
            {
                m_subPlayer.SetRealPlayInfo(m_realPlayInfo);
                return;
            }

            if (m_subPlayer != null)
            {
                UnLoadPlayerEvents(m_subPlayer);
            }

            m_subPlayer = CreateSubPlayer(targetType, m_realPlayInfo);
            m_subPlayer.SetRealPlayInfo(m_realPlayInfo);
            InitPlayerEvents(m_subPlayer);
        }

        private Type GetSubPlayerRuntimeType(RealPlayerType playerType, RealPlayInfo playInfo)
        {
            if (playInfo?.PlayMediaType == PlayMediaType.MultiFlv)
            {
                return typeof(MultiFlvSYEngineSubPlayer);
            }

            if (playInfo?.PlayMediaType == PlayMediaType.Single && playInfo.SingleIsFlv)
            {
                return playerType == RealPlayerType.FFmpegInterop
                    ? typeof(FlvFFmpegInteropSubPlayer)
                    : typeof(FlvSYEngineSubPlayer);
            }

            if (playInfo?.PlayMediaType == PlayMediaType.Dash)
            {
                return playerType switch
                {
                    RealPlayerType.ShakaPlayer => typeof(DashShakaSubPlayer),
                    RealPlayerType.Native => typeof(DashNativeSubPlayer),
                    RealPlayerType.FFmpegInterop => typeof(DashFFmpegInteropSubPlayer),
                    _ => typeof(DashShakaSubPlayer),
                };
            }

            return playerType switch
            {
                RealPlayerType.ShakaPlayer => typeof(DashShakaSubPlayer),
                RealPlayerType.Native => typeof(Mp4NativeSubPlayer),
                RealPlayerType.FFmpegInterop => typeof(FlvFFmpegInteropSubPlayer),
                _ => typeof(FlvFFmpegInteropSubPlayer),
            };
        }

        private static bool IsDash(RealPlayInfo info) => info?.PlayMediaType == PlayMediaType.Dash;

        private static bool IsSingleFlv(RealPlayInfo info) => info?.PlayMediaType == PlayMediaType.Single && info.SingleIsFlv;

        private static bool IsMultiFlv(RealPlayInfo info) => info?.PlayMediaType == PlayMediaType.MultiFlv;

        private List<RealPlayerType> BuildFallbackChain()
        {
            if (m_realPlayInfo?.FallbackPlayerTypes != null && m_realPlayInfo.FallbackPlayerTypes.Count > 0)
            {
                return m_realPlayInfo.FallbackPlayerTypes;
            }

            if (IsDash(m_realPlayInfo))
            {
                return new List<RealPlayerType>
                {
                    RealPlayerType.Native,
                    RealPlayerType.FFmpegInterop,
                    RealPlayerType.ShakaPlayer,
                };
            }

            if (IsSingleFlv(m_realPlayInfo))
            {
                return new List<RealPlayerType>
                {
                    RealPlayerType.FFmpegInterop,
                    RealPlayerType.Native,
                };
            }

            if (IsMultiFlv(m_realPlayInfo))
            {
                return new List<RealPlayerType>
                {
                    RealPlayerType.FFmpegInterop,
                };
            }

            return new List<RealPlayerType>
            {
                RealPlayerType.Native,
                RealPlayerType.FFmpegInterop,
            };
        }

        private async void SubPlayer_BufferingStarted(object sender, EventArgs e)
        {
            EmitBufferingChanged(true);
            await m_playerController.PlayState.Buff();
        }

        private async void SubPlayer_BufferingEnded(object sender, EventArgs e)
        {
            EmitBufferingChanged(false);
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
                await m_playerController.PlayState.Stop();
                await m_playerController.PlayState.Load();
            }

            ErrorOccurred?.Invoke(this, e);
        }

        private async void SubPlayer_MediaOpened(object sender, EventArgs e)
        {
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

        private async void SubPlayer_PositionChanged(object sender, double position)
        {
            PositionChanged?.Invoke(this, position);

            if (m_abSeeking || ABPlay == null)
            {
                return;
            }

            if (ABPlay.PointB != 0 && ABPlay.PointB != double.MaxValue && position > ABPlay.PointB)
            {
                m_abSeeking = true;
                try
                {
                    await m_subPlayer.SetPosition(ABPlay.PointA);
                }
                finally
                {
                    m_abSeeking = false;
                }
            }
        }

        private void SubPlayer_BufferCacheChanged(object sender, double cache)
        {
            EmitBufferCacheChanged(cache);
        }

        private void EmitBufferingChanged(bool isBuffering, bool force = false)
        {
            bool shouldNotify;
            lock (m_bufferLock)
            {
                var now = DateTime.UtcNow;
                shouldNotify = force || m_isBuffering != isBuffering ||
                               (now - m_lastBufferingUiNotifyAt) >= BufferingNotifyMinInterval;
                m_isBuffering = isBuffering;
                if (!shouldNotify)
                {
                    return;
                }

                m_lastBufferingUiNotifyAt = now;
            }

            BufferingChanged?.Invoke(this, isBuffering);
        }

        private void EmitBufferCacheChanged(double cache, bool force = false)
        {
            var normalized = Math.Min(1, Math.Max(0, cache));
            bool shouldNotify;
            lock (m_bufferLock)
            {
                var now = DateTime.UtcNow;
                var delta = Math.Abs(m_bufferCache - normalized);
                shouldNotify = force || delta >= 0.01 ||
                               (now - m_lastBufferCacheNotifyAt) >= BufferCacheNotifyMinInterval;
                m_bufferCache = normalized;
                if (!shouldNotify)
                {
                    return;
                }

                m_lastBufferCacheNotifyAt = now;
            }

            BufferCacheChanged?.Invoke(this, normalized);
        }

        private void InitPlayerEvents(ISubPlayer subPlayer)
        {
            subPlayer.PlayerErrorOccurred += SubPlayerOnPlayerErrorOccurred;
            subPlayer.MediaOpened += SubPlayer_MediaOpened;
            subPlayer.MediaEnded += SubPlayer_MediaEnded;
            subPlayer.BufferingStarted += SubPlayer_BufferingStarted;
            subPlayer.BufferingEnded += SubPlayer_BufferingEnded;
            subPlayer.BufferCacheChanged += SubPlayer_BufferCacheChanged;
            subPlayer.PositionChanged += SubPlayer_PositionChanged;

            subPlayer.Volume = m_isMuted ? 0 : m_lastVolume;
            _ = subPlayer.SetMuted(m_isMuted);
        }

        private void UnLoadPlayerEvents(ISubPlayer subPlayer)
        {
            subPlayer.PlayerErrorOccurred -= SubPlayerOnPlayerErrorOccurred;
            subPlayer.MediaOpened -= SubPlayer_MediaOpened;
            subPlayer.MediaEnded -= SubPlayer_MediaEnded;
            subPlayer.BufferingStarted -= SubPlayer_BufferingStarted;
            subPlayer.BufferingEnded -= SubPlayer_BufferingEnded;
            subPlayer.BufferCacheChanged -= SubPlayer_BufferCacheChanged;
            subPlayer.PositionChanged -= SubPlayer_PositionChanged;
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

        internal static async Task<byte[]> RenderElementToPngBytesAsync(UIElement element, double dpi)
        {
            if (element == null)
            {
                return null;
            }

            var bitmap = new Windows.UI.Xaml.Media.Imaging.RenderTargetBitmap();
            await bitmap.RenderAsync(element);
            var pixelBuffer = await bitmap.GetPixelsAsync();

            using var stream = new Windows.Storage.Streams.InMemoryRandomAccessStream();
            var encoder = await Windows.Graphics.Imaging.BitmapEncoder
                .CreateAsync(Windows.Graphics.Imaging.BitmapEncoder.PngEncoderId, stream);
            encoder.SetPixelData(Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8,
                Windows.Graphics.Imaging.BitmapAlphaMode.Ignore,
                (uint)bitmap.PixelWidth,
                (uint)bitmap.PixelHeight,
                dpi,
                dpi,
                pixelBuffer.ToArray());
            await encoder.FlushAsync();

            var dataReader = new Windows.Storage.Streams.DataReader(stream.GetInputStreamAt(0));
            await dataReader.LoadAsync((uint)stream.Size);
            var bytes = new byte[(int)stream.Size];
            dataReader.ReadBytes(bytes);
            dataReader.Dispose();
            return bytes;
        }

        internal static void ApplyStretch(MediaPlayerElement playerElement, RealPlayInfo playInfo, int mode)
        {
            if (playerElement == null)
            {
                return;
            }

            switch (mode)
            {
                case 0:
                    playerElement.Width = double.NaN;
                    playerElement.Height = double.NaN;
                    playerElement.Stretch = Stretch.Uniform;
                    break;
                case 1:
                    playerElement.Width = double.NaN;
                    playerElement.Height = double.NaN;
                    playerElement.Stretch = Stretch.UniformToFill;
                    break;
                case 2:
                    playerElement.Stretch = Stretch.Fill;
                    playerElement.Height = double.NaN;
                    playerElement.Width = double.NaN;
                    break;
                case 3:
                    playerElement.Stretch = Stretch.Fill;
                    playerElement.Height = double.NaN;
                    playerElement.Width = double.NaN;
                    break;
                case 4:
                    if (playInfo?.DashInfo?.Video != null && playInfo.DashInfo.Video.Height != 0)
                    {
                        playerElement.Stretch = Stretch.Fill;
                    }
                    else
                    {
                        playerElement.Stretch = Stretch.None;
                        playerElement.Width = double.NaN;
                        playerElement.Height = double.NaN;
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
