using System.Threading.Tasks;
using BiliLite.Player.Controllers;

namespace BiliLite.Player.States.PauseStates
{
    public class PausedState : IPauseState
    {
        public PausedState(BasePlayerController playerController, PauseStateHandler stateHandler) : base(playerController, stateHandler)
        {
        }

        public override bool IsPaused => true;

        public override async Task Resume(IBiliPlayer2 player)
        {
            m_stateHandler.ChangeToResume();
            await m_playerController.Player.Resume();
        }
    }
}
