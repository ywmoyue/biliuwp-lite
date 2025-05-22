using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common;
using BiliLite.Models.Requests;
using BiliLite.ViewModels.Video;
using Microsoft.Toolkit.Uwp.Helpers;
using System.Text;
using Newtonsoft.Json;
using BiliLite.Services;

namespace BiliLite.Modules.SpBlock.Api
{
    [RegisterTransientService]
    public class SponsorBlockApi
    {
        public string BaseUrl { get; set; }

        /// <summary>
        /// 获取视频的SponsorBlock数据。
        /// <para>使用sha256HashPrefix作为视频的标识符，保证隐私。</para>
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
                baseUrl = $"{BaseUrl}/skipSegments/{sha256HashPrefix}"
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
                baseUrl = $"{BaseUrl}/api/skipSegments",
                body = JsonConvert.SerializeObject(new 
                {
                    videoID = bvid,
                    cid = cvid,
                    userID = userId,
                    userAgent = $"BiliUWP-Lite/{SystemInformation.ApplicationVersion.Major}.{SystemInformation.ApplicationVersion.Minor}.{SystemInformation.ApplicationVersion.Build}",
                    videoDuration = videoDuration,
                    segments = segments
                })
            };
            return api;
        }
    }
}
