using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BiliLite.Models.Common.Player;
using BiliLite.Models.Common.Video.PlayUrlInfos;
using BiliLite.Player.MediaInfos;
using BiliLite.Services;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.Streaming.Adaptive;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;

namespace BiliLite.Player.SubPlayers
{
    public class DashNativeSubPlayer : ISubPlayer
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly MediaPlayerElement m_playerElement;
        private MediaPlayer m_mediaPlayer;
        private string m_url;
        private bool m_isBuffering;
        private double m_bufferCache;

        public DashNativeSubPlayer(MediaPlayerElement playerElement)
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
                Type = "DashNative",
                Url = m_url,
            };
        }

        public override async Task Load()
        {
            if (m_realPlayInfo?.DashInfo?.Video?.Url == null)
            {
                EmitError(PlayerError.PlayerErrorCode.PlayUrlError, "Dash 视频地址为空", PlayerError.RetryStrategy.NoRetry);
                return;
            }

            m_url = m_realPlayInfo.DashInfo.Video.Url;
            _logger.Info(
                $"DashNative.Load start: isLocal={m_realPlayInfo?.IsLocal}, videoUrl={SanitizeUrl(m_realPlayInfo?.DashInfo?.Video?.Url)}, audioUrl={SanitizeUrl(m_realPlayInfo?.DashInfo?.Audio?.Url)}");
            await StopCore();

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

            var adaptiveMediaSource = await CreateAdaptiveMediaSource(m_realPlayInfo.DashInfo, m_realPlayInfo.UserAgent, m_realPlayInfo.Referer);
            if (adaptiveMediaSource != null)
            {
                if (m_realPlayInfo.IsLocal)
                {
                    var videoFile = await Windows.Storage.StorageFile.GetFileFromPathAsync(m_url);
                    var videoBasicProperties = await videoFile.GetBasicPropertiesAsync();
                    var audioPath = m_realPlayInfo.DashInfo.Audio?.Url;
                    _logger.Info(
                        $"DashNative.Load local dash source: videoPath={videoFile.Path}, videoType={videoFile.FileType}, videoSize={videoBasicProperties.Size}, audioUrl={SanitizeUrl(audioPath)}");
                }

                m_mediaPlayer.Source = MediaSource.CreateFromAdaptiveMediaSource(adaptiveMediaSource);
            }
            else if (m_realPlayInfo.IsLocal)
            {
                _logger.Error($"DashNative 本地 DASH 创建 AdaptiveMediaSource 失败，无法继续 Native 播放: videoUrl={SanitizeUrl(m_url)}, audioUrl={SanitizeUrl(m_realPlayInfo.DashInfo.Audio?.Url)}");
                EmitError(PlayerError.PlayerErrorCode.NeedUseOtherPlayerError, "本地 DASH Native 初始化失败", PlayerError.RetryStrategy.Normal);
                return;
            }
            else
            {
                // 兼容兜底：远程 MPD 组装失败时回退到直连视频流。
                _logger.Warn($"DashNative 自建 MPD 失败，回退直连视频流: url={SanitizeUrl(m_url)}");
                m_mediaPlayer.Source = MediaSource.CreateFromUri(new Uri(m_url));
            }

            _logger.Info($"DashNative.Load source assigned: isLocal={m_realPlayInfo?.IsLocal}, sourceNull={m_mediaPlayer.Source == null}");

            await SetRate(m_rate);
        }

        private async Task<AdaptiveMediaSource> CreateAdaptiveMediaSource(BiliDashPlayUrlInfo dashInfo, string userAgent, string referer)
        {
            try
            {
                var httpClient = new HttpClient();
                if (!string.IsNullOrWhiteSpace(userAgent))
                {
                    httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
                }

                if (!string.IsNullOrWhiteSpace(referer))
                {
                    httpClient.DefaultRequestHeaders.Add("Referer", referer);
                }

                var mpdStr = BuildDashMpd(dashInfo);
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(mpdStr)).AsInputStream();
                var source = await AdaptiveMediaSource.CreateFromStreamAsync(
                    stream,
                    new Uri(dashInfo.Video.Url),
                    "application/dash+xml",
                    httpClient);

                if (source.Status != AdaptiveMediaSourceCreationStatus.Success)
                {
                    _logger.Warn($"DashNative 创建 AdaptiveMediaSource 失败: status={source.Status}, url={SanitizeUrl(dashInfo?.Video?.Url)}");
                    return null;
                }

                source.MediaSource.AdvancedSettings.AllSegmentsIndependent = true;
                if (!string.IsNullOrWhiteSpace(dashInfo.Audio?.Url))
                {
                    source.MediaSource.DownloadRequested += (sender, args) =>
                    {
                        if (args.ResourceContentType == "audio/mp4")
                        {
                            args.Result.ResourceUri = new Uri(dashInfo.Audio.Url);
                        }
                    };
                }

                return source.MediaSource;
            }
            catch (Exception ex)
            {
                _logger.Error($"DashNative 创建 AdaptiveMediaSource 异常: url={SanitizeUrl(dashInfo?.Video?.Url)}", ex);
                return null;
            }
        }

        private static string SanitizeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return "null";
            }

            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return $"{uri.Scheme}://{uri.Host}{uri.AbsolutePath}";
            }

            return "invalid_url";
        }

        private static string BuildDashMpd(BiliDashPlayUrlInfo dashInfo)
        {
            if (!string.IsNullOrWhiteSpace(dashInfo?.Audio?.Url))
            {
                return $@"<MPD xmlns=""urn:mpeg:DASH:schema:MPD:2011"" profiles=""urn:mpeg:dash:profile:isoff-on-demand:2011"" type=""static"">
  <Period start=""PT0S"">
    <AdaptationSet>
      <ContentComponent contentType=""video"" id=""1"" />
      <Representation bandwidth=""{dashInfo.Video.BandWidth}"" codecs=""{dashInfo.Video.Codecs}"" height=""{dashInfo.Video.Height}"" id=""{dashInfo.Video.ID}"" mimeType=""{dashInfo.Video.MimeType}"" width=""{dashInfo.Video.Width}"" startWithSap=""{dashInfo.Video.StartWithSap}"">
        <BaseURL></BaseURL>
        <SegmentBase indexRange=""{dashInfo.Video.SegmentBaseIndexRange}"">
          <Initialization range=""{dashInfo.Video.SegmentBaseInitialization}"" />
        </SegmentBase>
      </Representation>
    </AdaptationSet>
    <AdaptationSet>
      <ContentComponent contentType=""audio"" id=""2"" />
      <Representation bandwidth=""{dashInfo.Audio.BandWidth}"" codecs=""{dashInfo.Audio.Codecs}"" id=""{dashInfo.Audio.ID}"" mimeType=""{dashInfo.Audio.MimeType}"">
        <BaseURL></BaseURL>
        <SegmentBase indexRange=""{dashInfo.Audio.SegmentBaseIndexRange}"">
          <Initialization range=""{dashInfo.Audio.SegmentBaseInitialization}"" />
        </SegmentBase>
      </Representation>
    </AdaptationSet>
  </Period>
</MPD>";
            }

            return $@"<MPD xmlns=""urn:mpeg:DASH:schema:MPD:2011"" profiles=""urn:mpeg:dash:profile:isoff-on-demand:2011"" type=""static"">
  <Period start=""PT0S"">
    <AdaptationSet>
      <ContentComponent contentType=""video"" id=""1"" />
      <Representation bandwidth=""{dashInfo.Video.BandWidth}"" codecs=""{dashInfo.Video.Codecs}"" height=""{dashInfo.Video.Height}"" id=""{dashInfo.Video.ID}"" mimeType=""{dashInfo.Video.MimeType}"" width=""{dashInfo.Video.Width}"" startWithSap=""{dashInfo.Video.StartWithSap}"">
        <BaseURL></BaseURL>
        <SegmentBase indexRange=""{dashInfo.Video.SegmentBaseIndexRange}"">
          <Initialization range=""{dashInfo.Video.SegmentBaseInitialization}"" />
        </SegmentBase>
      </Representation>
    </AdaptationSet>
  </Period>
</MPD>";
        }

        public override async Task Buff()
        {
        }

        public override async Task Play()
        {
            await RunOnUiThreadAsync(() =>
            {
                _logger.Info(
                    $"DashNative.Play: elementHasPlayer={m_playerElement.MediaPlayer != null}, samePlayer={ReferenceEquals(m_playerElement.MediaPlayer, m_mediaPlayer)}, visibility={m_playerElement.Visibility}, width={m_playerElement.ActualWidth}, height={m_playerElement.ActualHeight}");
                if (m_playerElement.MediaPlayer != m_mediaPlayer)
                {
                    m_playerElement.SetMediaPlayer(m_mediaPlayer);
                    _logger.Info("DashNative.Play: MediaPlayerElement.SetMediaPlayer completed");
                }

                m_mediaPlayer?.Play();
                _logger.Info($"DashNative.Play: playbackState={m_mediaPlayer?.PlaybackSession?.PlaybackState}, autoPlay={m_mediaPlayer?.AutoPlay}");
            });
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
            await RunOnUiThreadAsync(() => m_playerElement.SetMediaPlayer(null));
            m_mediaPlayer.Dispose();
            m_mediaPlayer = null;
        }

        private void PlaybackSessionOnPositionChanged(MediaPlaybackSession sender, object args)
        {
            var position = sender?.Position.TotalSeconds ?? 0;
            PositionChanged?.Invoke(this, position);
        }

        private void PlaybackSessionOnPlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            _logger.Info(
                $"DashNative.PlaybackStateChanged: state={sender?.PlaybackState}, position={sender?.Position.TotalSeconds}, naturalDuration={sender?.NaturalDuration.TotalSeconds}, buffer={sender?.BufferingProgress}");
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
            await RunOnUiThreadAsync(() => VideoPlayer.ApplyStretch(m_playerElement, m_realPlayInfo, mode));
        }

        public override async Task SetVideoEnable(bool enable)
        {
            await RunOnUiThreadAsync(() =>
            {
                m_playerElement.Visibility = enable ? Visibility.Visible : Visibility.Collapsed;
                if (!enable)
                {
                    m_mediaPlayer?.Pause();
                }
            });
        }

        public override async Task<byte[]> CaptureAsync()
        {
            byte[] result = null;
            await RunOnUiThreadAsync(async () =>
            {
                result = await VideoPlayer.RenderElementToPngBytesAsync(m_playerElement, 96);
            });
            return result;
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

        private static async Task RunOnUiThreadAsync(Func<Task> action)
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

        private void MediaPlayerOnMediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            _logger.Error(
                $"DashNative MediaFailed: error={args?.Error}, errorMessage={args?.ErrorMessage}, extendedErrorCode={args?.ExtendedErrorCode}, url={SanitizeUrl(m_url)}");
            EmitError(PlayerError.PlayerErrorCode.NeedUseOtherPlayerError, args.ErrorMessage, PlayerError.RetryStrategy.Normal);
        }

        private void MediaPlayerOnMediaEnded(MediaPlayer sender, object args)
        {
            MediaEnded?.Invoke(this, EventArgs.Empty);
        }

        private void MediaPlayerOnMediaOpened(MediaPlayer sender, object args)
        {
            _logger.Info(
                $"DashNative.MediaOpened: video={sender?.PlaybackSession?.NaturalVideoWidth}x{sender?.PlaybackSession?.NaturalVideoHeight}, duration={sender?.PlaybackSession?.NaturalDuration.TotalSeconds}, position={sender?.PlaybackSession?.Position.TotalSeconds}, elementVisibility={m_playerElement.Visibility}, elementSize={m_playerElement.ActualWidth}x{m_playerElement.ActualHeight}");
            MediaOpened?.Invoke(this, EventArgs.Empty);
        }
    }
}
