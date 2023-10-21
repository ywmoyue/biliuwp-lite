using BiliLite.Player.States.PlayStates;
using System;
using BiliLite.Player.States.PauseStates;

namespace BiliLite.Player.Controllers
{
    public class BasePlayerController
    {
        private readonly PlayStateHandler m_playStateHandler;
        private readonly PauseStateHandler m_pauseStateHandler;

        public BasePlayerController()
        {
            m_playStateHandler = new PlayStateHandler(this);
            m_pauseStateHandler = new PauseStateHandler(this);
            InitEvent();
        }

        public event EventHandler<PlayStateChangedEventArgs> PlayStateChanged;

        public event EventHandler<PauseStateChangedEventArgs> PauseStateChanged;

        public IBiliPlayer2 Player { get; set; }

        public IPlayState PlayState { get; set; }

        public IPauseState PauseState { get; set; }

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
        }

        public void SetPlayer(IBiliPlayer2 player)
        {
            Player = player;
        }
    }
}
