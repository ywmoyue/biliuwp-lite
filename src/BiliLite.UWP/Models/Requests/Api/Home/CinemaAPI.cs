using System.Collections.Generic;
using BiliLite.Models.Common;
using BiliLite.Services;

namespace BiliLite.Models.Requests.Api.Home
{
    /// <summary>
    /// 影视首页接口
    /// </summary>
    public class CinemaAPI
    {
        /// <summary>
        /// 首页 HTML：轮播图数据在页面的 ul.carou-images-wrapper 里
        /// </summary>
        public ApiModel HomePage()
        {
            return new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://www.bilibili.com/cinema",
                headers = new Dictionary<string, string>()
                {
                    { "Referer", "https://www.bilibili.com/" },
                },
            };
        }

        /// <summary>
        /// 即将上线
        /// </summary>
        public ApiModel Online()
        {
            return new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/pgc/web/timeline/online",
                parameter = "type=1",
            };
        }

        /// <summary>
        /// 分区热播
        /// </summary>
        /// <param name="oid">2=电影，3=纪录片，5=电视剧，7=综艺</param>
        public ApiModel SeasonRank(int oid)
        {
            return new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/pgc/season/rank/web/list",
                parameter = $"day=3&season_type={oid}",
            };
        }
    }
}