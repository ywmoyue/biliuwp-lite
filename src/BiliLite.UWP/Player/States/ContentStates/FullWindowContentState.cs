using System.Threading.Tasks;
using BiliLite.Player.Controllers;

namespace BiliLite.Player.States.ContentStates
{
    public class FullWindowContentState : IContentState
    {
        public FullWindowContentState(BasePlayerController playerController, ContentStateHandler stateHandler) : base(playerController, stateHandler)
        {
        }

        public override bool IsFullWindow => true;

        public override async Task CancelFullWindow()
        {
            m_stateHandler.ChangeToNormalWindow();
            await m_playerController.Player.CancelFullWindow();
        }
    }
}
