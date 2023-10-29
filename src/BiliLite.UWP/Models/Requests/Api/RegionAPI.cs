using BiliLite.Services;

namespace BiliLite.Models.Requests.Api
{
    public class RegionAPI : BaseApi
    {
        public ApiModel Regions()
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://app.bilibili.com/x/v2/region/index",
                parameter = ApiHelper.MustParameter(AppKey, true)
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        public ApiModel RegionDynamic(int rid)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://app.bilibili.com/x/v2/region/dynamic",
                parameter = ApiHelper.MustParameter(AppKey, false) + $"&rid={rid}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }
        public ApiModel RegionDynamic(int rid, string next_aid)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://app.biliapi.net/x/v2/region/dynamic/list",
                parameter = ApiHelper.MustParameter(AppKey, false) + $"&rid={rid}&ctime={next_aid}&pull=false"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }


        public ApiModel RegionChildDynamic(int rid, int tag_id = 0)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://app.biliapi.net/x/v2/region/dynamic/child",
                parameter = ApiHelper.MustParameter(AppKey, false) + $"&rid={rid}&tag_id={tag_id}&pull=true"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        public ApiModel RegionChildDynamic(int rid, string next, int tag_id = 0)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://app.bilibili.com/x/v2/region/dynamic/child/list",
                parameter = ApiHelper.MustParameter(AppKey, false) + $"&rid={rid}&tag_id={tag_id}&pull=false&ctime={next}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }
        public ApiModel RegionChildList(int rid, string order, int page, int tag_id = 0)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://app.biliapi.net/x/v2/region/show/child/list",
                parameter = ApiHelper.MustParameter(AppKey, false) + $"&order={order}&pn={page}&ps=20&rid={rid}&tag_id={tag_id}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

    }
}
