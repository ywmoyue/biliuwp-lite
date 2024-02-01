using System;
using System.Collections.Generic;
using BiliLite.LibPlayers.MediaInfos;
using BiliLite.Player.Controllers;
using BiliLite.Services;

namespace BiliLite.Player.MediaInfos
{
    public class MediaInfosCollector
    {
        private readonly BasePlayerController m_playerController;
        private readonly Dictionary<string, BaseCollectInfoHandler> m_collectInfoHandlerMap;
        private BaseCollectInfoHandler m_currentCollectInfoHandler;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        public MediaInfosCollector(BasePlayerController playerController)
        {
            m_playerController = playerController;
            m_playerController.PlayStateChanged += PlayerController_PlayStateChanged;
            m_collectInfoHandlerMap = new Dictionary<string, BaseCollectInfoHandler>()
            {
                { "LiveHls", new FFMpegInteropMssCollectInfoHandler(this) }
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
            MediaInfo.PlayerType = collectInfo.Type;
            MediaInfo.Url = collectInfo.Url;
            return collectInfo;
        }

        private void StartCollect()
        {
            var collectInfo = GetCollectInfo();

            var success = m_collectInfoHandlerMap.TryGetValue(collectInfo.Type, out var handler);
            if (!success)
            {
                _logger.Error($"unknown collect info type: {collectInfo.Type}");
                return;
                // throw new Exception($"unknown collect info type: {collectInfo.Type}");
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
    }
}
