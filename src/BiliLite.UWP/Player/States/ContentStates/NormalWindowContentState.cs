using System.Threading.Tasks;
using BiliLite.Player.Controllers;

namespace BiliLite.Player.States.ContentStates
{
    public class NormalWindowContentState : IContentState
    {
        public NormalWindowContentState(BasePlayerController playerController, ContentStateHandler stateHandler) : base(playerController, stateHandler)
        {
        }

        public override bool IsFullWindow => false;

        public override async Task FullWindow()
        {
            m_stateHandler.ChangeToFullWindow();
            await m_playerController.Player.FullWindow();
        }
    }
}
