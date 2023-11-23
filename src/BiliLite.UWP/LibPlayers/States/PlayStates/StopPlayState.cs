using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BiliLite.Player.Controllers;

namespace BiliLite.Player.States.PlayStates
{
    public class StopPlayState : IPlayState
    {
        public StopPlayState(BasePlayerController playerController, PlayStateHandler stateHandler) : base(playerController, stateHandler)
        {
        }

        public override bool IsStopped => true;

        public override async Task Load()
        {
            m_stateHandler.ChangeToLoading();
            await m_playerController.Player.Load();
        }
    }
}
