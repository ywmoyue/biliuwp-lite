using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using Bilibili.Metadata;
using Bilibili.Metadata.Device;
using Bilibili.Metadata.Fawkes;
using Bilibili.Metadata.Locale;
using Google.Protobuf;
using Bilibili.Metadata.Network;

namespace BiliLite.gRPC.Api
{
    internal class IosGrpcRequestHeaderConfig
    {
        private readonly GrpcBiliUserInfo m_userinfo;
        private const string MOBI_APP = "iphone";
        private const string PLATFORM = "ios";
        private const string BUVID = "Y3436F391696935149CCA03C1537118D2080";
        private const string ENV = "prod";
        private const string BRAND = "Apple";
        private const string MODEL = "iPad 9G";
        private const string OS_VER = "15.5";
        private const string VERSION_NAME = "7.67.0";
        private const string CHANNEL = "apple";
        private const int BUILD = 76700100;
        private const string DEVICE = "pad";
        private const int APP_ID = 1;
        private const string REGION = "CN";
        private const string LANGUAGE = "zh";

        public IosGrpcRequestHeaderConfig(GrpcBiliUserInfo userInfo)
        {
            m_userinfo = userInfo;
        }

        public string Buvid => BUVID;

        public int Build => BUILD;

        public string Model => MODEL;

        public string MobiApp => MOBI_APP;

        public string OsVer => OS_VER;

        public string GetLocaleBin()
        {
            var msg = new Locale
            {
                CLocale = new LocaleIds
                {
                    Language = LANGUAGE,
                    Region = REGION
                },
                SLocale = new LocaleIds
                {
                    Language = LANGUAGE,
                    Region = REGION
                }
            };
            return ToBase64(msg.ToByteArray());
        }

        public string GetNetworkBin()
        {
            var msg = new Network
            {
                Type = NetworkType.Wifi
            };
            return ToBase64(msg.ToByteArray());
        }

        public string GetDeviceBin()
        {
            var msg = new Device
            {
                AppId = APP_ID,
                Build = BUILD,
                Buvid = BUVID,
                MobiApp = MOBI_APP,
                Platform = PLATFORM,
                Device_ = DEVICE,
                Channel = CHANNEL,
                Brand = BRAND,
                Model = MODEL,
                Osver = OS_VER
            };
            return ToBase64(msg.ToByteArray());
        }

        public string GetMetadataBin()
        {
            var msg = new Metadata
            {
                AccessKey = m_userinfo.AccessKey,
                MobiApp = MOBI_APP,
                Device = DEVICE,
                Build = BUILD,
                Channel = CHANNEL,
                Buvid = BUVID,
                Platform = PLATFORM
            };
            return ToBase64(msg.ToByteArray());
        }

        public string GetFawkesreqBin()
        {
            var msg = new FawkesReq();
            msg.Appkey = MOBI_APP;
            msg.Env = ENV;
            return ToBase64(msg.ToByteArray());
        }

        public string GetAuroraEid()
        {
            if (m_userinfo.UserMid == 0) return "";
            var resultByte = new List<byte>(64);
            var midByte = Encoding.UTF8.GetBytes(m_userinfo.UserMid.ToString());
            resultByte.AddRange(midByte.Select((t, i) => (byte)(t ^ (new byte[] { 97, 100, 49, 118, 97, 52, 54, 97, 55, 108, 122, 97 }[i % 12]))));

            return ToBase64(resultByte.ToArray()).TrimEnd('=');
        }

        public string GetAuthorization()
        {
            return string.IsNullOrEmpty(m_userinfo.AccessKey) ? "" : $"identify_v1 {m_userinfo.AccessKey}";
        }

        public string GenTraceId()
        {
            var randomId = GenRandomString(32);
            var randomTraceId = new StringBuilder(40);
            randomTraceId.Append(randomId.Substring(0, 24));
            var bArr = new byte[3];
            var ts = DateTimeOffset.Now.ToUnixTimeSeconds();
            for (var i = bArr.Length - 1; i >= 0; i--)
            {
                ts >>= 8;
                bArr[i] = (byte)((ts / 128) % 2 == 0 ? ts % 256 : ts % 256 - 256);
            }
            foreach (var b in bArr)
            {
                randomTraceId.Append(b.ToString("x2"));
            }
            randomTraceId.Append(randomId.Substring(30, 2));
            var randomTraceIdFinal = new StringBuilder(64);
            randomTraceIdFinal.Append(randomTraceId);
            randomTraceIdFinal.Append(":");
            randomTraceIdFinal.Append(randomTraceId.ToString().Substring(16, 16));
            randomTraceIdFinal.Append(":0:0");
            return randomTraceIdFinal.ToString();
        }

        private string GenRandomString(int length)
        {
            var chars = "0123456789abcdefghijklmnopqrstuvwxyz";
            var sb = new StringBuilder(length);
            var rnd = new Random();
            for (var i = 0; i < length; i++)
            {
                sb.Append(chars[rnd.Next(chars.Length)]);
            }
            return sb.ToString();
        }

        private string ToBase64(byte[] data)
        {
            return Convert.ToBase64String(data).TrimEnd('=');
        }
    }
}
