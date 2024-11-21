﻿using System.Threading.Tasks;
using BiliLite.Models.Common;
using BiliLite.Services;

namespace BiliLite.Models.Requests.Api
{
    public class RankAPI
    {
        //public ApiModel RankRegion()
        //{
        //    var api = new ApiModel()
        //    {
        //        method = HttpMethods.Get,
        //        baseUrl = $"{ApiHelper.baseUrl}/api/rank/RankRegion"
        //    };
        //    return api;
        //}
        /// <summary>
        /// 排行榜
        /// </summary>
        /// <param name="rid">分区ID</param>
        /// <param name="type">all=全站，origin=原创，rookie=新人</param>
        /// <returns></returns>
        public async Task<ApiModel> Rank(int rid, string type)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/web-interface/ranking/v2",
                parameter = $"rid={rid}&type={type}",
            };
            if (SettingService.Account.Logined)
            {
                api.need_cookie = true;
            }
            api.parameter = await ApiHelper.GetWbiSign(api.parameter);
            return api;
        }

        /// <summary>
        /// 排行榜
        /// </summary>
        /// <param name="type">1=全站，2原创</param>
        /// <returns></returns>
        public ApiModel SeasonRank(int type)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/pgc/season/rank/list",
                parameter = $"season_type={type}"
            };
            return api;
        }
    }
}
