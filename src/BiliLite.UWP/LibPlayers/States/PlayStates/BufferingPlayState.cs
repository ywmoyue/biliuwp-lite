using System.Threading.Tasks;
using BiliLite.Player.Controllers;

namespace BiliLite.Player.States.PlayStates
{
    public class BufferingPlayState : IPlayState
    {
        public BufferingPlayState(BasePlayerController playerController, PlayStateHandler stateHandler) : base(playerController, stateHandler)
        {
        }

        public override bool IsBuffering => true;

        public override async Task Play()
        {
            m_stateHandler.ChangeToPlaying();
            await m_playerController.Player.Play();
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
