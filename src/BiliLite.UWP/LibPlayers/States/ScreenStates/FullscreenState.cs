using System.Threading.Tasks;
using BiliLite.Player.Controllers;

namespace BiliLite.Player.States.ScreenStates
{
    public class FullscreenState : IScreenState
    {
        public FullscreenState(BasePlayerController playerController, ScreenStateHandler stateHandler) : base(playerController, stateHandler)
        {
        }

        public override bool IsFullscreen => true;

        public override async Task CancelFullscreen()
        {
            m_stateHandler.ChangeToNormalScreen();
            await m_playerController.Player.CancelFullscreen();
        }
    }
}
