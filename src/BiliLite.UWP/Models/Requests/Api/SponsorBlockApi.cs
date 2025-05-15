using BiliLite.Models.Common;
using BiliLite.ViewModels.Video;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Windows.ApplicationModel;

namespace BiliLite.Models.Requests.Api
{
    public class SponsorBlockApi
    {
        /// <summary>
        /// 获取视频的SponsorBlock数据。
        /// <para>使用sha256HashPrefix作为视频的唯一标识符，保证隐私。</para>
        /// </summary>
        /// <param name="bvid">视频的BVID</param>
        public ApiModel GetSponsorBlock(string bvid)
        {
            // 计算bvid的sha256并取前4位, 与浏览器插件一致
            string sha256HashPrefix;
            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(bvid));
                sha256HashPrefix = BitConverter.ToString(hash).Replace("-", "").ToLower().Substring(0, 4);
            }

            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"http://115.190.32.254:8080/api/skipSegments/{sha256HashPrefix}"
            };
            return api;
        }

        /// <summary>
        /// 提交SponsorBlock数据。
        /// </summary>
        /// <param name="bvid">视频的BVID</param>
        /// <param name="cvid">视频的CID</param>
        /// <param name="userId">用户ID</param>
        /// <param name="videoDuration">视频时长（秒）</param>
        /// <param name="segments">需要跳过的片段列表</param>
        public ApiModel PostSponsorBlock(string bvid, string cvid, string userId,
                                         float videoDuration, List<SponsorBlockSegment> segments)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = "http://115.190.32.254:8080/api/skipSegments",
                body = JsonConvert.SerializeObject(new 
                {
                    videoID = bvid,
                    cid = cvid,
                    userID = userId,
                    userAgent = $"BiliUWP-Lite/{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}",
                    videoDuration = videoDuration,
                    segments = segments
                })
            };
            return api;
        }
    }
}
