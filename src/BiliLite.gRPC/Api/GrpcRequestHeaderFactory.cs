using System.Collections.Generic;

namespace BiliLite.gRPC.Api
{
    internal static class GrpcRequestHeaderFactory
    {
        private static readonly Dictionary<string, IGrpcRequestHeader> _grpcRequestHeaders
            = new Dictionary<string, IGrpcRequestHeader>()
            {
                { Constants.ANDROID_APP_KEY, new AndroidGrpcRequestHeader() },
                { Constants.IOS_APP_KEY, new IosGrpcRequestHeader() }
            };

        public static IGrpcRequestHeader GetGrpcRequestHeader(string appKey)
        {
            return !_grpcRequestHeaders.TryGetValue(appKey, out var header) ? null : header;
        }
    }
}
