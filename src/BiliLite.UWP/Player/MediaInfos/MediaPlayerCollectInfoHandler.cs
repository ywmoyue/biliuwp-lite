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
            if (!(m_collectInfo?.Data is MediaPlayerCollectInfoData data) || data.MediaPlayer?.PlaybackSession == null)
            {
                return;
            }

            var session = data.MediaPlayer.PlaybackSession;
            m_mediaInfosCollector.MediaInfo.BufferingProgress = session.BufferingProgress;
            m_mediaInfosCollector.MediaInfo.VideoWidth = session.NaturalVideoWidth;
            m_mediaInfosCollector.MediaInfo.VideoHeight = session.NaturalVideoHeight;
            m_mediaInfosCollector.EmitUpdateMediaInfos();
        }
    }
}
