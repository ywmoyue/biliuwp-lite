namespace BiliLite.Player.MediaInfos;

public class ShakaPlayerCollectInfoHandler : BaseCollectInfoHandler
{
    private ShakaPlayerCollectInfoData m_collectInfo;

    public ShakaPlayerCollectInfoHandler(MediaInfosCollector mediaInfosCollector) : base(mediaInfosCollector)
    {
    }

    public override void InternalStart(CollectInfo collectInfo)
    {
        if (!(collectInfo.Data is ShakaPlayerCollectInfoData collectInfoData)) return;
        m_collectInfo = collectInfoData;
        collectInfoData.WebPlayer.StatsUpdated += WebPlayer_StatsUpdated;
    }

    public override void InternalStop()
    {
        m_collectInfo.WebPlayer.StatsUpdated -= WebPlayer_StatsUpdated;
    }

    private void WebPlayer_StatsUpdated(object sender, WebPlayer.Models.WebPlayerStatsUpdatedData e)
    {
        m_mediaInfosCollector.MediaInfo.PlayerType = m_collectInfo.WebPlayer.Type.ToString();
        m_mediaInfosCollector.MediaInfo.DroppedFrames = e.DroppedFrames;
        m_mediaInfosCollector.MediaInfo.DecodedFrames = e.DecodedFrames;
        m_mediaInfosCollector.MediaInfo.VideoHeight = e.Height;
        m_mediaInfosCollector.MediaInfo.VideoWidth = e.Width;
        m_mediaInfosCollector.MediaInfo.VideoBitRate = e.BpsVideo;
        m_mediaInfosCollector.MediaInfo.AudioBitRate = e.BpsAudio;
        m_mediaInfosCollector.MediaInfo.VideoCodec = e.VideoCodec;
        m_mediaInfosCollector.MediaInfo.AudioCodec = e.AudioCodec;
        m_mediaInfosCollector.MediaInfo.Speed = e.Speed;
        m_mediaInfosCollector.EmitUpdateMediaInfos();
    }
}