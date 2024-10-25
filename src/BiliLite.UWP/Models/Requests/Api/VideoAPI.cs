using BiliLite.Services;
using System;
using BiliLite.Models.Common;

namespace BiliLite.Models.Requests.Api
{
    public class VideoAPI : BaseApi
    {
        public ApiModel Detail(string id, bool isbvid)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://app.bilibili.com/x/v2/view",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&{(isbvid ? "bvid=" : "aid=")}{id}&plat=0"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        public ApiModel DetailWebInterface(string id, bool isBvId)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.bilibili.com/x/web-interface/view",
                parameter = $"&{(isBvId ? "bvid=" : "aid=")}{id}",
                need_cookie = true,
            };
            return api;
        }

        public ApiModel RelatesWebInterface(string id, bool isBvId)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.bilibili.com/x/web-interface/archive/related",
                parameter = $"&{(isBvId ? "bvid=" : "aid=")}{id}"
            };
            return api;
        }

        public ApiModel DetailProxy(string id, bool isbvid)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://app.bilibili.com/x/v2/view",
                parameter = ApiHelper.GetAccessParameter(AppKey) + $"&{(isbvid ? "bvid=" : "aid=")}{id}&plat=0"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            var apiUrl = Uri.EscapeDataString(api.url);
            api.baseUrl = "https://biliproxy.iill.moe/app.php";
            api.parameter = "url=" + apiUrl;
            return api;
        }
        /// <summary>
        ///点赞
        /// </summary>
        /// <param name="dislike"> 当前dislike状态</param>
        /// <param name="like">当前like状态</param>
        /// <returns></returns>
        public ApiModel Like(string aid, int dislike, int like)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = $"https://app.bilibili.com/x/v2/view/like",
                body = ApiHelper.MustParameter(AppKey, true) + $"&aid={aid}&dislike={dislike}&from=7&like={like}"
            };
            api.body += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }
        /// <summary>
        ///点赞
        /// </summary>
        /// <param name="dislike"> 当前dislike状态</param>
        /// <param name="like">当前like状态</param>
        /// <returns></returns>
        public ApiModel Dislike(string aid, int dislike, int like)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = $"https://app.biliapi.net/x/v2/view/dislike",
                body = ApiHelper.MustParameter(AppKey, true) + $"&aid={aid}&dislike={dislike}&from=7&like={like}"
            };
            api.body += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }

        /// <summary>
        ///一键三连
        /// </summary>
        /// <returns></returns>
        public ApiModel Triple(string aid)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = $"https://app.bilibili.com/x/v2/view/like/triple",
                body = ApiHelper.MustParameter(AppKey, true) + $"&aid={aid}"
            };
            api.body += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }

        public ApiModel Coin(string aid, int num)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = $"https://app.biliapi.net/x/v2/view/coin/add",
                body = ApiHelper.MustParameter(AppKey, true) + $"&aid={aid}&multiply={num}&platform=android&select_like=0"
            };
            api.body += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }

        /// <summary>
        /// 关注
        /// </summary>
        /// <param name="mid">用户ID</param>
        /// <param name="mode">1为关注，2为取消关注</param>
        /// <returns></returns>
        public ApiModel Attention(string mid, string mode)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/relation/modify",
                body = ApiHelper.MustParameter(AppKey, true) + $"&act={mode}&fid={mid}&re_src=32"
            };
            api.body += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }

        public ApiModel Tags(string aid)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/tag/archive/tags",
                parameter = $"&aid={aid}",
                need_cookie = true,
            };
            return api;
        }

        public ApiModel GetMediaList(string medisListId, string lastAid, int pagesize = 20)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.bilibili.com/x/v2/medialist/resource/list",
                parameter =
                    $"{ApiHelper.MustParameter(AppKey, true)}&type=1&biz_id={medisListId}&oid={lastAid}&otype=2&ps={pagesize}&direction=false&desc=true&sort_field=1&tid=0&with_current=false",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }
    }
}
