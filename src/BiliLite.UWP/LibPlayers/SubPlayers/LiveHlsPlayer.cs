using System;
using FFmpegInteropX;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using BiliLite.Models.Common.Player;
using BiliLite.Player.SubPlayers;
using BiliLite.Player.MediaInfos;

namespace BiliLite.Player
{
    public class LiveHlsPlayer : ISubPlayer
    {
        private MediaPlayer m_mediaPlayer;
        private FFmpegInteropX.FFmpegMediaSource m_fFmpegMediaSource;
        private readonly MediaSourceConfig m_config;
        private PlayerConfig m_playerConfig;
        private MediaPlayerElement m_playerElement;
        private string m_url;

        public LiveHlsPlayer(PlayerConfig playerConfig, MediaPlayerElement playerElement)
        {
            m_playerConfig = playerConfig;
            m_playerElement = playerElement;
            m_mediaPlayer = new MediaPlayer();
            InitPlayerEvent();
            m_config = new MediaSourceConfig();
            m_config.FFmpegOptions.Add("rtsp_transport", "tcp");
            m_config.FFmpegOptions.Add("user_agent", "Mozilla/5.0 BiliDroid/1.12.0 (bbcallen@gmail.com)");
            m_config.FFmpegOptions.Add("referer", "https://live.bilibili.com/");
        }

        public override double Volume
        {
            get => m_mediaPlayer.Volume;
            set => m_mediaPlayer.Volume = value;
        }

        public override event EventHandler MediaOpened;
        public override event EventHandler MediaEnded;
        public override event EventHandler BufferingEnded;

        private void InitPlayerEvent()
        {
            m_mediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged; ;
            m_mediaPlayer.PlaybackSession.BufferingStarted += PlaybackSession_BufferingStarted;
            m_mediaPlayer.PlaybackSession.BufferingProgressChanged += PlaybackSession_BufferingProgressChanged;
            m_mediaPlayer.PlaybackSession.BufferingEnded += PlaybackSession_BufferingEnded;
            m_mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            m_mediaPlayer.MediaEnded += MediaPlayer_MediaEnded; ;
            m_mediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
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
            EmitError(PlayerError.PlayerErrorCode.UnknownError,"");
        }

        private void ReloadConfig()
        {
            m_config.VideoDecoderMode = m_playerConfig.EnableHw ? VideoDecoderMode.ForceSystemDecoder : VideoDecoderMode.ForceFFmpegSoftwareDecoder;
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
        }

        private async Task StopCore()
        {
            if (m_mediaPlayer != null)
            {
                m_mediaPlayer.Pause();
                m_mediaPlayer.Source = null;
            }
            if (m_fFmpegMediaSource != null)
            {
                m_fFmpegMediaSource.Dispose();
                m_fFmpegMediaSource = null;
            }
            await m_playerElement.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                m_playerElement.SetMediaPlayer(null);
            });
        }

        public override CollectInfo GetCollectInfo()
        {
            return new CollectInfo()
            {
                Data = m_fFmpegMediaSource,
                RealPlayInfo = m_realPlayInfo,
                Type = "LiveHls",
                Url = m_url,
            };
        }

        public override async Task Load()
        {
            ReloadConfig();
            var urls = m_realPlayInfo.PlayUrls;
            if (urls.HlsUrls == null && urls.FlvUrls == null)
            {
                EmitError(PlayerError.PlayerErrorCode.PlayUrlError,"获取播放地址失败", PlayerError.RetryStrategy.NoRetry);
            }

            var defaultPlayerMode = m_playerConfig.PlayMode;
            var selectRouteLine = m_playerConfig.SelectedRouteLine;
            string url;

            if (defaultPlayerMode == LivePlayerMode.Hls)
            {
                url = urls.HlsUrls != null ? urls.HlsUrls[selectRouteLine].Url : urls.FlvUrls[selectRouteLine].Url;
            }
            else
            {
                url = urls.FlvUrls != null ? urls.FlvUrls[selectRouteLine].Url : urls.HlsUrls[selectRouteLine].Url;
            }

            m_url = url;

            m_fFmpegMediaSource = await FFmpegMediaSource.CreateFromUriAsync(url, m_config);
            m_mediaPlayer.AutoPlay = true;
            m_mediaPlayer.Source = m_fFmpegMediaSource.CreateMediaPlaybackItem();
        }

        public override async Task Buff()
        {
        }

        public override async Task Play()
        {
            await m_playerElement.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                m_playerElement.SetMediaPlayer(m_mediaPlayer);
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
            m_mediaPlayer.Pause();
        }

        public override async Task Resume()
        {
            m_mediaPlayer.Play();
        }
    }
}
