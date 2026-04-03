using System;
using System.Threading.Tasks;
using BiliLite.Models.Common.Player;
using BiliLite.Player.MediaInfos;
using FFmpegInteropX;
using Windows.ApplicationModel.Core;
using Windows.Media;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BiliLite.Player.SubPlayers
{
    public class DashFFmpegInteropSubPlayer : ISubPlayer
    {
        private readonly MediaPlayerElement m_playerElement;
        private MediaPlayer m_videoPlayer;
        private MediaPlayer m_audioPlayer;
        private MediaTimelineController m_timelineController;
        private FFmpegMediaSource m_videoMediaSource;
        private FFmpegMediaSource m_audioMediaSource;
        private string m_url;
        private bool m_isBuffering;
        private double m_bufferCache;
        private double m_volume = 1;
        private bool m_isMuted;

        public DashFFmpegInteropSubPlayer(MediaPlayerElement playerElement)
        {
            m_playerElement = playerElement;
        }

        public override RealPlayerType Type { get; } = RealPlayerType.FFmpegInterop;

        public override double Volume
        {
            get => m_videoPlayer?.Volume ?? m_audioPlayer?.Volume ?? m_volume;
            set
            {
                m_volume = value;
                if (m_videoPlayer != null)
                {
                    m_videoPlayer.Volume = value;
                }

                if (m_audioPlayer != null)
                {
                    m_audioPlayer.Volume = value;
                }
            }
        }

        public override double Position => m_timelineController?.Position.TotalSeconds
            ?? m_videoPlayer?.PlaybackSession?.Position.TotalSeconds
            ?? m_audioPlayer?.PlaybackSession?.Position.TotalSeconds
            ?? 0;

        public override double Duration
        {
            get
            {
                var duration = m_videoPlayer?.PlaybackSession?.NaturalDuration.TotalSeconds
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
                if (m_videoPlayer != null)
                {
                    m_videoPlayer.IsMuted = value;
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
                Data = new FFMpegInteropMssCollectInfoData()
                {
                    FFMpegInteropMss = m_videoMediaSource,
                    MediaPlayer = m_videoPlayer,
                },
                RealPlayInfo = m_realPlayInfo,
                Type = "LiveHls",
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
            await StopCore();

            var config = CreateMediaSourceConfig();
            m_videoMediaSource = await CreateMediaSourceAsync(m_realPlayInfo.DashInfo.Video?.Url, config);
            if (m_videoMediaSource == null)
            {
                EmitError(PlayerError.PlayerErrorCode.NeedUseOtherPlayerError, "创建 DASH 视频源失败", PlayerError.RetryStrategy.Normal);
                return;
            }

            if (!string.IsNullOrWhiteSpace(m_realPlayInfo.DashInfo.Audio?.Url))
            {
                m_audioMediaSource = await CreateMediaSourceAsync(m_realPlayInfo.DashInfo.Audio.Url, config);
            }

            m_videoPlayer = new MediaPlayer();
            m_videoPlayer.AutoPlay = true;
            m_videoPlayer.Source = m_videoMediaSource.CreateMediaPlaybackItem();
            m_videoPlayer.MediaOpened += MediaPlayerOnMediaOpened;
            m_videoPlayer.MediaEnded += MediaPlayerOnMediaEnded;
            m_videoPlayer.MediaFailed += MediaPlayerOnMediaFailed;
            m_videoPlayer.PlaybackSession.PositionChanged += PlaybackSessionOnPositionChanged;

            if (m_audioMediaSource != null)
            {
                m_audioPlayer = new MediaPlayer();
                m_audioPlayer.AutoPlay = true;
                m_audioPlayer.Source = m_audioMediaSource.CreateMediaPlaybackItem();
                m_audioPlayer.MediaFailed += AudioPlayerOnMediaFailed;

                m_timelineController = new MediaTimelineController();
                m_videoPlayer.TimelineController = m_timelineController;
                m_audioPlayer.TimelineController = m_timelineController;

                m_audioPlayer.PlaybackSession.BufferingStarted += PlaybackSessionOnBufferingStarted;
                m_audioPlayer.PlaybackSession.BufferingProgressChanged += PlaybackSessionOnBufferingProgressChanged;
                m_audioPlayer.PlaybackSession.BufferingEnded += PlaybackSessionOnBufferingEnded;
            }
            else
            {
                m_videoPlayer.PlaybackSession.BufferingStarted += PlaybackSessionOnBufferingStarted;
                m_videoPlayer.PlaybackSession.BufferingProgressChanged += PlaybackSessionOnBufferingProgressChanged;
                m_videoPlayer.PlaybackSession.BufferingEnded += PlaybackSessionOnBufferingEnded;
            }

            Volume = m_volume;
            IsMuted = m_isMuted;
            await SetRate(m_rate);
        }

        public override async Task Buff()
        {
        }

        public override async Task Play()
        {
            await RunOnUiThreadAsync(() =>
            {
                if (m_playerElement.MediaPlayer != m_videoPlayer)
                {
                    m_playerElement.SetMediaPlayer(m_videoPlayer);
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

                    return;
                }

                m_videoPlayer?.Play();
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

            m_videoPlayer?.Pause();
        }

        public override async Task Resume()
        {
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

            m_videoPlayer?.Play();
        }

        public override async Task SetRate(double value)
        {
            m_rate = value;
            if (m_timelineController != null)
            {
                m_timelineController.ClockRate = value;
                return;
            }

            if (m_videoPlayer?.PlaybackSession != null)
            {
                m_videoPlayer.PlaybackSession.PlaybackRate = value;
            }
        }

        public override async Task SetPosition(double value)
        {
            if (m_timelineController != null)
            {
                m_timelineController.Position = TimeSpan.FromSeconds(value);
                return;
            }

            if (m_videoPlayer?.PlaybackSession != null)
            {
                m_videoPlayer.PlaybackSession.Position = TimeSpan.FromSeconds(value);
            }
        }

        private async Task StopCore()
        {
            if (m_audioPlayer != null)
            {
                m_audioPlayer.Pause();
                m_audioPlayer.Source = null;
                m_audioPlayer.MediaFailed -= AudioPlayerOnMediaFailed;
                m_audioPlayer.PlaybackSession.BufferingStarted -= PlaybackSessionOnBufferingStarted;
                m_audioPlayer.PlaybackSession.BufferingProgressChanged -= PlaybackSessionOnBufferingProgressChanged;
                m_audioPlayer.PlaybackSession.BufferingEnded -= PlaybackSessionOnBufferingEnded;
                m_audioPlayer.Dispose();
                m_audioPlayer = null;
            }

            if (m_videoPlayer != null)
            {
                m_videoPlayer.Pause();
                m_videoPlayer.Source = null;
                m_videoPlayer.MediaOpened -= MediaPlayerOnMediaOpened;
                m_videoPlayer.MediaEnded -= MediaPlayerOnMediaEnded;
                m_videoPlayer.MediaFailed -= MediaPlayerOnMediaFailed;
                m_videoPlayer.PlaybackSession.BufferingStarted -= PlaybackSessionOnBufferingStarted;
                m_videoPlayer.PlaybackSession.BufferingProgressChanged -= PlaybackSessionOnBufferingProgressChanged;
                m_videoPlayer.PlaybackSession.BufferingEnded -= PlaybackSessionOnBufferingEnded;
                m_videoPlayer.PlaybackSession.PositionChanged -= PlaybackSessionOnPositionChanged;
                m_videoPlayer.Dispose();
                m_videoPlayer = null;
            }

            if (m_timelineController != null)
            {
                m_timelineController = null;
            }

            if (m_audioMediaSource != null)
            {
                m_audioMediaSource.Dispose();
                m_audioMediaSource = null;
            }

            if (m_videoMediaSource != null)
            {
                m_videoMediaSource.Dispose();
                m_videoMediaSource = null;
            }

            await RunOnUiThreadAsync(() => m_playerElement.SetMediaPlayer(null));
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
                    if (m_timelineController != null)
                    {
                        m_timelineController.Pause();
                    }
                    else
                    {
                        m_videoPlayer?.Pause();
                    }
                }
            });
        }

        private MediaSourceConfig CreateMediaSourceConfig()
        {
            var config = new MediaSourceConfig();
            if (!string.IsNullOrWhiteSpace(m_realPlayInfo.UserAgent))
            {
                config.FFmpegOptions.Add("user_agent", m_realPlayInfo.UserAgent);
            }

            if (!string.IsNullOrWhiteSpace(m_realPlayInfo.Referer))
            {
                config.FFmpegOptions.Add("referer", m_realPlayInfo.Referer);
                config.FFmpegOptions.Add("headers", $"Referer: {m_realPlayInfo.Referer}");
            }

            return config;
        }

        private async Task<FFmpegMediaSource> CreateMediaSourceAsync(string url, MediaSourceConfig config)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return null;
            }

            if (m_realPlayInfo.IsLocal)
            {
                var localPath = NormalizeLocalPath(url);
                var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(localPath);
                var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                return await FFmpegMediaSource.CreateFromStreamAsync(stream, config);
            }

            return await FFmpegMediaSource.CreateFromUriAsync(url, config);
        }

        private static string NormalizeLocalPath(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri) && uri.IsFile)
            {
                return uri.LocalPath;
            }

            return url;
        }

        private static async Task RunOnUiThreadAsync(Action action)
        {
            if (action == null)
            {
                return;
            }

            var dispatcher = CoreApplication.MainView?.CoreWindow?.Dispatcher;
            if (dispatcher == null || dispatcher.HasThreadAccess)
            {
                action();
                return;
            }

            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => action());
        }

        public override async Task<byte[]> CaptureAsync()
        {
            return await VideoPlayer.RenderElementToPngBytesAsync(m_playerElement, 96);
        }

        private void PlaybackSessionOnPositionChanged(MediaPlaybackSession sender, object args)
        {
            PositionChanged?.Invoke(this, sender?.Position.TotalSeconds ?? 0);
        }

        private void AudioPlayerOnMediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            EmitError(PlayerError.PlayerErrorCode.NeedUseOtherPlayerError, args.ErrorMessage, PlayerError.RetryStrategy.Normal);
        }

        private void MediaPlayerOnMediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            EmitError(PlayerError.PlayerErrorCode.NeedUseOtherPlayerError, args.ErrorMessage, PlayerError.RetryStrategy.Normal);
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
