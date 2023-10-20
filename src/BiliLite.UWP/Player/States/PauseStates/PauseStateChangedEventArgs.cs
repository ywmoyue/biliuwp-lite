namespace BiliLite.Player.States.PauseStates
{
    public class PauseStateChangedEventArgs
    {
        public PauseStateChangedEventArgs(IPauseState newState, IPauseState oldState)
        {
            NewState = newState;
            OldState = oldState;
        }

        public IPauseState NewState { get; set; }

        public IPauseState OldState { get; set; }
    }
}
