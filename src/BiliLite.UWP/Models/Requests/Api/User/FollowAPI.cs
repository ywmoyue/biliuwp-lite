﻿using BiliLite.Models.Common;
using BiliLite.Services;

namespace BiliLite.Models.Requests.Api.User
{
    public class FollowAPI : BaseApi
    {
        /// <summary>
        /// 我的追番
        /// </summary>
        /// <param name="page">页数</param>
        /// <param name="status">0=全部，1=想看，2=在看，3=看过</param>
        /// <param name="pagesize">每页数量</param>
        /// <returns></returns>
        public ApiModel MyFollowBangumi(int page = 1, int status = 0, int pagesize = 20)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/pgc/app/follow/v2/bangumi",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&pn={page}&ps={pagesize}&status={status}",
                headers = ApiHelper.GetAuroraHeaders()
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }
        /// <summary>
        /// 我的追剧
        /// </summary>
        /// <param name="page">页数</param>
        /// <param name="status">0=全部，1=想看，2=在看，3=看过</param>
        /// <param name="pagesize">每页数量</param>
        /// <returns></returns>
        public ApiModel MyFollowCinema(int page = 1, int status = 0, int pagesize = 20)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/pgc/app/follow/v2/cinema",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&pn={page}&ps={pagesize}&status={status}",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }
        /// <summary>
        /// 收藏番剧
        /// </summary>
        /// <returns></returns>
        public ApiModel FollowSeason(string season_id)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/pgc/app/follow/add",
                body = ApiHelper.MustParameter(AppKey, true) + $"&season_id={season_id}"
            };
            api.body += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }
        /// <summary>
        /// 取消收藏番剧
        /// </summary>
        /// <returns></returns>
        public ApiModel CancelFollowSeason(string season_id)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/pgc/app/follow/del",
                body = ApiHelper.MustParameter(AppKey, true) + $"&season_id={season_id}"
            };
            api.body += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }

        /// <summary>
        /// 设置状态
        /// </summary>
        /// <returns></returns>
        public ApiModel SetSeasonStatus(string season_id, int status)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/pgc/app/follow/status/update",
                body = ApiHelper.MustParameter(AppKey, true) + $"&season_id={season_id}&status={status}"
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

        /// <summary>
        /// 查询用户与自己关系_仅查关注
        /// </summary>
        /// <param name="mid"></param>
        /// <returns></returns>
        public ApiModel GetAttention(string mid)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/relation",
                parameter = $"fid={mid}",
                need_cookie = true
            };
            return api;
        }
    }
}
