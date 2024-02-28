using System;
using System.Collections.Generic;

namespace BiliLite.gRPC.Api
{
    internal interface IGrpcRequestHeader
    {
        Dictionary<string, string> GetHeaders(GrpcBiliUserInfo userInfo);
    }
}
