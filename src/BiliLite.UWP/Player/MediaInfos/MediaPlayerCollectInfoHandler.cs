using System;
using System.Timers;

namespace BiliLite.Player.MediaInfos
{
    public class MediaPlayerCollectInfoHandler : BaseCollectInfoHandler
    {
        private CollectInfo m_collectInfo;
        private readonly Timer m_timer;
        private bool m_canReadBufferingProgress = true;
        private bool m_hasLoggedBufferingProgressUnsupported;

        public MediaPlayerCollectInfoHandler(MediaInfosCollector mediaInfosCollector) : base(mediaInfosCollector)
        {
            m_timer = new Timer(1000);
            m_timer.AutoReset = true;
            m_timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Collect();
        }

        public override void InternalStart(CollectInfo collectInfo)
        {
            m_collectInfo = collectInfo;
            m_canReadBufferingProgress = true;
            m_hasLoggedBufferingProgressUnsupported = false;
            m_timer.Start();
        }

        public override void InternalStop()
        {
            m_timer.Stop();
        }

        private void Collect()
        {
            if (!(m_collectInfo?.Data is MediaPlayerCollectInfoData data) || data.MediaPlayer == null)
            {
                return;
            }

            try
            {
                var session = data.MediaPlayer.PlaybackSession;
                if (session == null)
                {
                    return;
                }

                m_mediaInfosCollector.MediaInfo.BufferingProgress = TryGetBufferingProgress(session);
                m_mediaInfosCollector.MediaInfo.VideoWidth = session.NaturalVideoWidth;
                m_mediaInfosCollector.MediaInfo.VideoHeight = session.NaturalVideoHeight;
                m_mediaInfosCollector.EmitUpdateMediaInfos();
            }
            catch
            {
                // Player may be disposed during timer tick; ignore this cycle.
            }
        }

        private double? TryGetBufferingProgress(Windows.Media.Playback.MediaPlaybackSession session)
        {
            if (session == null || !m_canReadBufferingProgress)
            {
                return null;
            }

            try
            {
                return session.BufferingProgress;
            }
            catch
            {
                m_canReadBufferingProgress = false;
                if (!m_hasLoggedBufferingProgressUnsupported)
                {
                    m_hasLoggedBufferingProgressUnsupported = true;
                    _logger.Warn("MediaPlayerCollectInfoHandler: BufferingProgress 在当前播放器实例上不可读，后续采集将跳过该字段");
                }

                return null;
            }
        }

        public override void Dispose()
        {
            m_timer.Stop();
            m_timer.Elapsed -= Timer_Elapsed;
            m_timer.Dispose();
            m_collectInfo = null;
            m_canReadBufferingProgress = true;
            m_hasLoggedBufferingProgressUnsupported = false;
        }
    }
}
