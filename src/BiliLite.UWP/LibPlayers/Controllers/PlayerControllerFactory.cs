using System;
using System.Collections.Generic;

namespace BiliLite.Player.Controllers
{
    public static class PlayerControllerFactory
    {
        public static BasePlayerController Create(PlayerType type)
        {
            var createPlayerControllerMap = new Dictionary<PlayerType, Func<BasePlayerController>>()
            {
                { PlayerType.Live, () => new LivePlayerController() }
            };
            var success = createPlayerControllerMap.TryGetValue(type, out var createPlayerControllerAction);
            return !success ? null : createPlayerControllerAction();
        }
    }
}
