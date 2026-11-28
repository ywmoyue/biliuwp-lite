using System.Collections.Generic;
using BiliLite.Models.Common;
using BiliLite.Services;

namespace BiliLite.Models.Requests.Api.Home
{
    /// <summary>
    /// 番剧/国创首页接口
    /// </summary>
    public class AnimeAPI
    {
        private const int BANGUMI_SEASON_TYPE = 1;

        /// <summary>
        /// 首页推荐流（题材榜 + 猜你喜欢），游标从 0 开始，每页返回 28 张推荐卡 + 1 个题材榜
        /// </summary>
        /// <param name="seasonType">1=番剧，4=国创</param>
        /// <param name="cursor">上一页返回的 coursor</param>
        public ApiModel Feed(int seasonType, int cursor)
        {
            return new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/pgc/page/web/v3/feed",
                parameter = $"name={GetPageName(seasonType)}&coursor={cursor}",
            };
        }

        /// <summary>
        /// 热门榜单（带追番数、弹幕数、播放数）
        /// </summary>
        /// <param name="seasonType">1=番剧，4=国创</param>
        public ApiModel Rank(int seasonType)
        {
            return new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/pgc/web/rank/list",
                parameter = $"day=3&season_type={seasonType}",
            };
        }

        /// <summary>
        /// 热门番剧（索引接口，按播放最多取）
        /// </summary>
        /// <param name="seasonType">1=番剧，4=国创</param>
        public ApiModel Hot(int seasonType)
        {
            return new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/pgc/season/index/result",
                parameter = "st=1&order=2&season_version=-1&area=-1&is_finish=-1&copyright=-1&season_status=-1" +
                            $"&season_month=-1&year=-1&style_id=-1&sort=0&page=1&season_type={seasonType}&pagesize=20&type=1",
            };
        }

        /// <summary>
        /// 番剧/国创时间表
        /// </summary>
        /// <param name="seasonType">1=番剧，4=国创</param>
        public ApiModel Timeline(int seasonType)
        {
            return new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://bangumi.bilibili.com/web_api/" +
                          (seasonType == BANGUMI_SEASON_TYPE ? "timeline_global" : "timeline_cn"),
            };
        }

        /// <summary>
        /// 番剧/国创首页 HTML：轮播图数据在页面的 window.__INITIAL_STATE__ 里，只能整页取
        /// </summary>
        /// <param name="seasonType">1=番剧，4=国创</param>
        public ApiModel HomePage(int seasonType)
        {
            return new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = seasonType == BANGUMI_SEASON_TYPE
                    ? "https://www.bilibili.com/anime"
                    : "https://www.bilibili.com/guochuang",
                headers = new Dictionary<string, string>()
                {
                    { "Referer", "https://www.bilibili.com/" },
                },
            };
        }

        private static string GetPageName(int seasonType)
        {
            return seasonType == BANGUMI_SEASON_TYPE ? "anime" : "guochuang";
        }
    }
}