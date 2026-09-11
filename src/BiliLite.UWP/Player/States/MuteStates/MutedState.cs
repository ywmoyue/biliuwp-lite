using System.Threading.Tasks;
using BiliLite.Player.Controllers;

namespace BiliLite.Player.States.MuteStates
{
    public class MutedState : IMuteState
    {
        public MutedState(BasePlayerController playerController, MuteStateHandler stateHandler) : base(playerController, stateHandler)
        {
        }

        public override bool IsMuted => true;

        public override async Task UnMute()
        {
            m_stateHandler.ChangeToUnMuted();
            await m_playerController.SetMuted(false);
        }
    }
}
