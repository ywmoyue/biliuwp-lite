namespace BiliLite.gRPC.Api
{
    public class GrpcBiliUserInfo
    {
        public GrpcBiliUserInfo(string accessKey, long userMid, string cookies, string appKey)
        {
            AccessKey = accessKey;
            UserMid = userMid;
            Cookies = cookies;
            AppKey = appKey;
        }

        public GrpcBiliUserInfo(string accessKey, long userMid, string appKey)
        {
            AccessKey = accessKey;
            UserMid = userMid;
            AppKey = appKey;
        }

        public void GrpcBiliUserInfo1()
        {
        }

        public string AccessKey { get; set; }

        public long UserMid { get; set; }

        public string Cookies { get; set; }

        public string AppKey { get; set; }

        public string Build { get; set; }
    }
}
