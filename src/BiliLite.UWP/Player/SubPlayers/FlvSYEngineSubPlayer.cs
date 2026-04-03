using System;
using System.Threading.Tasks;
using BiliLite.Models.Common.Player;
using BiliLite.Player.MediaInfos;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BiliLite.Player.SubPlayers
{
    public class FlvSYEngineSubPlayer : ISubPlayer
    {
        private readonly MediaPlayerElement m_playerElement;
        private MediaPlayer m_mediaPlayer;
        private string m_url;
        private bool m_isBuffering;
        private double m_bufferCache;

        public FlvSYEngineSubPlayer(MediaPlayerElement playerElement)
        {
            m_playerElement = playerElement;
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
            m_mediaPlayer.PlaybackSession.BufferingStarted += PlaybackSessionOnBufferingStarted;
            m_mediaPlayer.PlaybackSession.BufferingProgressChanged += PlaybackSessionOnBufferingProgressChanged;
            m_mediaPlayer.PlaybackSession.BufferingEnded += PlaybackSessionOnBufferingEnded;
            m_mediaPlayer.PlaybackSession.PositionChanged += PlaybackSessionOnPositionChanged;

            var mediaSource = await playList.SaveAndGetFileUriAsync();
            m_mediaPlayer.Source = MediaSource.CreateFromUri(mediaSource);
            await SetRate(m_rate);
        }

        public override async Task Buff()
        {
        }

        public override async Task Play()
        {
            if (m_playerElement.MediaPlayer != m_mediaPlayer)
            {
                m_playerElement.SetMediaPlayer(m_mediaPlayer);
            }

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
            m_playerElement.SetMediaPlayer(null);
            m_mediaPlayer.Dispose();
            m_mediaPlayer = null;
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
            BufferingStarted?.Invoke(this, EventArgs.Empty);
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
            EmitBufferCacheChanged(m_bufferCache);
            BufferingEnded?.Invoke(this, EventArgs.Empty);
        }

        public override async Task SetRatioMode(int mode)
        {
            VideoPlayer.ApplyStretch(m_playerElement, m_realPlayInfo, mode);
        }

        public override async Task SetVideoEnable(bool enable)
        {
            m_playerElement.Visibility = enable ? Visibility.Visible : Visibility.Collapsed;
            if (!enable)
            {
                m_mediaPlayer?.Pause();
            }
        }

        public override async Task<byte[]> CaptureAsync()
        {
            return await VideoPlayer.RenderElementToPngBytesAsync(m_playerElement, 96);
        }

        private void MediaPlayerOnMediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            EmitError(PlayerError.PlayerErrorCode.UnknownError, args.ErrorMessage, PlayerError.RetryStrategy.NoRetry);
        }

        private void MediaPlayerOnMediaEnded(MediaPlayer sender, object args)
        {
            MediaEnded?.Invoke(this, EventArgs.Empty);
        }

        private void MediaPlayerOnMediaOpened(MediaPlayer sender, object args)
        {
            MediaOpened?.Invoke(this, EventArgs.Empty);
        }
    }
}
