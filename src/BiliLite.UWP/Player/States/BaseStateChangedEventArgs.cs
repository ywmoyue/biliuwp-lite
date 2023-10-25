namespace BiliLite.Player.States
{
    public abstract class BaseStateChangedEventArgs<TState>
    {
        protected BaseStateChangedEventArgs(TState newState, TState oldState)
        {
            NewState = newState;
            OldState = oldState;
        }

        public TState NewState { get; set; }

        public TState OldState { get; set; }
    }
}
