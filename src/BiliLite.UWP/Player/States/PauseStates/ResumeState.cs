using System.Threading.Tasks;
using BiliLite.Player.Controllers;

namespace BiliLite.Player.States.PauseStates
{
    public class ResumeState : IPauseState
    {
        public ResumeState(BasePlayerController playerController, PauseStateHandler stateHandler) : base(playerController, stateHandler)
        {
        }

        public override bool IsPaused => false;

        public override async Task Pause()
        {
            m_stateHandler.ChangeToPaused();
            await m_playerController.Player.Pause();
        }
    }
}
