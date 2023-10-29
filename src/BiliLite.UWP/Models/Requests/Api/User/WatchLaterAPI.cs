using BiliLite.Services;

namespace BiliLite.Models.Requests.Api.User
{
    public class WatchLaterAPI : BaseApi
    {
        public ApiModel Add(string aid)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/history/toview/add",
                body = ApiHelper.MustParameter(AppKey, true) + $"&aid={aid}"
            };
            api.body += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }
        public ApiModel Watchlater()
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/history/toview",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&ps=100"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }
        public ApiModel Clear()
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/history/toview/clear",
                body = ApiHelper.MustParameter(AppKey, true)
            };
            api.parameter += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }
        public ApiModel Del()
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/history/toview/del",
                body = ApiHelper.MustParameter(AppKey, true) + "&viewed=true"
            };
            api.parameter += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }
        public ApiModel Del(string id)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/history/toview/del",
                body = ApiHelper.MustParameter(AppKey, true) + "&aid=" + id
            };
            api.parameter += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }
    }
}
