using System;
using System.IO;
using System.Threading.Tasks;
using BiliLite.Models.Common.Player;
using BiliLite.Player.MediaInfos;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BiliLite.Player.SubPlayers
{
    public class FlvSYEngineSubPlayer : ISubPlayer
    {
        private readonly Panel m_playerHost;
        private readonly bool m_useSharedPlayerElement;
        private MediaPlayerElement m_playerElement;
        private MediaPlayer m_mediaPlayer;
        private string m_url;
        private bool m_isBuffering;
        private bool m_userRequestedPlay;
        private bool m_waitingForPlayableStatePromotion;
        private double m_bufferCache;

        public FlvSYEngineSubPlayer(Panel playerHost, MediaPlayerElement sharedPlayerElement = null)
        {
            m_playerHost = playerHost;
            if (sharedPlayerElement != null)
            {
                m_playerElement = sharedPlayerElement;
                m_useSharedPlayerElement = true;
            }
        }

        public override RealPlayerType Type { get; } = RealPlayerType.Native;

        public override double Volume
        {
            get => m_mediaPlayer?.Volume ?? 1;
            set
            {
                if (m_mediaPlayer != null)
                {
                    m_mediaPlayer.Volume = value;
                }
            }
        }

        public override double Position => m_mediaPlayer?.PlaybackSession?.Position.TotalSeconds ?? 0;

        public override FrameworkElement PlayerView => m_playerElement;

        public override double Duration
        {
            get
            {
                var duration = m_mediaPlayer?.PlaybackSession?.NaturalDuration.TotalSeconds ?? 0;
                return duration > 0 ? duration : base.Duration;
            }
        }

        public override bool IsMuted
        {
            get => m_mediaPlayer?.IsMuted == true;
            set
            {
                if (m_mediaPlayer != null)
                {
                    m_mediaPlayer.IsMuted = value;
                }
            }
        }

        public override bool IsBuffering => m_isBuffering;

        public override double BufferCache => m_bufferCache;

        public override event EventHandler MediaOpened;
        public override event EventHandler MediaEnded;
        public override event EventHandler BufferingStarted;
        public override event EventHandler BufferingEnded;
        public override event EventHandler<double> PositionChanged;

        public override CollectInfo GetCollectInfo()
        {
            return new CollectInfo()
            {
                Data = new MediaPlayerCollectInfoData
                {
                    MediaPlayer = m_mediaPlayer,
                },
                RealPlayInfo = m_realPlayInfo,
                Type = "FlvSyEngine",
                Url = m_url,
            };
        }

        public override async Task Load()
        {
            if (string.IsNullOrEmpty(m_realPlayInfo?.SingleUrl))
            {
                EmitError(PlayerError.PlayerErrorCode.PlayUrlError, "FLV 播放地址为空", PlayerError.RetryStrategy.NoRetry);
                return;
            }

            m_url = m_realPlayInfo.SingleUrl;

            // 本地 FLV 无法通过 SYEngine 网络播放列表播放（MediaFailed 且无回落），
            // 本地文件直接报错并交给回落链切换 FFmpegInterop 播放器
            if (m_realPlayInfo.IsLocal || IsLocalPathOrFileUri(m_url))
            {
                EmitError(PlayerError.PlayerErrorCode.NeedUseOtherPlayerError,
                    "本地FLV暂不支持SYEngine播放器，正在尝试其他播放方式", PlayerError.RetryStrategy.Normal);
                return;
            }

            await StopCore();
            var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp)
            {
                NetworkConfigs = CreatePlaylistNetworkConfigs(m_realPlayInfo.UserAgent, m_realPlayInfo.Referer),
            };
            playList.Append(m_url, 0, 0);

            m_mediaPlayer = new MediaPlayer();
            m_mediaPlayer.AutoPlay = true;
            m_mediaPlayer.MediaOpened += MediaPlayerOnMediaOpened;
            m_mediaPlayer.MediaEnded += MediaPlayerOnMediaEnded;
            m_mediaPlayer.MediaFailed += MediaPlayerOnMediaFailed;
            m_mediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSessionOnPlaybackStateChanged;
            m_mediaPlayer.PlaybackSession.BufferingStarted += PlaybackSessionOnBufferingStarted;
            m_mediaPlayer.PlaybackSession.BufferingProgressChanged += PlaybackSessionOnBufferingProgressChanged;
            m_mediaPlayer.PlaybackSession.BufferingEnded += PlaybackSessionOnBufferingEnded;
            m_mediaPlayer.PlaybackSession.PositionChanged += PlaybackSessionOnPositionChanged;

            var mediaSource = await playList.SaveAndGetFileUriAsync();
            // 必须在赋值 Source 前把画面元素绑定到 MediaPlayer：
            // AutoPlay=true 会在 Source 赋值后立即开始播放，若播放时才绑定元素（Play() 内），
            // 未绑定时视频帧会被丢弃，表现为只有声音没有画面（黑屏）
            await RunOnUiThreadAsync(() =>
            {
                EnsurePlayerElement();
                AttachPlayerElement();
                if (m_playerElement.MediaPlayer != m_mediaPlayer)
                {
                    m_playerElement.SetMediaPlayer(m_mediaPlayer);
                }
            });
            m_mediaPlayer.Source = MediaSource.CreateFromUri(mediaSource);
            await SetRate(m_rate);
        }

        public override async Task Buff()
        {
        }

        public override async Task Play()
        {
            EnsurePlayerElement();
            AttachPlayerElement();
            if (m_playerElement.MediaPlayer != m_mediaPlayer)
            {
                m_playerElement.SetMediaPlayer(m_mediaPlayer);
            }

            // 用户显式请求播放后，不再拦截初次自动播放
            m_userRequestedPlay = true;
            m_mediaPlayer?.Play();
        }

        public override async Task Stop()
        {
            await StopCore();
        }

        public override async Task Fault()
        {
            await StopCore();
        }

        public override async Task Pause()
        {
            m_mediaPlayer?.Pause();
        }

        public override async Task Resume()
        {
            // 用户显式恢复播放后，不再拦截初次自动播放
            m_userRequestedPlay = true;
            m_mediaPlayer?.Play();
        }

        public override async Task SetRate(double value)
        {
            m_rate = value;
            if (m_mediaPlayer?.PlaybackSession != null)
            {
                m_mediaPlayer.PlaybackSession.PlaybackRate = value;
            }
        }

        public override async Task SetPosition(double value)
        {
            if (m_mediaPlayer?.PlaybackSession != null)
            {
                m_mediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(value);
            }
        }

        private async Task StopCore()
        {
            if (m_mediaPlayer == null)
            {
                return;
            }

            m_mediaPlayer.Pause();
            m_mediaPlayer.Source = null;
            m_mediaPlayer.MediaOpened -= MediaPlayerOnMediaOpened;
            m_mediaPlayer.MediaEnded -= MediaPlayerOnMediaEnded;
            m_mediaPlayer.MediaFailed -= MediaPlayerOnMediaFailed;
            m_mediaPlayer.PlaybackSession.BufferingStarted -= PlaybackSessionOnBufferingStarted;
            m_mediaPlayer.PlaybackSession.BufferingProgressChanged -= PlaybackSessionOnBufferingProgressChanged;
            m_mediaPlayer.PlaybackSession.BufferingEnded -= PlaybackSessionOnBufferingEnded;
            m_mediaPlayer.PlaybackSession.PositionChanged -= PlaybackSessionOnPositionChanged;
            m_mediaPlayer.PlaybackSession.PlaybackStateChanged -= PlaybackSessionOnPlaybackStateChanged;
            if (m_playerElement != null)
            {
                m_playerElement.SetMediaPlayer(null);
                if (!m_useSharedPlayerElement)
                {
                    m_playerHost?.Children.Remove(m_playerElement);
                }
            }
            m_mediaPlayer.Dispose();
            m_mediaPlayer = null;
            m_userRequestedPlay = false;
        }

        private void PlaybackSessionOnPositionChanged(MediaPlaybackSession sender, object args)
        {
            PositionChanged?.Invoke(this, sender?.Position.TotalSeconds ?? 0);
        }

        private static SYEngine.PlaylistNetworkConfigs CreatePlaylistNetworkConfigs(string userAgent, string referer)
        {
            var config = new SYEngine.PlaylistNetworkConfigs()
            {
                DownloadRetryOnFail = true,
                HttpCookie = string.Empty,
                UniqueId = string.Empty,
                HttpReferer = string.Empty,
                HttpUserAgent = string.Empty,
            };

            if (!string.IsNullOrWhiteSpace(userAgent))
            {
                config.HttpUserAgent = userAgent;
            }

            if (!string.IsNullOrWhiteSpace(referer))
            {
                config.HttpReferer = referer;
            }

            return config;
        }

        private void PlaybackSessionOnBufferingStarted(MediaPlaybackSession sender, object args)
        {
            m_isBuffering = true;
            m_waitingForPlayableStatePromotion = true;
            BufferingStarted?.Invoke(this, EventArgs.Empty);
        }

        private void PlaybackSessionOnPlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            // 初次自动播放拦截：不改变 AutoPlay 属性，媒体保持自动预加载并渲染首帧，
            // 但在用户未请求播放（未点击播放、未开启自动播放）时，进入播放态后立即暂停，
            // 避免打开视频页时自动出声。
            if (sender?.PlaybackState == MediaPlaybackState.Playing)
            {
                if (m_realPlayInfo?.IsAutoPlay != true && !m_userRequestedPlay)
                {
                    m_userRequestedPlay = true;
                    m_mediaPlayer?.Pause();
                }
            }

            // 与 FlvFFmpegInteropSubPlayer 相同的兜底：
            // 底层已可播（Playing/Paused 且已打开）但未触发 BufferingEnded 时补发一次，
            // 避免外层一直停留在缓冲态、无法调用 Play() 绑定画面
            TryPromotePlayableStateAfterBuffering(sender, "PlaybackStateChanged");
        }

        private void PlaybackSessionOnBufferingProgressChanged(MediaPlaybackSession sender, object args)
        {
            m_bufferCache = sender?.BufferingProgress ?? 0;
            EmitBufferCacheChanged(m_bufferCache);
        }

        private void PlaybackSessionOnBufferingEnded(MediaPlaybackSession sender, object args)
        {
            m_isBuffering = false;
            m_bufferCache = 1;
            m_waitingForPlayableStatePromotion = false;
            EmitBufferCacheChanged(m_bufferCache);
            BufferingEnded?.Invoke(this, EventArgs.Empty);
        }

        public override async Task SetRatioMode(int mode)
        {
            EnsurePlayerElement();
            VideoPlayer.ApplyStretch(m_playerElement, m_realPlayInfo, mode);
        }

        public override async Task SetVideoEnable(bool enable)
        {
            EnsurePlayerElement();
            m_playerElement.Visibility = enable ? Visibility.Visible : Visibility.Collapsed;
            if (!enable)
            {
                m_mediaPlayer?.Pause();
            }
        }

        public override async Task<byte[]> CaptureAsync()
        {
            EnsurePlayerElement();
            return await VideoPlayer.RenderElementToPngBytesAsync(m_playerElement, 96);
        }

        private void EnsurePlayerElement()
        {
            if (m_playerElement != null)
            {
                return;
            }

            m_playerElement = new MediaPlayerElement
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Width = double.NaN,
                Height = double.NaN,
            };
        }

        private void AttachPlayerElement()
        {
            EnsurePlayerElement();
            if (m_playerElement.Parent == m_playerHost)
            {
                return;
            }

            if (m_playerElement.Parent is Panel oldParent)
            {
                oldParent.Children.Remove(m_playerElement);
            }

            m_playerHost?.Children.Insert(0, m_playerElement);
        }

        private void MediaPlayerOnMediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            var desc = string.IsNullOrEmpty(args.ErrorMessage) ? "SYEngine 播放器播放失败" : args.ErrorMessage;
            EmitError(PlayerError.PlayerErrorCode.UnknownError, desc, PlayerError.RetryStrategy.NoRetry);
        }

        private void MediaPlayerOnMediaEnded(MediaPlayer sender, object args)
        {
            MediaEnded?.Invoke(this, EventArgs.Empty);
        }

        private void MediaPlayerOnMediaOpened(MediaPlayer sender, object args)
        {
            MediaOpened?.Invoke(this, EventArgs.Empty);
            SchedulePlayableStatePromotionAfterMediaOpened(sender?.PlaybackSession);
        }

        private void TryPromotePlayableStateAfterBuffering(MediaPlaybackSession session, string source)
        {
            if (!m_waitingForPlayableStatePromotion || session == null)
            {
                return;
            }

            var hasOpened = session.NaturalDuration > TimeSpan.Zero;
            var playbackState = session.PlaybackState;
            var isPlayableState = playbackState == MediaPlaybackState.Playing ||
                                  playbackState == MediaPlaybackState.Paused;
            if (!hasOpened || !isPlayableState)
            {
                return;
            }

            PlaybackSessionOnBufferingEnded(session, EventArgs.Empty);
        }

        private void SchedulePlayableStatePromotionAfterMediaOpened(MediaPlaybackSession session)
        {
            _ = RunOnUiThreadAsync(async () =>
            {
                await Task.Yield();

                var playbackSession = m_mediaPlayer?.PlaybackSession ?? session;
                if (playbackSession == null)
                {
                    return;
                }

                if (playbackSession.PlaybackState == MediaPlaybackState.None)
                {
                    playbackSession = session;
                }

                TryPromotePlayableStateAfterBuffering(playbackSession, "MediaOpened");
            });
        }

        private static bool IsLocalPathOrFileUri(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            if (Path.IsPathRooted(url))
            {
                return true;
            }

            return Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.IsFile;
        }

        private async Task RunOnUiThreadAsync(Action action)
        {
            if (action == null)
            {
                return;
            }

            var dispatcher = m_playerHost?.Dispatcher ?? m_playerElement?.Dispatcher;
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

        private async Task RunOnUiThreadAsync(Func<Task> action)
        {
            if (action == null)
            {
                return;
            }

            Task innerTask = null;
            await RunOnUiThreadAsync(() =>
            {
                innerTask = action();
            });

            if (innerTask != null)
            {
                await innerTask;
            }
        }
    }
}
