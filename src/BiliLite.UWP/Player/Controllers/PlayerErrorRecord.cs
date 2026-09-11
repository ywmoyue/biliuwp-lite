using BiliLite.Models.Common.Player;

namespace BiliLite.Player.Controllers
{
    public class PlayerErrorRecord
    {
        public PlayerErrorRecord(PlayerError.PlayerErrorCode code, string description, PlayerError.RetryStrategy retryStrategy)
        {
            Code = code;
            Description = description;
            RetryStrategy = retryStrategy;
            Time = System.DateTimeOffset.Now;
        }

        public System.DateTimeOffset Time { get; }

        public PlayerError.PlayerErrorCode Code { get; }

        public string Description { get; }

        public PlayerError.RetryStrategy RetryStrategy { get; }
    }
}
