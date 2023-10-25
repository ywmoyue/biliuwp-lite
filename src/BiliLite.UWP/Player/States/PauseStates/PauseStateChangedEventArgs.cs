namespace BiliLite.Player.States.PauseStates
{
    public class PauseStateChangedEventArgs : BaseStateChangedEventArgs<IPauseState>
    {
        public PauseStateChangedEventArgs(IPauseState newState, IPauseState oldState) : base(newState, oldState)
        {
        }
    }
}
