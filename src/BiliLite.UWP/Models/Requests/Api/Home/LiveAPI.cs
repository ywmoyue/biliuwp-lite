using BiliLite.Services;

namespace BiliLite.Models.Requests.Api.Home
{
    public class LiveAPI : BaseApi
    {
        public ApiModel LiveHome()
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = "https://api.live.bilibili.com/xlive/app-interface/v2/index/getAllList",
                parameter = ApiHelper.MustParameter(AppKey, true) + "&device=android&rec_page=1&relation_page=1&scale=xxhdpi",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        public ApiModel LiveHomeItems()
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = "https://api.live.bilibili.com/xlive/web-interface/v1/index/getList",
                parameter = "platform=web"
            };
            return api;
        }
    }
}
