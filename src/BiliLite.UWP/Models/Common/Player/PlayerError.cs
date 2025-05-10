namespace BiliLite.Models.Common.Player
{
    public static class PlayerError
    {
        public enum PlayerErrorCode
        {
            NetworkError,
            PlayUrlError,
            NeedUseOtherPlayerError,
            UnknownError,  
        }

        /// <summary>
        /// 重试策略
        /// </summary>
        public enum RetryStrategy
        {
            /// <summary>
            /// 正常重试
            /// </summary>
            Normal,

            /// <summary>
            /// 不重试
            /// </summary>
            NoRetry,

            /// <summary>
            /// 无错误
            /// </summary>
            NoError,
        }
    }
}
