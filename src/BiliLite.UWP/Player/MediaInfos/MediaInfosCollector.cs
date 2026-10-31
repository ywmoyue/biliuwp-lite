using System;
using System.Collections.Generic;
using System.Linq;
using BiliLite.Player.Controllers;

namespace BiliLite.Player.MediaInfos
{
    public class MediaInfosCollector : IDisposable
    {
        private readonly BasePlayerController m_playerController;
        private readonly Dictionary<string, BaseCollectInfoHandler> m_collectInfoHandlerMap;
        private BaseCollectInfoHandler m_currentCollectInfoHandler;

        public MediaInfosCollector(BasePlayerController playerController)
        {
            m_playerController = playerController;
            m_playerController.PlayStateChanged += PlayerController_PlayStateChanged;
            m_collectInfoHandlerMap = new Dictionary<string, BaseCollectInfoHandler>()
            {
                { "LiveHls", new FFMpegInteropMssCollectInfoHandler(this) },
                { "ShakaPlayer", new ShakaPlayerCollectInfoHandler(this) },
                { "DashNative", new MediaPlayerCollectInfoHandler(this) },
                { "Mp4Native", new MediaPlayerCollectInfoHandler(this) },
                { "FlvSyEngine", new MediaPlayerCollectInfoHandler(this) },
                { "MultiFlvSyEngine", new MediaPlayerCollectInfoHandler(this) }
            };
        }

        public event EventHandler<MediaInfo> MediaInfosUpdated;

        public MediaInfo MediaInfo { get; set; } = new MediaInfo();

        private void PlayerController_PlayStateChanged(object sender, States.PlayStates.PlayStateChangedEventArgs e)
        {
            if (e.NewState.IsPlaying)
            {
                StartCollect();
            }
            else
            {
                StopCollect();
            }
        }

        private CollectInfo GetCollectInfo()
        {
            var collectInfo = m_playerController.Player.GetCollectInfo();
            if (collectInfo == null)
            {
                return null;
            }

            MediaInfo.PlayerType = collectInfo.Type;
            MediaInfo.Url = collectInfo.Url;
            return collectInfo;
        }

        private void StartCollect()
        {
            var collectInfo = GetCollectInfo();
            if (collectInfo == null)
            {
                return;
            }

            var success = m_collectInfoHandlerMap.TryGetValue(collectInfo.Type, out var handler);
            if (!success)
            {
                throw new Exception($"unknown collect info type: {collectInfo.Type}");
            }

            m_currentCollectInfoHandler = handler;
            m_currentCollectInfoHandler.Start(collectInfo);
        }

        private void StopCollect()
        {
            m_currentCollectInfoHandler?.Stop();
        }

        public void EmitUpdateMediaInfos()
        {
            MediaInfosUpdated?.Invoke(this, MediaInfo);
        }

        public void Dispose()
        {
            m_playerController.PlayStateChanged -= PlayerController_PlayStateChanged;
            StopCollect();
            m_currentCollectInfoHandler = null;

            foreach (var handler in m_collectInfoHandlerMap.Values.Distinct())
            {
                if (handler is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}
