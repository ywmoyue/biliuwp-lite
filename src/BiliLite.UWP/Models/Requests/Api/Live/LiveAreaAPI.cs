using BiliLite.Models.Common;
using BiliLite.Services;

namespace BiliLite.Models.Requests.Api.Live
{
    public class LiveAreaAPI : BaseApi
    {
        public ApiModel LiveAreaList()
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.live.bilibili.com/room/v1/Area/getList",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&actionKey=appkey&need_entrance=1&parent_id=0"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        public ApiModel LiveAreaRoomList(int area_id = 0, int parent_area_id = 0, int page = 1, string sort_type = "online")
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.live.bilibili.com/room/v3/Area/getRoomList",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&actionKey=appkey&area_id={area_id}&cate_id=0&parent_area_id={parent_area_id}&page={page}&page_size=36&sort_type={sort_type}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }
    }
}
