using System.Collections.Generic;

namespace BiliLite.gRPC.Api
{
    internal class IosGrpcRequestHeader : IGrpcRequestHeader
    {
        public Dictionary<string, string> GetHeaders(GrpcBiliUserInfo userInfo)
        {
            var config = new IosGrpcRequestHeaderConfig(userInfo);
            var ua = $"bili-universal/{config.Build} os/ios model/{config.Model} mobi_app/{config.MobiApp} osVer/{config.OsVer} network/2";
            return new Dictionary<string, string>
            {
                { "User-Agent", ua },
                {"x-bili-gaia-vtoken",""},
                { "x-bili-aurora-eid", config.GetAuroraEid() },
                { "x-bili-mid", userInfo.UserMid.ToString() },
                { "x-bili-aurora-zone", "" },
                { "x-bili-trace-id", config.GenTraceId() },
                {"authorization",config.GetAuthorization()},
                { "buvid", config.Buvid },
                { "x-bili-fawkes-req-bin", config.GetFawkesreqBin() },
                { "x-bili-metadata-bin", config.GetMetadataBin() },
                { "x-bili-device-bin", config.GetDeviceBin() },        
                { "x-bili-network-bin", config.GetNetworkBin() },
                { "x-bili-restriction-bin", "" },
                { "x-bili-locale-bin", config.GetLocaleBin() },
                { "x-bili-exps-bin", "" },
                { "te", "trailers" },
            };
        }
    }
}
