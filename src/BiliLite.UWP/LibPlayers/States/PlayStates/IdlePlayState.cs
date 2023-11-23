using System.Threading.Tasks;
using BiliLite.Player.Controllers;

namespace BiliLite.Player.States.PlayStates
{
    public class IdlePlayState : IPlayState
    {
        public IdlePlayState(BasePlayerController playerController, PlayStateHandler stateHandler) : base(playerController, stateHandler)
        {
        }

        public override bool IsIdle => true;

        public override async Task Load()
        {
            m_stateHandler.ChangeToLoading();
            await m_playerController.Player.Load();
        }
    }
}
