using BiliLite.Player.States.PauseStates;

namespace BiliLite.Player.States.PlayStates
{
    public class PlayStateChangedEventArgs : BaseStateChangedEventArgs<IPlayState>
    {
        public PlayStateChangedEventArgs(IPlayState newState, IPlayState oldState) : base(newState, oldState)
        {
        }
    }
}
