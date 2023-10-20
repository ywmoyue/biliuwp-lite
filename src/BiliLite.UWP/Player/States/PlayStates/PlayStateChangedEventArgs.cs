namespace BiliLite.Player.States.PlayStates
{
    public class PlayStateChangedEventArgs
    {
        public PlayStateChangedEventArgs(IPlayState newState, IPlayState oldState)
        {
            NewState = newState;
            OldState = oldState;
        }

        public IPlayState NewState { get; set; }

        public IPlayState OldState { get; set; }
    }
}
