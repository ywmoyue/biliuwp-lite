namespace BiliLite.Models.Common
{
    public class ApiKeyInfo
    {
        public ApiKeyInfo(string key, string secret, string mobiApp, string userAgent)
        {
            Appkey = key;
            Secret = secret;
            MobiApp = mobiApp;
            UserAgent = userAgent;
        }
        public string Appkey { get; set; }

        public string Secret { get; set; }

        public string MobiApp { get; set; }

        public string UserAgent { get; set; }
    }
}