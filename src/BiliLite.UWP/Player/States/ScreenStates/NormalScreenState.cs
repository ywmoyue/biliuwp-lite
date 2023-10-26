using System.Threading.Tasks;
using BiliLite.Player.Controllers;

namespace BiliLite.Player.States.ScreenStates
{
    public class NormalScreenState : IScreenState
    {
        public NormalScreenState(BasePlayerController playerController, ScreenStateHandler stateHandler) : base(playerController, stateHandler)
        {
        }

        public override bool IsFullscreen => false;

        public override async Task Fullscreen()
        {
            m_stateHandler.ChangeToFullscreen();
            await m_playerController.Player.Fullscreen();
        }
    }
}
