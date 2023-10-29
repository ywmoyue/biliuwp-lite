using BiliLite.Services;

namespace BiliLite.Player.MediaInfos
{
    public abstract class BaseCollectInfoHandler
    {
        protected readonly MediaInfosCollector m_mediaInfosCollector;
        protected static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        public BaseCollectInfoHandler(MediaInfosCollector mediaInfosCollector)
        {
            m_mediaInfosCollector = mediaInfosCollector;
        }

        public void Start(CollectInfo collectInfo)
        {
            _logger.Debug($"{GetType().Name}");
            InternalStart(collectInfo);
        }

        public void Stop()
        {
            _logger.Debug($"{GetType().Name}");
            InternalStop();
        }

        public abstract void InternalStart(CollectInfo collectInfo);

        public abstract void InternalStop();
    }
}
