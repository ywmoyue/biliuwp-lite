using System.Threading.Tasks;
using BiliLite.Player.Controllers;

namespace BiliLite.Player.States.PlayStates
{
    public class LoadingPlayState : IPlayState
    {
        public LoadingPlayState(BasePlayerController playerController, PlayStateHandler stateHandler) : base(playerController, stateHandler)
        {
        }

        public override bool IsLoading => true;

        public override async Task Buff()
        {
            m_stateHandler.ChangeToBuffering();
            await m_playerController.Player.Buff();
        }

        public override async Task Stop()
        {
            m_stateHandler.ChangeToStoppd();
            await m_playerController.Player.Stop();
        }

        public override async Task Fault()
        {
            m_stateHandler.ChangeToFault();
            await m_playerController.Player.Fault();
        }
    }
}
