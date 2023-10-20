using BiliLite.Models.Common.Player;
using System;
using static BiliLite.Models.Common.Player.PlayerError;

namespace BiliLite.Models.Exceptions
{
    public class PlayerException : Exception
    {
        public PlayerException(PlayerErrorCode errorCode, string description, RetryStrategy retryStrategy)
        {
            Code = errorCode;
            Description = description;
            RetryStrategy = retryStrategy;
        }

        public PlayerErrorCode Code { get; set; }

        public string Description { get; set; }

        RetryStrategy RetryStrategy { get; set; }

        public static PlayerException Create(PlayerErrorCode errorCode, string description, RetryStrategy retryStrategy = RetryStrategy.NoRetry)
        {
            return new PlayerException(errorCode, description, retryStrategy);
        }
    }
}
