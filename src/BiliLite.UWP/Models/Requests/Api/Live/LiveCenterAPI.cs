using BiliLite.Services;

namespace BiliLite.Models.Requests.Api.Live
{
    public class LiveCenterAPI : BaseApi
    {
        public ApiModel FollowLive()
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = "https://api.live.bilibili.com/xlive/app-interface/v1/relation/liveAnchor",
                parameter = ApiHelper.MustParameter(AppKey, true) + "&qn=0&sortRule=0&filterRule=0",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        public ApiModel FollowUnLive(int page)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = "https://api.live.bilibili.com/xlive/app-interface/v1/relation/unliveAnchor",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&page={page}&pagesize=30",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        public ApiModel History(int page)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = "https://app.bilibili.com/x/v2/history/liveList",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&pn={page}&ps=20",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        public ApiModel SignInfo()
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = "https://api.live.bilibili.com/rc/v2/Sign/getSignInfo",
                parameter = ApiHelper.MustParameter(AppKey, true) + "&actionKey=appkey",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        public ApiModel DoSign()
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://api.live.bilibili.com/rc/v1/Sign/doSign",
                parameter = ApiHelper.MustParameter(AppKey, true) + "&actionKey=appkey"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }
    }

}
