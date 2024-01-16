using BiliLite.Services;

namespace BiliLite.Models.Requests.Api.Home
{
    public class HotAPI : BaseApi
    {
        public ApiModel Popular(string idx = "0", string last_param = "")
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://app.bilibili.com/x/v2/show/popular/index",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&idx={idx}&last_param={last_param}"
            };
            api.parameter = api.parameter.Replace("mobi_app=iphone", "mobi_app=android");
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

    }
}
