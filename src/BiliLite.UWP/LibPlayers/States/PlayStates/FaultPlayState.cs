using System.Threading.Tasks;
using BiliLite.Player.Controllers;

namespace BiliLite.Player.States.PlayStates
{
    public class FaultPlayState : IPlayState
    {
        public FaultPlayState(BasePlayerController playerController, PlayStateHandler stateHandler) : base(playerController, stateHandler)
        {
        }
     
        public override bool IsFault => true;

        public override async Task Load()
        {
            await m_playerController.Player.Load();
        }
    }
}
