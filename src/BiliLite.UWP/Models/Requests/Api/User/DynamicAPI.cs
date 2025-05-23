﻿using BiliLite.Services;
using System;
using BiliLite.Models.Common;
using BiliLite.Models.Common.UserDynamic;

namespace BiliLite.Models.Requests.Api.User
{
    public class DynamicAPI : BaseApi
    {
        /// <summary>
        /// 读取动态列表
        /// </summary>
        /// <param name="type">268435455=全部,512,4097,4098,4099,4100,4101=番剧，8=视频，64=专栏</param>
        /// <returns></returns>
        public ApiModel DyanmicNew(UserDynamicType type)
        {
            var typeList = "";
            switch (type)
            {

                case UserDynamicType.Season:
                    typeList = "512,4097,4098,4099,4100,4101";
                    break;
                case UserDynamicType.Video:
                    typeList = "8";
                    break;
                case UserDynamicType.Article:
                    typeList = "64";
                    break;
                default:
                    typeList = "268435455";
                    //typeList = "268435455";
                    break;
            }
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/dynamic_new",
                parameter = $"type_list={Uri.EscapeDataString(typeList)}&uid={SettingService.Account.UserID}",
            };
            //var api = new ApiModel()
            //{
            //    method = HttpMethods.Get,
            //    baseUrl = "https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/space_history",
            //    parameter = $"host_uid={SettingService.Account.UserID}&visitor_uid={SettingService.Account.UserID}",
            //};
            //使用Web的API
            if (SettingService.Account.Logined)
            {
                api.parameter += "&";
                api.parameter += ApiHelper.MustParameter(AppKey, true);
            }
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        public ApiModel DynamicDetail(string id)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/get_dynamic_detail",
                parameter = $"dynamic_id={id}",
            };
            //使用Web的API
            if (SettingService.Account.Logined)
            {
                api.parameter += "&";
                api.parameter += ApiHelper.MustParameter(AppKey, true);
                api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            }

            return api;
        }
        public ApiModel DynamicRepost(string id, string offset = "")
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://api.vc.bilibili.com/dynamic_repost/v1/dynamic_repost/repost_detail",
                parameter = $"dynamic_id={id}&offset={offset}",
                need_cookie = true,
            };

            return api;
        }
        public ApiModel HistoryDynamic(string dynamic_id, UserDynamicType type)
        {
            var typeList = "";
            switch (type)
            {

                case UserDynamicType.Season:
                    typeList = "512,4097,4098,4099,4100,4101";
                    break;
                case UserDynamicType.Video:
                    typeList = "8";
                    break;
                case UserDynamicType.Article:
                    typeList = "64";
                    break;
                default:
                    typeList = "268435455";
                    //typeList = "268435455";
                    break;
            }

            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/dynamic_history",
                parameter = $"offset_dynamic_id={dynamic_id}&type_list={Uri.EscapeDataString(typeList)}&uid={SettingService.Account.UserID}"
            };//使用Web的API
            if (SettingService.Account.Logined)
            {
                api.parameter += "&";
                api.parameter += ApiHelper.MustParameter(AppKey, true);
            }
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        public ApiModel SpaceHistory(string mid, string dynamic_id = "")
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/space_history",
                parameter = $"offset_dynamic_id={dynamic_id}&visitor_uid={SettingService.Account.UserID}&host_uid={mid}&need_top=1"
            };
            if (SettingService.Account.Logined)
            {
                api.parameter += ApiHelper.MustParameter(AppKey, true);
                api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            }

            return api;
        }

        public ApiModel SpaceHistoryV2(string mid, string offset = "")
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.bilibili.com/x/polymer/web-dynamic/v1/feed/space",
                parameter = $"offset={offset}&host_mid={mid}",
                need_cookie = true,
            };
            //if (SettingService.Account.Logined)
            //{
            //    api.parameter += ApiHelper.MustParameter(AppKey, true);
            //    api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            //}

            return api;
        }

        public ApiModel Article(string updateBaseline,string type="article")
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.bilibili.com/x/polymer/web-dynamic/v1/feed/nav",
                parameter = $"type={type}&update_baseline={updateBaseline}",
                need_cookie = true,
            };

            return api;
        }

        /// <summary>
        /// 推荐话题
        /// </summary>
        /// <returns></returns>
        public ApiModel RecommendTopic()
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://api.vc.bilibili.com/topic_svr/v1/topic_svr/get_rcmd_topic",
                parameter = ApiHelper.MustParameter(AppKey, true),
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }


        /// <summary>
        /// 发表图片动态
        /// </summary>
        /// <returns></returns>
        public ApiModel CreateDynamicPhoto(string imgs, string content, string at_uids, string at_control)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = $"https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/create_draw",
                parameter = ApiHelper.MustParameter(AppKey, true),
                body = $"uid={SettingService.Account.UserID}&category=3&pictures={Uri.EscapeDataString(imgs)}&description={Uri.EscapeDataString(content)}&content={Uri.EscapeDataString(content)}&setting=%7B%22copy_forbidden%22%3A0%7D&at_uids={Uri.EscapeDataString(at_uids)}&at_control={Uri.EscapeDataString(at_control)}&extension=%7B%22emoji_type%22%3A1%7D"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        /// <summary>
        /// 发表文本动态
        /// </summary>
        /// <returns></returns>
        public ApiModel CreateDynamicText(string content, string at_uids, string at_control)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = $"https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/create",
                parameter = ApiHelper.MustParameter(AppKey, true),
                body = $"uid={SettingService.Account.UserID}&dynamic_id=0&type=4&content={Uri.EscapeDataString(content)}&setting=%7B%22copy_forbidden%22%3A0%7D&at_uids={Uri.EscapeDataString(at_uids)}&at_control={Uri.EscapeDataString(at_control)}&jumpfrom=110&extension=%7B%22emoji_type%22%3A1%7D"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        /// <summary>
        /// 转发动态
        /// </summary>
        /// <returns></returns>
        public ApiModel RepostDynamic(string dynamic_id, string content, string at_uids, string at_control)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = $"https://api.vc.bilibili.com/dynamic_repost/v1/dynamic_repost/repost",
                parameter = ApiHelper.MustParameter(AppKey, true),
                body = $"uid={SettingService.Account.UserID}&dynamic_id={dynamic_id}&content={Uri.EscapeDataString(content)}&at_uids={Uri.EscapeDataString(at_uids)}&at_control={Uri.EscapeDataString(at_control)}&extension=%7B%22emoji_type%22%3A1%7D"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        /// <summary>
        /// 点赞动态
        /// </summary>
        /// <param name="dynamic_id">动态ID</param>
        /// <param name="up">点赞=1，取消点赞=2</param>
        /// <returns></returns>
        public ApiModel Like(string dynamic_id, int up)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = $"https://api.vc.bilibili.com/dynamic_like/v1/dynamic_like/thumb",
                body = ApiHelper.MustParameter(AppKey, true) + $"&dynamic_id={dynamic_id}&uid={SettingService.Account.UserID}&up={up}"
            };
            api.body += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }
        /// <summary>
        /// 删除动态
        /// </summary>
        /// <param name="dynamic_id">动态ID</param>
        /// <returns></returns>
        public ApiModel Delete(string dynamic_id)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = $"https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/rm_dynamic",
                body = ApiHelper.MustParameter(AppKey, true) + $"&dynamic_id={dynamic_id}"
            };
            api.body += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="dynamic_id">动态ID</param>
        /// <returns></returns>
        public ApiModel UploadImage()
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://api.vc.bilibili.com/api/v1/drawImage/upload",
                parameter = ApiHelper.MustParameter(AppKey, true)
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }
    }
}
