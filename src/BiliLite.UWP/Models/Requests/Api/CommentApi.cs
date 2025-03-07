using System;
using System.Collections.Generic;
using System.Linq;
using BiliLite.Models.Common;
using BiliLite.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using BiliLite.Models.Common.UserDynamic;
using BiliLite.Models.Exceptions;

namespace BiliLite.Models.Requests.Api
{
    public class CommentApi
    {
        private readonly CookieService m_cookieService;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        public CommentApi()
        {
            m_cookieService = App.ServiceProvider.GetRequiredService<CookieService>();
        }

        public enum CommentType
        {
            Video = 1,
            Dynamic = 17,
            Article = 12,
            Photo = 11,
            MiniVideo = 5,
            SongMenu = 19,
            Song = 14
        }
        public enum CommentSort
        {
            New = 0,
            Hot = 1,
        }
        /// <summary>
        /// 读取评论
        /// </summary>
        /// <param name="oid">ID</param>
        /// <param name="sort">1=最新，2=最热</param>
        /// <param name="pn">页数</param>
        /// <param name="type">类型，1=视频，17=动态，11=图片，5=小视频，19=歌单，14=歌曲</param>
        /// <param name="ps">每页数量</param>
        /// <returns></returns>
        public ApiModel Comment(string oid, CommentSort sort, int pn, int type, int ps = 30)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/reply",
                parameter = $"oid={oid}&plat=2&pn={pn}&ps={ps}&sort={(int)sort}&type={type}",
                need_cookie = true,
            };
            //api.parameter += ApiHelper.GetSign(api.parameter, appKey);
            return api;
        }

        /// <summary>
        /// 读取评论V2接口
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="sort"></param>
        /// <param name="pn"></param>
        /// <param name="type"></param>
        /// <param name="ps"></param>
        /// <returns></returns>
        public ApiModel CommentV2(string oid, CommentSort sort, int pn, int type, int ps = 30, string offsetStr = null)
        {
            var mode = sort == CommentSort.Hot ? 3 : 2;
            var pagination = new { offset = offsetStr };
            var paginationStr = JsonConvert.SerializeObject(pagination);
            var parameters = $"oid={oid}&ps={ps}&mode={mode}&type={type}&pagination_str={paginationStr}";

            try
            {
                var csrf = m_cookieService.GetCSRFToken();
                parameters += $"&csrf={csrf}";
            }
            catch (UnLoginException)
            {
                _logger.Warn("获取评论未登录");
            }

            return new ApiModel
            {
                method = HttpMethods.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/reply/main",
                parameter = parameters,
                need_cookie = true,
            };
        }


        public ApiModel Reply(string oid, string root, int pn, int type, int ps = 30)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/reply/reply",
                parameter = $"oid={oid}&plat=2&pn={pn}&ps={ps}&root={root}&type={type}",
                need_cookie = true,
            };
            //api.parameter += ApiHelper.GetSign(api.parameter, appKey);
            return api;
        }

        public ApiModel Like(string oid, string root, int action, int type)
        {
            var csrf = m_cookieService.GetCSRFToken();
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/reply/action",
                body = $"&oid={oid}&rpid={root}&action={action}&type={type}&csrf={csrf}",
                need_cookie = true,
            };
            //api.body += ApiHelper.GetSign(api.body, appKey);
            return api;
        }

        public ApiModel ReplyComment(string oid, string root, string parent, string message, int type)
        {
            var csrf = m_cookieService.GetCSRFToken();
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/reply/add",
                body = $"&oid={oid}&root={root}&parent={parent}&type={type}&message={message}&csrf={csrf}",
                need_cookie = true,
            };
            // api.body += ApiHelper.GetSign(api.body, appKey);
            return api;
        }

        public ApiModel DeleteComment(string oid, string rpid, int type)
        {
            var csrf = m_cookieService.GetCSRFToken();
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/reply/del",
                body = $"&oid={oid}&rpid={rpid}&type={type}&csrf={csrf}",
                need_cookie = true,
            };
            // api.body += ApiHelper.GetSign(api.body, appKey);
            return api;
        }

        public ApiModel AddComment(string oid, CommentType type, string message, List<DynamicPicture> pictures = null)
        {
            var csrf = m_cookieService.GetCSRFToken();
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/reply/add",
                body = $"&oid={oid}&type={(int)type}&message={Uri.EscapeDataString(message)}&csrf={csrf}",
                need_cookie = true,
            };

            if (pictures != null && pictures.Any())
            {
                var pictureList = new List<object>();
                foreach (var picture in pictures)
                {
                    pictureList.Add(new
                    {
                        img_src = picture.ImageUrl,
                        img_width = picture.ImageWidth,
                        img_height = picture.ImageHeight,
                        img_size = picture.ImgSize
                    });
                }

                var picturesArgs = JsonConvert.SerializeObject(pictureList);
                api.body += $"&pictures={picturesArgs}";
            }

            //api.body += ApiHelper.GetSign(api.body, appKey);
            return api;
        }

        public ApiModel UploadDraw(UploadFileInfo file, string biz = "new_dyn")
        {
            var csrf = m_cookieService.GetCSRFToken();
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/dynamic/feed/draw/upload_bfs",
                FormData = new Dictionary<string, object>()
                {
                    { "file_up", file },
                    { "biz", biz },
                    { "csrf", csrf }
                },
                need_cookie = true,
            };

            if (biz == "new_dyn")
            {
                api.FormData.Add("category", "daily");
            }

            //api.body += ApiHelper.GetSign(api.body, appKey);
            return api;
        }
    }
}
