using BiliLite.Services;
using System;
using System.Collections.Generic;

namespace BiliLite.Models.Requests.Api.User
{
    /// <summary>
    /// 收藏夹相关API
    /// </summary>
    public class FavoriteApi : BaseApi
    {
        /// <summary>
        /// 我的收藏夹/收藏的收藏夹
        /// </summary>
        /// <returns></returns>
        public ApiModel MyFavorite()
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v3/fav/folder/space/v2",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&up_mid={SettingService.Account.UserID}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }
        public ApiModel MyCreatedFavoriteList(int page)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v3/fav/folder/created/list",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&up_mid={SettingService.Account.UserID}&pn={page}&ps=20"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }
        /// <summary>
        /// 我创建的收藏夹
        /// </summary>
        /// <returns></returns>
        public ApiModel MyCreatedFavorite(string aid)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/medialist/gateway/base/created",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&rid={aid}&up_mid={SettingService.Account.UserID}&type=2&pn=1&ps=100"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        /// <summary>
        /// 添加到收藏夹
        /// </summary>
        /// <returns></returns>
        public ApiModel AddFavorite(List<string> favIds, string avid)
        {
            var ids = "";
            foreach (var item in favIds)
            {
                ids += item + ",";
            }
            ids = Uri.EscapeDataString(ids.TrimEnd(','));
            var api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/medialist/gateway/coll/resource/deal",
                body = ApiHelper.MustParameter(AppKey, true) + $"&add_media_ids={ids}&rid={avid}&type=2"
            };
            api.body += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }

        /// <summary>
        /// 收藏收藏夹
        /// </summary>
        /// <returns></returns>
        public ApiModel CollectFavorite(string media_id)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v3/fav/folder/fav",
                body = ApiHelper.MustParameter(AppKey, true) + $"&media_id={media_id}"
            };
            api.body += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }
        /// <summary>
        /// 取消收藏收藏夹
        /// </summary>
        /// <returns></returns>
        public ApiModel CacnelCollectFavorite(string media_id)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v3/fav/folder/unfav",
                body = ApiHelper.MustParameter(AppKey, true) + $"&media_id={media_id}"
            };
            api.body += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }

        /// <summary>
        /// 创建收藏夹
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="intro">介绍</param>
        /// <param name="isOpen">是否私密</param>
        /// <returns></returns>
        public ApiModel CreateFavorite(string title, string intro, bool isOpen)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v3/fav/folder/add",
                body = ApiHelper.MustParameter(AppKey, true) + $"&privacy={(isOpen ? 0 : 1)}&title={Uri.EscapeDataString(title)}&intro={Uri.EscapeDataString(intro)}"
            };
            api.body += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }
        /// <summary>
        /// 编辑收藏夹
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="intro">介绍</param>
        /// <param name="isOpen">是否私密</param>
        /// <param name="media_id">收藏夹ID</param>
        /// <returns></returns>
        public ApiModel EditFavorite(string title, string intro, bool isOpen, string media_id)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v3/fav/folder/edit",
                body = ApiHelper.MustParameter(AppKey, true) + $"&privacy={(isOpen ? 0 : 1)}&title={Uri.EscapeDataString(title)}&intro={Uri.EscapeDataString(intro)}&media_id={media_id}"
            };
            api.body += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }

        /// <summary>
        /// 删除收藏夹
        /// </summary>
        /// <param name="media_id">收藏夹ID</param>
        /// <returns></returns>
        public ApiModel DelFavorite(string media_id)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v3/fav/folder/del",
                body = ApiHelper.MustParameter(AppKey, true) + $"&media_ids={media_id}"
            };
            api.body += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }

        /// <summary>
        /// 收藏夹信息
        /// </summary>
        /// <returns></returns>
        public ApiModel FavoriteInfo(string fid, string keyword, int page = 1)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v3/fav/resource/list",
                parameter = $"media_id={fid}&mid={SettingService.Account.UserID}&keyword={Uri.EscapeDataString(keyword)}&pn={page}&ps=20&platform=web",
                need_cookie = true,
            };
            return api;
        }

        public ApiModel FavoriteSeasonInfo(string season_id, string keyword, int page = 1)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/space/fav/season/list",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&season_id={season_id}&mid={SettingService.Account.UserID}&keyword={Uri.EscapeDataString(keyword)}&pn={page}&ps=20"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        /// <summary>
        /// 取消收藏
        /// </summary>
        /// <returns></returns>
        public ApiModel Delete(string media_id, List<string> video_ids)
        {
            var ids = "";
            foreach (var item in video_ids)
            {
                ids += $"{item}:2,";
            }
            ids = Uri.EscapeDataString(ids.TrimEnd(','));
            var api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v3/fav/resource/batch-del",
                body = ApiHelper.MustParameter(AppKey, true) + $"&media_id={media_id}&resources={ids}"
            };
            api.body += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }
        /// <summary>
        /// 复制到自己的收藏夹
        /// </summary>
        /// <returns></returns>
        public ApiModel Copy(string src_media_id, string tar_media_id, List<string> video_ids, string mid)
        {
            var ids = "";
            foreach (var item in video_ids)
            {
                ids += $"{item}:2,";
            }
            ids = Uri.EscapeDataString(ids.TrimEnd(','));
            var api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v3/fav/resource/copy",
                body = ApiHelper.MustParameter(AppKey, true) + $"&src_media_id={src_media_id}&tar_media_id={tar_media_id}&resources={ids}&mid={mid}"
            };
            api.body += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }
        /// <summary>
        /// 移动到收藏夹
        /// </summary>
        /// <returns></returns>
        public ApiModel Move(string src_media_id, string tar_media_id, List<string> video_ids)
        {
            var ids = "";
            foreach (var item in video_ids)
            {
                ids += $"{item}:2,";
            }
            ids = Uri.EscapeDataString(ids.TrimEnd(','));
            var api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v3/fav/resource/move",
                body = ApiHelper.MustParameter(AppKey, true) + $"&src_media_id={src_media_id}&tar_media_id={tar_media_id}&resources={ids}"
            };
            api.body += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }
        /// <summary>
        /// 清除失效
        /// </summary>
        /// <returns></returns>
        public ApiModel Clean(string media_id)
        {
            var api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v3/fav/resource/clean",
                body = ApiHelper.MustParameter(AppKey, true) + $"&media_id={media_id}"
            };
            api.body += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }

        public ApiModel Sort(List<string> favIdList)
        {
            var sort = string.Join(',', favIdList);
            var api = new ApiModel()
            {
                method = RestSharp.Method.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v3/fav/folder/sort",
                body = ApiHelper.MustParameter(AppKey, true) + $"&sort={sort}"
            };
            api.body += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }
    }
}
