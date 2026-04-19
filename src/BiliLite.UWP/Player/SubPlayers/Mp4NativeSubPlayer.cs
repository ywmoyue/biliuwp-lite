using System;
using System.Threading.Tasks;
using BiliLite.Models.Common.Player;
using BiliLite.Player.MediaInfos;
using BiliLite.Services;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BiliLite.Player.SubPlayers
{
    public class Mp4NativeSubPlayer : ISubPlayer
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly Panel m_playerHost;
        private MediaPlayerElement m_playerElement;
        private MediaPlayer m_mediaPlayer;
        private string m_url;
        private bool m_isBuffering;
        private double m_bufferCache;

        public Mp4NativeSubPlayer(Panel playerHost)
        {
            m_playerHost = playerHost;
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
                Type = "Mp4Native",
                Url = m_url,
            };
        }

        public override async Task Load()
        {
            if (string.IsNullOrEmpty(m_realPlayInfo?.SingleUrl))
            {
                EmitError(PlayerError.PlayerErrorCode.PlayUrlError, "MP4 播放地址为空", PlayerError.RetryStrategy.NoRetry);
                return;
            }

            m_url = m_realPlayInfo.SingleUrl;
            _logger.Info($"Mp4Native.Load start: isLocal={m_realPlayInfo?.IsLocal}, url={SanitizeUrl(m_url)}");
            await StopCore();

            m_mediaPlayer = new MediaPlayer();
            m_mediaPlayer.AutoPlay = m_realPlayInfo?.IsAutoPlay == true;
            m_mediaPlayer.MediaOpened += MediaPlayerOnMediaOpened;
            m_mediaPlayer.MediaEnded += MediaPlayerOnMediaEnded;
            m_mediaPlayer.MediaFailed += MediaPlayerOnMediaFailed;
            m_mediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSessionOnPlaybackStateChanged;
            m_mediaPlayer.PlaybackSession.BufferingStarted += PlaybackSessionOnBufferingStarted;
            m_mediaPlayer.PlaybackSession.BufferingProgressChanged += PlaybackSessionOnBufferingProgressChanged;
            m_mediaPlayer.PlaybackSession.BufferingEnded += PlaybackSessionOnBufferingEnded;
            m_mediaPlayer.PlaybackSession.PositionChanged += PlaybackSessionOnPositionChanged;

            if (m_realPlayInfo.IsLocal)
            {
                var file = await StorageFile.GetFileFromPathAsync(m_url);
                var basicProperties = await file.GetBasicPropertiesAsync();
                _logger.Info($"Mp4Native.Load local source: path={file.Path}, type={file.FileType}, size={basicProperties.Size}");
                m_mediaPlayer.Source = MediaSource.CreateFromStorageFile(file);
            }
            else
            {
                m_mediaPlayer.Source = MediaSource.CreateFromUri(new Uri(m_url));
            }

            _logger.Info($"Mp4Native.Load source assigned: isLocal={m_realPlayInfo?.IsLocal}, sourceNull={m_mediaPlayer.Source == null}");

            await SetRate(m_rate);
        }

        public override async Task Buff()
        {
        }

        public override async Task Play()
        {
            EnsurePlayerElement();
            _logger.Info(
                $"Mp4Native.Play: elementHasPlayer={m_playerElement.MediaPlayer != null}, samePlayer={ReferenceEquals(m_playerElement.MediaPlayer, m_mediaPlayer)}, visibility={m_playerElement.Visibility}, width={m_playerElement.ActualWidth}, height={m_playerElement.ActualHeight}");
            AttachPlayerElement();
            if (m_playerElement.MediaPlayer != m_mediaPlayer)
            {
                m_playerElement.SetMediaPlayer(m_mediaPlayer);
                _logger.Info("Mp4Native.Play: MediaPlayerElement.SetMediaPlayer completed");
            }

            m_mediaPlayer?.Play();
            _logger.Info($"Mp4Native.Play: playbackState={m_mediaPlayer?.PlaybackSession?.PlaybackState}, autoPlay={m_mediaPlayer?.AutoPlay}");
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
            m_mediaPlayer.PlaybackSession.PlaybackStateChanged -= PlaybackSessionOnPlaybackStateChanged;
            m_mediaPlayer.PlaybackSession.BufferingStarted -= PlaybackSessionOnBufferingStarted;
            m_mediaPlayer.PlaybackSession.BufferingProgressChanged -= PlaybackSessionOnBufferingProgressChanged;
            m_mediaPlayer.PlaybackSession.BufferingEnded -= PlaybackSessionOnBufferingEnded;
            m_mediaPlayer.PlaybackSession.PositionChanged -= PlaybackSessionOnPositionChanged;
            if (m_playerElement != null)
            {
                m_playerElement.SetMediaPlayer(null);
                m_playerHost?.Children.Remove(m_playerElement);
            }
            m_mediaPlayer.Dispose();
            m_mediaPlayer = null;
        }

        private void PlaybackSessionOnPositionChanged(MediaPlaybackSession sender, object args)
        {
            PositionChanged?.Invoke(this, sender?.Position.TotalSeconds ?? 0);
        }

        private void PlaybackSessionOnPlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            var buffer = TryGetBufferingProgress(sender);
            _logger.Info(
                $"Mp4Native.PlaybackStateChanged: state={sender?.PlaybackState}, position={sender?.Position.TotalSeconds}, naturalDuration={sender?.NaturalDuration.TotalSeconds}, buffer={buffer}");
        }

        private void PlaybackSessionOnBufferingStarted(MediaPlaybackSession sender, object args)
        {
            m_isBuffering = true;
            BufferingStarted?.Invoke(this, EventArgs.Empty);
        }

        private void PlaybackSessionOnBufferingProgressChanged(MediaPlaybackSession sender, object args)
        {
            m_bufferCache = TryGetBufferingProgress(sender);
            EmitBufferCacheChanged(m_bufferCache);
        }

        private static double TryGetBufferingProgress(MediaPlaybackSession session)
        {
            if (session == null)
            {
                return 0;
            }

            try
            {
                return session.BufferingProgress;
            }
            catch
            {
                return 0;
            }
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

        private void MediaPlayerOnMediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            _logger.Error(
                $"Mp4Native.MediaFailed: error={args?.Error}, errorMessage={args?.ErrorMessage}, extendedErrorCode={args?.ExtendedErrorCode}, url={SanitizeUrl(m_url)}");
            EmitError(PlayerError.PlayerErrorCode.UnknownError, args.ErrorMessage, PlayerError.RetryStrategy.NoRetry);
        }

        private void MediaPlayerOnMediaEnded(MediaPlayer sender, object args)
        {
            MediaEnded?.Invoke(this, EventArgs.Empty);
        }

        private void MediaPlayerOnMediaOpened(MediaPlayer sender, object args)
        {
            var elementVisibility = m_playerElement?.Visibility.ToString() ?? "null";
            var elementWidth = m_playerElement?.ActualWidth ?? 0;
            var elementHeight = m_playerElement?.ActualHeight ?? 0;
            _logger.Info(
                $"Mp4Native.MediaOpened: video={sender?.PlaybackSession?.NaturalVideoWidth}x{sender?.PlaybackSession?.NaturalVideoHeight}, duration={sender?.PlaybackSession?.NaturalDuration.TotalSeconds}, position={sender?.PlaybackSession?.Position.TotalSeconds}, elementVisibility={elementVisibility}, elementSize={elementWidth}x{elementHeight}");
            MediaOpened?.Invoke(this, EventArgs.Empty);
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

        private static string SanitizeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return "null";
            }

            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return uri.IsFile ? uri.LocalPath : $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}";
            }

            return url;
        }
    }
}
