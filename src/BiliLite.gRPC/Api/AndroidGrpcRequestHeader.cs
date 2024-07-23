using System;
using System.Collections.Generic;

namespace BiliLite.gRPC.Api
{
    internal class AndroidGrpcRequestHeader : IGrpcRequestHeader
    {
        public Dictionary<string, string> GetHeaders(GrpcBiliUserInfo userInfo)
        {
            var config = new AndroidGrpcRequestHeaderConfig(userInfo);
            var ua = $"Dalvik/{AndroidGrpcRequestHeaderConfig.dalvik_ver} "
                     + $"(Linux; U; Android {AndroidGrpcRequestHeaderConfig.os_ver}; {AndroidGrpcRequestHeaderConfig.brand} {AndroidGrpcRequestHeaderConfig.model}) {AndroidGrpcRequestHeaderConfig.app_ver} "
                     + $"os/android model/{AndroidGrpcRequestHeaderConfig.model} mobi_app/android build/{AndroidGrpcRequestHeaderConfig.build} "
                     + $"channel/{AndroidGrpcRequestHeaderConfig.channel} innerVer/{AndroidGrpcRequestHeaderConfig.build} osVer/{AndroidGrpcRequestHeaderConfig.os_ver} "
                     + $"network/{AndroidGrpcRequestHeaderConfig.network_type}";
            return new Dictionary<string, string>
            {
                {"User-Agent",ua},
                {"APP-KEY","android"},
                {"x-bili-metadata-bin",config.GetMetadataBin()},
                {"authorization","identify_v1 " + userInfo.AccessKey},
                {"x-bili-device-bin",config.GetDeviceBin()},
                {"x-bili-network-bi",config.GetNetworkBin()},
                {"x-bili-restriction-bin",""},
                {"x-bili-locale-bin",config.GetLocaleBin()},
                {"x-bili-fawkes-req-bin",config.GetFawkesreqBin()},
                {"grpc-accept-encoding","identity"},
                {"grpc-timeout","17985446u"},
                {"env","prod"},
                {"Transfer-Encoding","chunked"},
                {"TE","trailers"},
            };
        }
    }
}
