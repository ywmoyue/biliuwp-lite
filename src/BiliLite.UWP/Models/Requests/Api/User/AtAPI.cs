using BiliLite.Services;
using System;
using BiliLite.Models.Common;

namespace BiliLite.Models.Requests.Api.User
{
    public class AtApi : BaseApi
    {
        public ApiModel RecommendAt(int page = 1, int pagesize = 20)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://api.vc.bilibili.com/dynamic_mix/v1/dynamic_mix/rcmd_at",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&need_attention=1&need_recent_at=1&page={page}&pagesize={pagesize}&teenagers_mode=0",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        public ApiModel SearchUser(string keyword, int page = 1, int pagesize = 20)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://app.bilibili.com/x/v2/search/user",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&keyword={Uri.EscapeDataString(keyword)}&order=totalrank&order_sort=0&pn={page}&ps={pagesize}",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }
    }
}
