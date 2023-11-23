namespace BiliLite.Player.States.ContentStates
{
    public class ContentStateChangedEventArgs : BaseStateChangedEventArgs<IContentState>
    {
        public ContentStateChangedEventArgs(IContentState newState, IContentState oldState) : base(newState, oldState)
        {
        }
    }
}
