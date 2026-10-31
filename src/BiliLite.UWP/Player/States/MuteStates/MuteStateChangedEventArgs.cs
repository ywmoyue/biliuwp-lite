namespace BiliLite.Player.States.MuteStates
{
    public class MuteStateChangedEventArgs : BaseStateChangedEventArgs<IMuteState>
    {
        public MuteStateChangedEventArgs(IMuteState newState, IMuteState oldState) : base(newState, oldState)
        {
        }
    }
}
