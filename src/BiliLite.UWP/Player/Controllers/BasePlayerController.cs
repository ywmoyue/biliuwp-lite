using BiliLite.Player.States.PlayStates;
using System;
using BiliLite.Player.MediaInfos;
using BiliLite.Player.States.ContentStates;
using BiliLite.Player.States.MuteStates;
using BiliLite.Player.States.PauseStates;
using BiliLite.Player.States.ScreenStates;
using BiliLite.Models.Exceptions;
using System.Collections.Generic;

namespace BiliLite.Player.Controllers
{
    public class BasePlayerController : IDisposable
    {
        private const int DefaultMaxErrorRecordCount = 20;
        private readonly PlayStateHandler m_playStateHandler;
        private readonly PauseStateHandler m_pauseStateHandler;
        private readonly ContentStateHandler m_contentStateHandler;
        private readonly MuteStateHandler m_muteStateHandler;
        private readonly ScreenStateHandler m_screenStateHandler;
        private readonly MediaInfosCollector m_mediaInfosCollector;
        private readonly List<PlayerErrorRecord> m_errorList;

        public BasePlayerController()
        {
            m_playStateHandler = new PlayStateHandler(this);
            m_pauseStateHandler = new PauseStateHandler(this);
            m_contentStateHandler = new ContentStateHandler(this);
            m_muteStateHandler = new MuteStateHandler(this);
            m_screenStateHandler = new ScreenStateHandler(this);
            m_mediaInfosCollector = new MediaInfosCollector(this);
            m_errorList = new List<PlayerErrorRecord>();
            InitEvent();
        }

        public event EventHandler<PlayStateChangedEventArgs> PlayStateChanged;

        public event EventHandler<PauseStateChangedEventArgs> PauseStateChanged;

        public event EventHandler<ContentStateChangedEventArgs> ContentStateChanged;

        public event EventHandler<MuteStateChangedEventArgs> MuteStateChanged;

        public event EventHandler<ScreenStateChangedEventArgs> ScreenStateChanged;

        public event EventHandler<MediaInfo> MediaInfosUpdated;

        public event EventHandler<int> RetryCountAdded;

        public event EventHandler<PlayerErrorRecord> ErrorPushed;

        public IBiliPlayer2 Player { get; set; }

        public IPlayState PlayState { get; set; }

        public IPauseState PauseState { get; set; }

        public IContentState ContentState { get; set; }

        public IMuteState MuteState { get; set; }

        public IScreenState ScreenState { get; set; }

        public int RetryCount { get; private set; }

        public IReadOnlyList<PlayerErrorRecord> ErrorList => m_errorList.AsReadOnly();

        private void InitEvent()
        {
            m_playStateHandler.PlayStateChanged += (sender, e) =>
            {
                if (e.NewState.IsLoading && e.OldState != null && !e.OldState.IsIdle)
                {
                    RetryCount++;
                    RetryCountAdded?.Invoke(this, RetryCount);
                }
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
            m_muteStateHandler.MuteStateChanged += (sender, e) =>
            {
                MuteStateChanged?.Invoke(this, e);
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

        public async System.Threading.Tasks.Task SetRate(double value)
        {
            if (Player == null)
            {
                return;
            }

            await Player.SetRate(value);
        }

        public async System.Threading.Tasks.Task SetMuted(bool muted)
        {
            if (Player == null)
            {
                return;
            }

            await Player.SetMuted(muted);
        }

        public void PushError(PlayerException exception)
        {
            if (exception == null)
            {
                return;
            }

            var record = new PlayerErrorRecord(exception.Code, exception.Description, exception.RetryStrategy);
            m_errorList.Add(record);
            if (m_errorList.Count > DefaultMaxErrorRecordCount)
            {
                m_errorList.RemoveAt(0);
            }

            ErrorPushed?.Invoke(this, record);
        }

        public void Dispose()
        {
            m_mediaInfosCollector.MediaInfosUpdated -= MediaInfosCollector_MediaInfosUpdated;
            m_mediaInfosCollector.Dispose();
        }
    }
}
