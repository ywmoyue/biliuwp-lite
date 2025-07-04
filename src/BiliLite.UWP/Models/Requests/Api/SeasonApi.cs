﻿using BiliLite.Services;
using System;
using BiliLite.Models.Common;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLite.Models.Requests.Api
{
    public class SeasonApi : BaseApi
    {
        private readonly CookieService m_cookieService;

        public SeasonApi()
        {
            m_cookieService = App.ServiceProvider.GetRequiredService<CookieService>();
        }

        /// <summary>
        /// 用season_id / ep_id 取番剧信息
        /// </summary>
        /// <param name="id"> seasonId / epId</param>
        /// <param name="type">id类型</param>
        /// <param name="proxy"></param>
        /// <returns></returns>
        public ApiModel Detail(string id, SeasonIdType type = SeasonIdType.SeasonId, bool proxy = false)
        {
            var baseUrl = ApiHelper.API_BASE_URL;

            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"{baseUrl}/pgc/view/v2/app/season",
                parameter = ApiHelper.MustParameter(AppKey, true),
            };
            if (type == SeasonIdType.SeasonId)
            {
                api.parameter += $"&season_id={id}";
            }
            else
            {
                api.parameter += $"&ep_id={id}";
            }

            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        public ApiModel EpList(string id, SeasonIdType type = SeasonIdType.SeasonId)
        {
            var baseUrl = ApiHelper.API_BASE_URL;

            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"{baseUrl}/pgc/view/web/ep/list",
                need_cookie = true
            };

            if (type == SeasonIdType.SeasonId)
            {
                api.parameter += $"&season_id={id}";
            }
            else
            {
                api.parameter += $"&ep_id={id}";
            }

            return api;
        }

        /// <summary>
        /// 短评
        /// </summary>
        /// <param name="media_id"></param>
        /// <returns></returns>
        public ApiModel ShortReview(int media_id, string next = "", int sort = 0)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/pgc/review/short/list",
                parameter = $"media_id={media_id}&ps=20&sort={sort}&cursor={next}"
            };
            if (SettingService.Account.Logined)
            {
                api.parameter += $"&access_key={SettingService.Account.AccessKey}";
            }
            return api;
        }
        /// <summary>
        /// 点赞短评
        /// </summary>
        /// <param name="media_id"></param>
        /// <returns></returns>
        public ApiModel LikeReview(int media_id, int review_id, ReviewType review_type = ReviewType.Short)
        {
            var csrf = m_cookieService.GetCSRFToken();
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = $"https://api.bilibili.com/pgc/review/action/like",
                body = $"{ApiHelper.MustParameter(AppKey, true)}&media_id={media_id}&review_id={review_id}&review_type={(int)review_type}",
            };
            api.body += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }
        /// <summary>
        /// 反对短评
        /// </summary>
        /// <param name="media_id"></param>
        /// <returns></returns>
        public ApiModel DislikeReview(int media_id, int review_id, ReviewType review_type = ReviewType.Short)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = $"https://api.bilibili.com/pgc/review/action/dislike",
                body = $"{ApiHelper.MustParameter(AppKey, true)}&media_id={media_id}&review_id={review_id}&review_type={(int)review_type}"
            };
            api.body += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }
        /// <summary>
        /// 发表点评
        /// </summary>
        /// <param name="media_id">ID</param>
        /// <param name="content">内容</param>
        /// <param name="share_feed">是否分享动态</param>
        /// <param name="score">分数（10分制）</param>
        /// <returns></returns>
        public ApiModel SendShortReview(int media_id, string content, bool share_feed, int score)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = $"https://api.bilibili.com/pgc/review/action/short/post",
                body = $"{ApiHelper.MustParameter(AppKey, true)}&media_id={media_id}&content={Uri.EscapeDataString(content)}&share_feed={(share_feed ? 1 : 0)}&score={score}"
            };
            api.body += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }
    }

    public enum ReviewType
    {
        Long = 1,
        Short = 2,
    }
}
