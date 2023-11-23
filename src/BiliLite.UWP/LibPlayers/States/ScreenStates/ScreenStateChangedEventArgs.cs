namespace BiliLite.Player.States.ScreenStates
{
    public class ScreenStateChangedEventArgs : BaseStateChangedEventArgs<IScreenState>
    {
        public ScreenStateChangedEventArgs(IScreenState newState, IScreenState oldState) : base(newState, oldState)
        {
        }
    }
}
