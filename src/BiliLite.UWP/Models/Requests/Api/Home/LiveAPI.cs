using BiliLite.Models.Common;
using BiliLite.Services;

namespace BiliLite.Models.Requests.Api.Home
{
    public class LiveAPI : BaseApi
    {
        public ApiModel LiveHome()
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://api.live.bilibili.com/xlive/app-interface/v2/index/getAllList",
                parameter = ApiHelper.MustParameter(AppKey, true) + "&device=android&rec_page=1&relation_page=1&scale=xxhdpi",
            };
            api.parameter = api.parameter.Replace("mobi_app=iphone", "mobi_app=android"); //只有mobi_app=android才能拿到area_entrance_v2
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        public ApiModel LiveHomeItems()
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://api.live.bilibili.com/xlive/web-interface/v1/index/getList",
                parameter = "platform=web"
            };
            return api;
        }
    }
}
