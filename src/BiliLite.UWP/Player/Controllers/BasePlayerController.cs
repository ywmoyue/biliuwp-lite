using BiliLite.Player.States.PlayStates;
using System;
using BiliLite.Player.MediaInfos;
using BiliLite.Player.States.ContentStates;
using BiliLite.Player.States.PauseStates;
using BiliLite.Player.States.ScreenStates;

namespace BiliLite.Player.Controllers
{
    public class BasePlayerController
    {
        private readonly PlayStateHandler m_playStateHandler;
        private readonly PauseStateHandler m_pauseStateHandler;
        private readonly ContentStateHandler m_contentStateHandler;
        private readonly ScreenStateHandler m_screenStateHandler;
        private readonly MediaInfosCollector m_mediaInfosCollector;

        public BasePlayerController()
        {
            m_playStateHandler = new PlayStateHandler(this);
            m_pauseStateHandler = new PauseStateHandler(this);
            m_contentStateHandler = new ContentStateHandler(this);
            m_screenStateHandler = new ScreenStateHandler(this);
            m_mediaInfosCollector = new MediaInfosCollector(this);
            InitEvent();
        }

        public event EventHandler<PlayStateChangedEventArgs> PlayStateChanged;

        public event EventHandler<PauseStateChangedEventArgs> PauseStateChanged;

        public event EventHandler<ContentStateChangedEventArgs> ContentStateChanged;

        public event EventHandler<ScreenStateChangedEventArgs> ScreenStateChanged;

        public event EventHandler<MediaInfo> MediaInfosUpdated;

        public IBiliPlayer2 Player { get; set; }

        public IPlayState PlayState { get; set; }

        public IPauseState PauseState { get; set; }

        public IContentState ContentState { get; set; }

        public IScreenState ScreenState { get; set; }

        private void InitEvent()
        {
            m_playStateHandler.PlayStateChanged += (sender, e) =>
            {
                PlayStateChanged?.Invoke(this, e);
            };
            m_pauseStateHandler.PauseStateChanged += (sender, e) =>
            {
                PauseStateChanged?.Invoke(this, e);
            };
            m_contentStateHandler.ContentStateChanged += (sender, e) =>
            {
                ContentStateChanged?.Invoke(this, e);
            };
            m_screenStateHandler.ScreenStateChanged += (sender, e) =>
            {
                ScreenStateChanged?.Invoke(this, e);
            };
            m_mediaInfosCollector.MediaInfosUpdated += MediaInfosCollector_MediaInfosUpdated;
        }

        private void MediaInfosCollector_MediaInfosUpdated(object sender, MediaInfo e)
        {
            MediaInfosUpdated?.Invoke(this, e);
        }

        public void SetPlayer(IBiliPlayer2 player)
        {
            Player = player;
        }

        public void UpdateMediaInfo()
        {
            m_mediaInfosCollector.StartCollect();
        }
    }
}
