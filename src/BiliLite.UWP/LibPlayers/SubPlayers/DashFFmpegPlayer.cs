using System;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.UI.Xaml.Controls;
using BiliLite.Models.Common.Player;
using BiliLite.Player.MediaInfos;
using BiliLite.Extensions;
using FFmpegInteropX;
using Windows.UI.Core;
using BiliLite.Services;
using Windows.Media;
using Google.Type;

namespace BiliLite.Player.SubPlayers
{
    public class DashFFmpegPlayer : ISubPlayer
    {
        private MediaPlayer m_videoMediaPlayer;
        private MediaPlayer m_audioMediaPlayer;
        private FFmpegInteropX.FFmpegMediaSource m_videoMediaSource;
        private FFmpegInteropX.FFmpegMediaSource m_audioMediaSource;
        private readonly MediaSourceConfig m_config;
        private PlayerConfig m_playerConfig;
        private MediaPlayerElement m_videoPlayerElement;
        private MediaPlayerElement m_audioPlayerElement;
        private double m_duration;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private MediaTimelineController m_mediaTimelineController;

        public DashFFmpegPlayer(PlayerConfig playerConfig, MediaPlayerElement videoPlayerElement,
            MediaPlayerElement audioPlayerElement)
        {
            m_playerConfig = playerConfig;
            m_videoPlayerElement = videoPlayerElement;
            m_audioPlayerElement = audioPlayerElement;
            m_videoMediaPlayer = new MediaPlayer();
            m_audioMediaPlayer = new MediaPlayer();
            m_mediaTimelineController = new MediaTimelineController();
            //m_videoMediaPlayer.AutoPlay = true;
            //m_audioMediaPlayer.AutoPlay = true;
            m_videoMediaPlayer.TimelineController = m_mediaTimelineController;
            m_audioMediaPlayer.TimelineController = m_mediaTimelineController;
            m_mediaTimelineController.Start();
            InitPlayerEvent();
            m_config = new MediaSourceConfig();
        }

        public override event EventHandler MediaOpened;
        public override event EventHandler MediaEnded;
        public override event EventHandler BufferingEnded;
        public override event EventHandler<double> PositionChanged;

        public override double Volume
        {
            get => m_audioMediaPlayer.Volume;
            set => m_audioMediaPlayer.Volume = value;
        }

        public override double Position
        {
            get => m_mediaTimelineController.Position.TotalSeconds;
            set => m_mediaTimelineController.Position = TimeSpan.FromSeconds(value);
        }

        public override double Duration => m_duration;

        private void InitPlayerEvent()
        {
            m_videoMediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged; ;
            m_videoMediaPlayer.PlaybackSession.BufferingStarted += PlaybackSession_BufferingStarted;
            m_videoMediaPlayer.PlaybackSession.BufferingProgressChanged += PlaybackSession_BufferingProgressChanged;
            m_videoMediaPlayer.PlaybackSession.BufferingEnded += PlaybackSession_BufferingEnded;
            m_videoMediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            m_videoMediaPlayer.MediaEnded += MediaPlayer_MediaEnded; 
            m_videoMediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
            m_videoMediaPlayer.PlaybackSession.PositionChanged += PlaybackSession_PositionChanged;
        }

        private async void PlaybackSession_PositionChanged(MediaPlaybackSession sender, object args)
        {
            await m_videoPlayerElement.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                PositionChanged?.Invoke(this, Position);
            });
        }

        private void MediaPlayer_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            //switch (args.Error)
            //{
            //    case MediaPlayerError.Aborted:
            //        break;
            //    case MediaPlayerError.DecodingError:
            //        break;

            //    case MediaPlayerError.Unknown:
            //        break;
            //    case MediaPlayerError.NetworkError:
            //        break;
            //    case MediaPlayerError.SourceNotSupported:
            //        break;
            //    default:
            //        throw new ArgumentOutOfRangeException();
            //}
            EmitError(PlayerError.PlayerErrorCode.UnknownError, "");
        }

        private void MediaPlayer_MediaEnded(MediaPlayer sender, object args)
        {
            MediaEnded?.Invoke(this, EventArgs.Empty);
        }

        private void MediaPlayer_MediaOpened(MediaPlayer sender, object args)
        {
            MediaOpened?.Invoke(this, EventArgs.Empty);
        }

        private void PlaybackSession_BufferingEnded(MediaPlaybackSession sender, object args)
        {
            BufferingEnded?.Invoke(this, EventArgs.Empty);
        }

        // TODO: 显示缓冲进度
        private void PlaybackSession_BufferingProgressChanged(MediaPlaybackSession sender, object args)
        {
            //PlayerLoadText.Text = sender.BufferingProgress.ToString("p");
        }

        private void PlaybackSession_BufferingStarted(MediaPlaybackSession sender, object args)
        {
        }

        private void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            _logger.Debug($"PlaybackStateChanged {sender.PlaybackState}");
        }

        private async Task StopCore()
        {
            if (m_videoMediaPlayer != null)
            {
                m_videoMediaPlayer.Pause();
                m_videoMediaPlayer.Source = null;
            }

            if (m_audioMediaPlayer != null)
            {
                m_audioMediaPlayer.Pause();
                m_audioMediaPlayer.Source = null;
            }

            if (m_videoMediaSource != null)
            {
                m_videoMediaSource.Dispose();
                m_videoMediaSource = null;
            }

            if (m_audioMediaSource != null)
            {
                m_audioMediaSource.Dispose();
                m_audioMediaSource = null;
            }
            await m_videoPlayerElement.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                m_videoPlayerElement.SetMediaPlayer(null);
                m_audioPlayerElement.SetMediaPlayer(null);
            });
        }

        public override CollectInfo GetCollectInfo()
        {
            return new CollectInfo()
            {
                Data = null,
                RealPlayInfo = m_realPlayInfo,
                Type = "DashFFmpeg",
                Url = "",
            };
        }

        public override async Task Load()
        {
            m_config.ReloadFFmpegConfig(m_playerConfig.UserAgent, m_playerConfig.Referer);
            if (!string.IsNullOrEmpty(m_realPlayInfo.PlayUrls.DashVideoUrl))
            {
                m_videoMediaSource = await FFmpegMediaSource.CreateFromUriAsync(m_realPlayInfo.PlayUrls.DashVideoUrl, m_config);
                var mediaSource = m_videoMediaSource.CreateMediaPlaybackItem();
                m_videoMediaPlayer.Source = mediaSource;
            }
            if (!string.IsNullOrEmpty(m_realPlayInfo.PlayUrls.DashAudioUrl))
            {
                try
                {
                    m_audioMediaSource = await FFmpegMediaSource.CreateFromUriAsync(m_realPlayInfo.PlayUrls.DashAudioUrl, m_config);
                    var mediaSource = m_audioMediaSource.CreateMediaPlaybackItem();
                    m_audioMediaPlayer.Source = mediaSource;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message, ex);
                }
            }
        }

        public override async Task Buff()
        {
            await m_videoPlayerElement.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (m_videoMediaPlayer != null && m_duration==0)
                {
                    m_duration = m_videoMediaPlayer.PlaybackSession.NaturalDuration.TotalSeconds;
                }
                if (m_audioMediaPlayer != null && m_duration == 0)
                {
                    m_duration = m_audioMediaPlayer.PlaybackSession.NaturalDuration.TotalSeconds;
                }
            });
        }

        public override async Task Play()
        {
            await m_videoPlayerElement.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (m_videoMediaPlayer != null)
                {
                    m_videoPlayerElement.SetMediaPlayer(m_videoMediaPlayer);
                }
                if (m_audioMediaPlayer != null)
                {
                    m_audioPlayerElement.SetMediaPlayer(m_audioMediaPlayer);
                }
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
            m_mediaTimelineController.Pause();
        }

        public override async Task Resume()
        {
            m_mediaTimelineController.Resume();
        }

        public override async Task SetRate(double value)
        {
            m_mediaTimelineController.ClockRate = value;
        }
    }
}
