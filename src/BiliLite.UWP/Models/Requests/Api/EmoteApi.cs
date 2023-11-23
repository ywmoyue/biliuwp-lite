using BiliLite.Services;

namespace BiliLite.Models.Requests.Api
{
    public enum EmoteBusiness
    {
        /// <summary>
        /// 评论区
        /// </summary>
        reply,
        /// <summary>
        /// 动态
        /// </summary>
        dynamic
    }

    public class EmoteApi : BaseApi
    {
        public ApiModel UserEmote(EmoteBusiness business)
        {
            var type = business.ToString();
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/emote/user/panel/web",
                parameter = ApiHelper.MustParameter(AppKey) + $"&business={type}",
                headers = ApiHelper.GetAuroraHeaders(),
                need_cookie = true,
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }
    }
}
