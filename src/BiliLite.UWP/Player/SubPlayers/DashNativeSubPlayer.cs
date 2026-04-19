using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using BiliLite.Models.Common.Player;
using BiliLite.Models.Common.Video.PlayUrlInfos;
using BiliLite.Player.MediaInfos;
using BiliLite.Services;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.Streaming.Adaptive;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;
using Windows.Storage;

namespace BiliLite.Player.SubPlayers
{
    public class DashNativeSubPlayer : ISubPlayer
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly Panel m_playerHost;
        private MediaPlayerElement m_playerElement;
        private MediaPlayer m_mediaPlayer;
        private MediaPlayer m_audioPlayer;
        private MediaTimelineController m_timelineController;
        private string m_url;
        private bool m_isBuffering;
        private double m_bufferCache;
        private double m_volume = 1;
        private bool m_isMuted;

        public DashNativeSubPlayer(Panel playerHost)
        {
            m_playerHost = playerHost;
        }

        public override RealPlayerType Type { get; } = RealPlayerType.Native;

        public override double Volume
        {
            get => m_mediaPlayer?.Volume ?? m_audioPlayer?.Volume ?? m_volume;
            set
            {
                m_volume = value;
                if (m_mediaPlayer != null)
                {
                    m_mediaPlayer.Volume = value;
                }

                if (m_audioPlayer != null)
                {
                    m_audioPlayer.Volume = value;
                }
            }
        }

        public override double Position => m_timelineController?.Position.TotalSeconds
            ?? m_mediaPlayer?.PlaybackSession?.Position.TotalSeconds
            ?? m_audioPlayer?.PlaybackSession?.Position.TotalSeconds
            ?? 0;

        public override FrameworkElement PlayerView => m_playerElement;

        public override double Duration
        {
            get
            {
                var duration = m_mediaPlayer?.PlaybackSession?.NaturalDuration.TotalSeconds
                               ?? m_audioPlayer?.PlaybackSession?.NaturalDuration.TotalSeconds
                               ?? 0;
                return duration > 0 ? duration : base.Duration;
            }
        }

        public override bool IsMuted
        {
            get => m_isMuted;
            set
            {
                m_isMuted = value;
                if (m_mediaPlayer != null)
                {
                    m_mediaPlayer.IsMuted = value;
                }

                if (m_audioPlayer != null)
                {
                    m_audioPlayer.IsMuted = value;
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

            var audioUrl = m_realPlayInfo.DashInfo.Audio?.Url;
            var shouldUseDualPlayers = !string.IsNullOrWhiteSpace(audioUrl)
                && (m_realPlayInfo.IsLocal || IsLocalPathOrFileUri(audioUrl));
            if (shouldUseDualPlayers)
            {
                await LoadDashWithDualPlayersAsync();
                await SetRate(m_rate);
                return;
            }

            m_mediaPlayer = CreateVideoPlayer(autoPlay: m_realPlayInfo?.IsAutoPlay == true, trackBuffering: true);

            if (m_realPlayInfo.IsLocal)
            {
                var videoFile = await LoadLocalFileAsync(m_url);
                var videoBasicProperties = await videoFile.GetBasicPropertiesAsync();
                _logger.Info(
                    $"DashNative.Load local single source: videoPath={videoFile.Path}, videoType={videoFile.FileType}, videoSize={videoBasicProperties.Size}, audioUrl={SanitizeUrl(m_realPlayInfo.DashInfo.Audio?.Url)}");
                m_mediaPlayer.Source = MediaSource.CreateFromStorageFile(videoFile);
            }
            else
            {
                var adaptiveMediaSource = await CreateAdaptiveMediaSource(m_realPlayInfo.DashInfo, m_realPlayInfo.UserAgent, m_realPlayInfo.Referer);
                if (adaptiveMediaSource != null)
                {
                    m_mediaPlayer.Source = MediaSource.CreateFromAdaptiveMediaSource(adaptiveMediaSource);
                }
                else
                {
                    _logger.Warn($"DashNative 自建 MPD 失败，回退直连视频流: url={SanitizeUrl(m_url)}");
                    m_mediaPlayer.Source = MediaSource.CreateFromUri(new Uri(m_url));
                }
            }

            _logger.Info($"DashNative.Load source assigned: isLocal={m_realPlayInfo?.IsLocal}, sourceNull={m_mediaPlayer.Source == null}");

            Volume = m_volume;
            IsMuted = m_isMuted;
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
                EnsurePlayerElement();
                _logger.Info(
                    $"DashNative.Play: elementHasPlayer={m_playerElement.MediaPlayer != null}, samePlayer={ReferenceEquals(m_playerElement.MediaPlayer, m_mediaPlayer)}, hasAudioPlayer={m_audioPlayer != null}, visibility={m_playerElement.Visibility}, width={m_playerElement.ActualWidth}, height={m_playerElement.ActualHeight}");
                AttachPlayerElement();
                if (m_playerElement.MediaPlayer != m_mediaPlayer)
                {
                    m_playerElement.SetMediaPlayer(m_mediaPlayer);
                    _logger.Info("DashNative.Play: MediaPlayerElement.SetMediaPlayer completed");
                }

                if (m_timelineController != null)
                {
                    if (m_timelineController.State == MediaTimelineControllerState.Paused)
                    {
                        m_timelineController.Resume();
                    }
                    else
                    {
                        m_timelineController.Start();
                    }
                }
                else
                {
                    m_mediaPlayer?.Play();
                }

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
            if (m_timelineController != null)
            {
                m_timelineController.Pause();
                return;
            }

            m_mediaPlayer?.Pause();
        }

        public override async Task Resume()
        {
            await RunOnUiThreadAsync(() =>
            {
                EnsurePlayerElement();
                AttachPlayerElement();
                if (m_playerElement.MediaPlayer != m_mediaPlayer)
                {
                    m_playerElement.SetMediaPlayer(m_mediaPlayer);
                }
            });

            if (m_timelineController != null)
            {
                if (m_timelineController.State == MediaTimelineControllerState.Paused)
                {
                    m_timelineController.Resume();
                }
                else
                {
                    m_timelineController.Start();
                }

                return;
            }

            m_mediaPlayer?.Play();
        }

        public override async Task SetRate(double value)
        {
            m_rate = value;
            if (m_timelineController != null)
            {
                m_timelineController.ClockRate = value;
                return;
            }

            if (m_mediaPlayer?.PlaybackSession != null)
            {
                m_mediaPlayer.PlaybackSession.PlaybackRate = value;
            }
        }

        public override async Task SetPosition(double value)
        {
            if (m_timelineController != null)
            {
                m_timelineController.Position = TimeSpan.FromSeconds(value);
                return;
            }

            if (m_mediaPlayer?.PlaybackSession != null)
            {
                m_mediaPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(value);
            }
        }

        private async Task StopCore()
        {
            var hasTimelineController = m_timelineController != null;
            if (m_timelineController != null)
            {
                m_timelineController.Pause();
            }

            if (m_audioPlayer != null)
            {
                if (hasTimelineController)
                {
                    m_audioPlayer.TimelineController = null;
                }
                else
                {
                    m_audioPlayer.Pause();
                }

                m_audioPlayer.Source = null;
                m_audioPlayer.MediaFailed -= AudioPlayerOnMediaFailed;
                m_audioPlayer.PlaybackSession.BufferingStarted -= PlaybackSessionOnBufferingStarted;
                m_audioPlayer.PlaybackSession.BufferingProgressChanged -= PlaybackSessionOnBufferingProgressChanged;
                m_audioPlayer.PlaybackSession.BufferingEnded -= PlaybackSessionOnBufferingEnded;
                m_audioPlayer.Dispose();
                m_audioPlayer = null;
            }

            if (m_mediaPlayer != null)
            {
                if (hasTimelineController)
                {
                    m_mediaPlayer.TimelineController = null;
                }
                else
                {
                    m_mediaPlayer.Pause();
                }

                m_mediaPlayer.Source = null;
                m_mediaPlayer.MediaOpened -= MediaPlayerOnMediaOpened;
                m_mediaPlayer.MediaEnded -= MediaPlayerOnMediaEnded;
                m_mediaPlayer.MediaFailed -= MediaPlayerOnMediaFailed;
                m_mediaPlayer.PlaybackSession.PlaybackStateChanged -= PlaybackSessionOnPlaybackStateChanged;
                m_mediaPlayer.PlaybackSession.BufferingStarted -= PlaybackSessionOnBufferingStarted;
                m_mediaPlayer.PlaybackSession.BufferingProgressChanged -= PlaybackSessionOnBufferingProgressChanged;
                m_mediaPlayer.PlaybackSession.BufferingEnded -= PlaybackSessionOnBufferingEnded;
                m_mediaPlayer.PlaybackSession.PositionChanged -= PlaybackSessionOnPositionChanged;
                m_mediaPlayer.Dispose();
                m_mediaPlayer = null;
            }

            m_timelineController = null;
            m_isBuffering = false;
            m_bufferCache = 0;
            await RunOnUiThreadAsync(() =>
            {
                if (m_playerElement == null)
                {
                    return;
                }

                m_playerElement.SetMediaPlayer(null);
                m_playerHost?.Children.Remove(m_playerElement);
            });
        }

        private void PlaybackSessionOnPositionChanged(MediaPlaybackSession sender, object args)
        {
            var position = sender?.Position.TotalSeconds ?? 0;
            PositionChanged?.Invoke(this, position);
        }

        private void PlaybackSessionOnPlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            var buffer = TryGetBufferingProgress(sender);
            _logger.Info(
                $"DashNative.PlaybackStateChanged: state={sender?.PlaybackState}, position={sender?.Position.TotalSeconds}, naturalDuration={sender?.NaturalDuration.TotalSeconds}, buffer={buffer}");
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
            await RunOnUiThreadAsync(() =>
            {
                EnsurePlayerElement();
                VideoPlayer.ApplyStretch(m_playerElement, m_realPlayInfo, mode);
            });
        }

        public override async Task SetVideoEnable(bool enable)
        {
            await RunOnUiThreadAsync(() =>
            {
                EnsurePlayerElement();
                m_playerElement.Visibility = enable ? Visibility.Visible : Visibility.Collapsed;
                if (!enable)
                {
                    if (m_timelineController != null)
                    {
                        m_timelineController.Pause();
                    }
                    else
                    {
                        m_mediaPlayer?.Pause();
                    }
                }
            });
        }

        private MediaPlayer CreateVideoPlayer(bool autoPlay, bool trackBuffering)
        {
            var mediaPlayer = new MediaPlayer();
            mediaPlayer.AutoPlay = autoPlay;
            mediaPlayer.MediaOpened += MediaPlayerOnMediaOpened;
            mediaPlayer.MediaEnded += MediaPlayerOnMediaEnded;
            mediaPlayer.MediaFailed += MediaPlayerOnMediaFailed;
            mediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSessionOnPlaybackStateChanged;
            if (trackBuffering)
            {
                mediaPlayer.PlaybackSession.BufferingStarted += PlaybackSessionOnBufferingStarted;
                mediaPlayer.PlaybackSession.BufferingProgressChanged += PlaybackSessionOnBufferingProgressChanged;
                mediaPlayer.PlaybackSession.BufferingEnded += PlaybackSessionOnBufferingEnded;
            }

            mediaPlayer.PlaybackSession.PositionChanged += PlaybackSessionOnPositionChanged;
            return mediaPlayer;
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
                VerticalAlignment = VerticalAlignment.Stretch,
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

        private async Task LoadDashWithDualPlayersAsync()
        {
            var videoUrl = m_realPlayInfo.DashInfo.Video.Url;
            var audioUrl = m_realPlayInfo.DashInfo.Audio.Url;
            var isVideoLocal = IsLocalPathOrFileUri(videoUrl);
            var isAudioLocal = IsLocalPathOrFileUri(audioUrl);
            StorageFile videoFile = null;
            StorageFile audioFile = null;

            if (isVideoLocal)
            {
                videoFile = await LoadLocalFileAsync(videoUrl);
            }

            if (isAudioLocal)
            {
                audioFile = await LoadLocalFileAsync(audioUrl);
            }

            var videoBasicProperties = videoFile == null ? null : await videoFile.GetBasicPropertiesAsync();
            var audioBasicProperties = await audioFile.GetBasicPropertiesAsync();

            _logger.Info(
                $"DashNative.Load dual source: videoUrl={SanitizeUrl(videoUrl)}, videoPath={(videoFile?.Path ?? "remote")}, videoType={(videoFile?.FileType ?? "remote")}, videoSize={(videoBasicProperties?.Size ?? 0)}, audioPath={audioFile.Path}, audioType={audioFile.FileType}, audioSize={audioBasicProperties.Size}");

            m_mediaPlayer = CreateVideoPlayer(autoPlay: false, trackBuffering: !isVideoLocal);
            m_audioPlayer = new MediaPlayer();
            m_audioPlayer.AutoPlay = false;
            m_audioPlayer.MediaFailed += AudioPlayerOnMediaFailed;
            if (isVideoLocal)
            {
                m_audioPlayer.PlaybackSession.BufferingStarted += PlaybackSessionOnBufferingStarted;
                m_audioPlayer.PlaybackSession.BufferingProgressChanged += PlaybackSessionOnBufferingProgressChanged;
                m_audioPlayer.PlaybackSession.BufferingEnded += PlaybackSessionOnBufferingEnded;
            }

            m_timelineController = new MediaTimelineController();
            m_mediaPlayer.TimelineController = m_timelineController;
            m_audioPlayer.TimelineController = m_timelineController;

            if (isVideoLocal)
            {
                m_mediaPlayer.Source = MediaSource.CreateFromStorageFile(videoFile);
                _logger.Info($"DashNative.Load dual video source: local file path={videoFile.Path}, type={videoFile.FileType}");
            }
            else
            {
                // In dual-player mode (remote video + local audio), keep header-aware loading path for remote video.
                var videoOnlyDashInfo = CloneDashInfoWithoutAudio(m_realPlayInfo.DashInfo);
                var adaptiveMediaSource = await CreateAdaptiveMediaSource(videoOnlyDashInfo, m_realPlayInfo.UserAgent, m_realPlayInfo.Referer);
                if (adaptiveMediaSource != null)
                {
                    m_mediaPlayer.Source = MediaSource.CreateFromAdaptiveMediaSource(adaptiveMediaSource);
                    _logger.Info(
                        $"DashNative.Load dual video source: adaptive source created, videoUrl={SanitizeUrl(videoUrl)}, hasUserAgent={!string.IsNullOrWhiteSpace(m_realPlayInfo?.UserAgent)}, hasReferer={!string.IsNullOrWhiteSpace(m_realPlayInfo?.Referer)}");
                }
                else
                {
                    _logger.Warn(
                        $"DashNative.Load dual video source: adaptive source failed, fallback CreateFromUri, videoUrl={SanitizeUrl(videoUrl)}");
                    m_mediaPlayer.Source = MediaSource.CreateFromUri(new Uri(videoUrl));
                }
            }

            m_audioPlayer.Source = MediaSource.CreateFromStorageFile(audioFile);

            Volume = m_volume;
            IsMuted = m_isMuted;
        }

        private static BiliDashPlayUrlInfo CloneDashInfoWithoutAudio(BiliDashPlayUrlInfo source)
        {
            if (source == null)
            {
                return null;
            }

            return new BiliDashPlayUrlInfo
            {
                Duration = source.Duration,
                MinBufferTime = source.MinBufferTime,
                Video = source.Video,
                Audio = null,
            };
        }

        private static async Task<StorageFile> LoadLocalFileAsync(string url)
        {
            var localPath = NormalizeLocalPath(url);
            return await StorageFile.GetFileFromPathAsync(localPath);
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

        private static string NormalizeLocalPath(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.IsFile)
            {
                return uri.LocalPath;
            }

            return url;
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

        private void AudioPlayerOnMediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            _logger.Error(
                $"DashNative Audio MediaFailed: error={args?.Error}, errorMessage={args?.ErrorMessage}, extendedErrorCode={args?.ExtendedErrorCode}, url={SanitizeUrl(m_realPlayInfo?.DashInfo?.Audio?.Url)}");
            EmitError(PlayerError.PlayerErrorCode.NeedUseOtherPlayerError, args.ErrorMessage, PlayerError.RetryStrategy.Normal);
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
            _ = RunOnUiThreadAsync(() =>
            {
                _logger.Info(
                $"DashNative.MediaOpened: video={sender?.PlaybackSession?.NaturalVideoWidth}x{sender?.PlaybackSession?.NaturalVideoHeight}, duration={sender?.PlaybackSession?.NaturalDuration.TotalSeconds}, position={sender?.PlaybackSession?.Position.TotalSeconds}, elementVisibility={m_playerElement?.Visibility}, elementSize={m_playerElement?.ActualWidth}x{m_playerElement?.ActualHeight}");
                MediaOpened?.Invoke(this, EventArgs.Empty);
            });
        }
    }
}
