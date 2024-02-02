using System;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using BiliLite.Extensions;
using BiliLite.Models.Common.Player;
using BiliLite.Player.MediaInfos;
using FFmpegInteropX;
using Windows.Media.Core;

namespace BiliLite.Player.SubPlayers
{
    public class FlvSyEnginePlayer : ISubPlayer
    {
        private MediaPlayer m_videoMediaPlayer;
        private readonly MediaSourceConfig m_config;
        private PlayerConfig m_playerConfig;
        private MediaPlayerElement m_videoPlayerElement;

        public FlvSyEnginePlayer(PlayerConfig playerConfig, MediaPlayerElement videoPlayerElement,
            MediaPlayerElement audioPlayerElement)
        {
            m_playerConfig = playerConfig;
            m_videoPlayerElement = videoPlayerElement;
            m_videoMediaPlayer = new MediaPlayer();
            InitPlayerEvent();
            m_config = new MediaSourceConfig();
        }

        public override event EventHandler MediaOpened;
        public override event EventHandler MediaEnded;
        public override event EventHandler BufferingEnded;

        public override double Volume { get; set; }
        public override double Position { get; set; }
        public override double Duration { get; }

        private void InitPlayerEvent()
        {
            m_videoMediaPlayer.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged; ;
            m_videoMediaPlayer.PlaybackSession.BufferingStarted += PlaybackSession_BufferingStarted;
            m_videoMediaPlayer.PlaybackSession.BufferingProgressChanged += PlaybackSession_BufferingProgressChanged;
            m_videoMediaPlayer.PlaybackSession.BufferingEnded += PlaybackSession_BufferingEnded;
            m_videoMediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            m_videoMediaPlayer.MediaEnded += MediaPlayer_MediaEnded; ;
            m_videoMediaPlayer.MediaFailed += MediaPlayer_MediaFailed;
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
        }

        private async Task StopCore()
        {
            if (m_videoMediaPlayer != null)
            {
                m_videoMediaPlayer.Pause();
                m_videoMediaPlayer.Source = null;
            }
            await m_videoPlayerElement.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                m_videoPlayerElement.SetMediaPlayer(null);
            });
        }

        public override CollectInfo GetCollectInfo()
        {
            throw new System.NotImplementedException();
        }

        public override async Task Load()
        {
            var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);
            //if (needConfig)
            //{
            //    playList.NetworkConfigs = CreatePlaylistNetworkConfigs(userAgent, referer, epId);
            //}
            foreach (var urlItem in m_realPlayInfo.PlayUrls.VideoFlvUrls)
            {
                playList.Append(urlItem.Url, 0, urlItem.Length / 1000);
            }
            var mediaSource = await playList.SaveAndGetFileUriAsync();

            m_videoMediaPlayer.Source = MediaSource.CreateFromUri(mediaSource);
        }

        public override async Task Buff()
        {
        }

        public override async Task Play()
        {
            await m_videoPlayerElement.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (m_videoMediaPlayer != null)
                {
                    m_videoPlayerElement.SetMediaPlayer(m_videoMediaPlayer);
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
            m_videoMediaPlayer.Pause();
        }

        public override async Task Resume()
        {
            m_videoMediaPlayer.Play();
        }
    }
}
