﻿using BiliLite.Services;
using System.Threading.Tasks;

namespace BiliLite.Models.Requests.Api.User
{
    public class UserDetailAPI : BaseApi
    {
        /// <summary>
        /// 用户信息
        /// </summary>
        /// <param name="mid"></param>
        /// <returns></returns>
        public async Task<ApiModel> UserInfo(string mid)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/space/wbi/acc/info",
                parameter = ApiHelper.MustParameter(AppKey, needAccesskey: true) + $"&mid={mid}",
            };
            api.parameter = await ApiHelper.GetWbiSign(api.parameter);
            return api;
        }
        /// <summary>
        /// 个人空间
        /// </summary>
        /// <param name="mid"></param>
        /// <returns></returns>
        public ApiModel Space(string mid)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = "https://app.bilibili.com/x/v2/space",
                parameter = ApiHelper.MustParameter(AppKey, needAccesskey: true) + $"&vmid={mid}",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }
        /// <summary>
        /// 数据
        /// </summary>
        /// <param name="mid"></param>
        /// <returns></returns>
        public ApiModel UserStat(string mid)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/relation/stat",
                parameter = $"vmid={mid}",
            };

            return api;
        }

        /// <summary>
        /// 用户视频投稿
        /// </summary>
        /// <param name="mid">用户ID</param>
        /// <param name="page">页数</param>
        /// <param name="pagesize">每页数量</param>
        /// <returns></returns>
        public async Task<ApiModel> SubmitVideos(string mid, int page = 1, int pagesize = 30, string keyword = "", int tid = 0, SubmitVideoOrder order = SubmitVideoOrder.pubdate)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/space/wbi/arc/search",
                parameter = $"mid={mid}&ps={pagesize}&tid={tid}&pn={page}&keyword={keyword}&order={order.ToString()}",
                need_cookie = true,
            };
            api.parameter = await ApiHelper.GetWbiSign(api.parameter);
            return api;
        }

        public ApiModel SubmitVideosCursor(string mid, int pagesize = 30, SubmitVideoOrder order = SubmitVideoOrder.pubdate,string cursor=null)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://app.bilibili.com/x/v2/space/archive/cursor",
                parameter =
                    $"{ApiHelper.MustParameter(AppKey, true)}&vmid={mid}&ps={pagesize}&order={order.ToString()}&aid={cursor}",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        /// <summary>
        /// 用户专栏投稿
        /// </summary>
        /// <param name="mid">用户ID</param>
        /// <param name="page">页数</param>
        /// <param name="pagesize">每页数量</param>
        /// <returns></returns>
        public ApiModel SubmitArticles(string mid, int page = 1, int pagesize = 30, SubmitArticleOrder order = SubmitArticleOrder.publish_time)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/space/article",
                parameter = $"mid={mid}&ps={pagesize}&pn={page}&sort={order.ToString()}",
            };
            return api;
        }

        public ApiModel SubmitCollections(string mid, int pageNum)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"https://app.bilibili.com/x/polymer/space/seasons_series_list_mobile",
                parameter = $"{ApiHelper.MustParameter(AppKey, true)}&mid={mid}&page_num={pageNum}&page_size=20",
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        /// <summary>
        /// 关注的分组
        /// </summary>
        /// <returns></returns>
        public ApiModel FollowingsTag()
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/relation/tags",
                parameter = ApiHelper.MustParameter(AppKey, true)
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }
        /// <summary>
        /// 关注的人
        /// </summary>
        /// <param name="mid">用户ID</param>
        /// <param name="page">页数</param>
        /// <param name="pagesize">每页数量</param>
        /// <returns></returns>
        public async Task<ApiModel> Followings(string mid, int page = 1, int pagesize = 30, int tid = 0, string keyword = "", FollowingsOrder order = FollowingsOrder.attention)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/relation/followings",
                parameter = $"vmid={mid}&ps={pagesize}&pn={page}&order=desc&order_type={(order == FollowingsOrder.attention ? "attention" : "")}",
                need_cookie = true,
            };
            if (tid == -1 && keyword != "")
            {
                api.baseUrl = $"{ApiHelper.API_BASE_URL}/x/relation/followings/search";
                api.parameter += $"&name={keyword}";
            }
            if (tid != -1)
            {
                api.baseUrl = $"{ApiHelper.API_BASE_URL}/x/relation/tag";
                api.parameter += $"&tagid={tid}&mid={mid}";
            }
            api.parameter = await ApiHelper.GetWbiSign(api.parameter);
            return api;
        }

        /// <summary>
        /// 粉丝
        /// </summary>
        /// <param name="mid">用户ID</param>
        /// <param name="page">页数</param>
        /// <param name="pagesize">每页数量</param>
        /// <returns></returns>
        public ApiModel Followers(string mid, int page = 1, int pagesize = 30)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/relation/followers",
                parameter = $"vmid={mid}&ps={pagesize}&pn={page}&order=desc",
                need_cookie = true,
            };
            return api;
        }

        /// <summary>
        /// 用户收藏夹
        /// </summary>
        /// <param name="mid">用户ID</param>
        /// <returns></returns>
        public ApiModel Favlist(string mid)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v3/fav/folder/created/list-all",
                parameter = $"up_mid={mid}",
            };
            return api;
        }
    }

    public enum SubmitVideoOrder
    {
        pubdate,
        click,
        stow
    }

    public enum SubmitArticleOrder
    {
        publish_time,
        view,
        fav
    }

    public enum FollowingsOrder
    {
        /// <summary>
        /// 关注时间
        /// </summary>
        addtime,
        /// <summary>
        /// 最常访问
        /// </summary>
        attention,
    }
}
