using FFmpegInteropX;

namespace BiliLite.Player.MediaInfos
{
    public class FFMpegInteropMssCollectInfoHandler : BaseCollectInfoHandler
    {
        public FFMpegInteropMssCollectInfoHandler(MediaInfosCollector mediaInfosCollector) : base(mediaInfosCollector)
        {
        }

        public override void InternalStart(CollectInfo collectInfo)
        {
            if (!(collectInfo.Data is FFmpegMediaSource ffMpegInteropMss)) return;
            if (ffMpegInteropMss.VideoStreams.Count > 0)
            {
                m_mediaInfosCollector.MediaInfo.VideoBitRate = ffMpegInteropMss.VideoStreams[0].Bitrate.ToString();
                m_mediaInfosCollector.MediaInfo.VideoCodec = ffMpegInteropMss.VideoStreams[0].CodecName;
                m_mediaInfosCollector.MediaInfo.Fps = ffMpegInteropMss.VideoStreams[0].FramesPerSecond;
                m_mediaInfosCollector.MediaInfo.VideoHeight = ffMpegInteropMss.VideoStreams[0].PixelHeight;
                m_mediaInfosCollector.MediaInfo.VideoWidth = ffMpegInteropMss.VideoStreams[0].PixelWidth;
            }

            if (ffMpegInteropMss.AudioStreams.Count > 0)
            {
                m_mediaInfosCollector.MediaInfo.AudioBitRate = ffMpegInteropMss.AudioStreams[0].Bitrate.ToString();
                m_mediaInfosCollector.MediaInfo.AudioCodec = ffMpegInteropMss.AudioStreams[0].CodecName;
            }
            m_mediaInfosCollector.EmitUpdateMediaInfos();
        }

        public override void InternalStop()
        {
        }
    }
}
