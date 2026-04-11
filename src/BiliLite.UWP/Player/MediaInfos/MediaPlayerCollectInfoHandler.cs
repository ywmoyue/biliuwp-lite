using System;
using System.Timers;

namespace BiliLite.Player.MediaInfos
{
    public class MediaPlayerCollectInfoHandler : BaseCollectInfoHandler
    {
        private CollectInfo m_collectInfo;
        private readonly Timer m_timer;

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

                m_mediaInfosCollector.MediaInfo.BufferingProgress = GetSafeBufferingProgress(session);
                m_mediaInfosCollector.MediaInfo.VideoWidth = session.NaturalVideoWidth;
                m_mediaInfosCollector.MediaInfo.VideoHeight = session.NaturalVideoHeight;
                m_mediaInfosCollector.EmitUpdateMediaInfos();
            }
            catch
            {
                // Player may be disposed during timer tick; ignore this cycle.
            }
        }

        private static double GetSafeBufferingProgress(Windows.Media.Playback.MediaPlaybackSession session)
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
    }
}
