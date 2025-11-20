using System.Timers;

namespace BiliLite.Player.MediaInfos
{
    public class FFMpegInteropMssCollectInfoHandler : BaseCollectInfoHandler
    {
        private CollectInfo m_collectInfo;
        private Timer m_timer;

        public FFMpegInteropMssCollectInfoHandler(MediaInfosCollector mediaInfosCollector) : base(mediaInfosCollector)
        {
            m_timer = new Timer(2000);
            m_timer.AutoReset = true;
            m_timer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CollectMediaInfo();
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

        private void CollectMediaInfo()
        {
            //if (!(m_collectInfo.Data is FFMpegInteropMssCollectInfoData collectInfoData)) return;
            //var ffMpegInteropMss = collectInfoData.FFMpegInteropMss;
            //if (ffMpegInteropMss.VideoStreams.Count > 0)
            //{
            //    m_mediaInfosCollector.MediaInfo.VideoBitRate = ffMpegInteropMss.VideoStreams[0].Bitrate;
            //    m_mediaInfosCollector.MediaInfo.VideoCodec = ffMpegInteropMss.VideoStreams[0].CodecName;
            //    m_mediaInfosCollector.MediaInfo.Fps = ffMpegInteropMss.VideoStreams[0].FramesPerSecond;
            //    m_mediaInfosCollector.MediaInfo.VideoHeight = ffMpegInteropMss.VideoStreams[0].PixelHeight;
            //    m_mediaInfosCollector.MediaInfo.VideoWidth = ffMpegInteropMss.VideoStreams[0].PixelWidth;
            //}

            //if (ffMpegInteropMss.AudioStreams.Count > 0)
            //{
            //    m_mediaInfosCollector.MediaInfo.AudioBitRate = ffMpegInteropMss.AudioStreams[0].Bitrate;
            //    m_mediaInfosCollector.MediaInfo.AudioCodec = ffMpegInteropMss.AudioStreams[0].CodecName;
            //}

            //m_mediaInfosCollector.MediaInfo.BufferingProgress = collectInfoData.MediaPlayer.PlaybackSession.DownloadProgress;
            //m_mediaInfosCollector.MediaInfo.BufferTime = ffMpegInteropMss.BufferTime.ToString("g");
            
            //m_mediaInfosCollector.EmitUpdateMediaInfos();
        }
    }
}
