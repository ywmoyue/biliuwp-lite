using System.Threading.Tasks;
using BiliLite.Player.Controllers;

namespace BiliLite.Player.States.MuteStates
{
    public class UnMutedState : IMuteState
    {
        public UnMutedState(BasePlayerController playerController, MuteStateHandler stateHandler) : base(playerController, stateHandler)
        {
        }

        public override async Task Mute()
        {
            m_stateHandler.ChangeToMuted();
            await m_playerController.SetMuted(true);
        }
    }
}
