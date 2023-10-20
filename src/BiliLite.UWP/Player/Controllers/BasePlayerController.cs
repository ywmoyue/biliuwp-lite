using BiliLite.Player.States.PlayStates;
using System;
using BiliLite.Player.States.PauseStates;

namespace BiliLite.Player.Controllers
{
    public class BasePlayerController
    {
        private readonly PlayStateHandler m_playStateHandler;

        public BasePlayerController()
        {
            m_playStateHandler = new PlayStateHandler(this);
            InitEvent();
        }

        public event EventHandler<PlayStateChangedEventArgs> PlayStateChanged;

        public IBiliPlayer2 Player { get; set; }

        public IPlayState PlayState { get; set; }

        public IPauseState PauseState { get; set; }

        private void InitEvent()
        {
            m_playStateHandler.PlayStateChanged += (sender, e) =>
            {
                PlayStateChanged?.Invoke(this, e);
            };
        }

        public void SetPlayer(IBiliPlayer2 player)
        {
            Player = player;
        }
    }
}
