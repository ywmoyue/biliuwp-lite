using BiliLite.Models.Common.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BiliLite.Player.RangeDownload
{
    /// <summary>
    /// 多线程下载的视频节点与换节点规则
    /// 节点列表与规则参考 Bilibili 线程撕裂者（MIT）：https://github.com/MrTangLuyao/Bilibili-thread-ripper
    /// </summary>
    public static class BiliVideoCdn
    {
        public static readonly IReadOnlyList<string> MainlandHosts = new[]
        {
            "upos-sz-mirrorali.bilivideo.com",
            "upos-sz-mirrorhw.bilivideo.com",
            "upos-sz-mirrorbos.bilivideo.com",
            "upos-sz-mirror08c.bilivideo.com",
            "upos-sz-mirrorbd.bilivideo.com",
            "upos-sz-mirror14b.bilivideo.com",
            "upos-sz-estgoss.bilivideo.com",
            "upos-sz-mirrorcos.bilivideo.com",
        };

        public static readonly IReadOnlyList<string> OverseasHosts = new[]
        {
            "upos-sz-mirrorcosov.bilivideo.com",
            "upos-sz-mirroraliov.bilivideo.com",
            "cn-hk-eq-01-01.bilivideo.com",
            "cn-hk-eq-01-03.bilivideo.com",
        };

        private static readonly Regex MediaHostRegex = new Regex(
            @"(?:^|\.)(?:bilivideo\.(?:com|cn|net)|akamaized\.net|szbdyd\.com|hdslb\.com|xycdn\.com|mountaintoys\.cn|nexusedgeio\.com|ahdohpiechei\.com)$",
            RegexOptions.IgnoreCase);

        private static readonly Regex HostRegex = new Regex(@"^https://[^/]+", RegexOptions.IgnoreCase);

        /// <summary>
        /// 是否B站的音视频地址。签名过的下载地址只能发给B站自己的服务器
        /// </summary>
        public static bool IsBiliMediaUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) return false;
            if (uri.Scheme != "https") return false;
            var path = uri.AbsolutePath;
            if (!path.EndsWith(".m4s", StringComparison.OrdinalIgnoreCase) &&
                !path.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) &&
                !path.EndsWith(".flv", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return MediaHostRegex.IsMatch(uri.Host);
        }

        /// <summary>
        /// 生成一个音视频文件的全部下载节点
        /// </summary>
        /// <param name="url">当前使用的地址</param>
        /// <param name="backupUrls">接口给的备用地址</param>
        /// <param name="mode">节点模式</param>
        public static List<RangeSource> BuildSources(string url, IEnumerable<string> backupUrls, MultiThreadCdnMode mode)
        {
            var originals = new[] { url }
                .Concat(backupUrls ?? Enumerable.Empty<string>())
                .Where(IsBiliMediaUrl)
                .Distinct()
                .ToList();
            // 接口给的地址都参与下载：热门视频海外节点有缓存时它们最快，下载时按实测速度分配
            var sources = originals.Select(x => new RangeSource(x)).ToList();
            if (mode == MultiThreadCdnMode.Original) return sources;

            // 同一个签名地址可以发给任意节点，只换域名
            // 部分海外账号只拿到 akamaized.net 地址，这时节点也认它的签名，才用它来换
            var hosts = mode == MultiThreadCdnMode.Overseas ? OverseasHosts : MainlandHosts;
            var donor = originals.FirstOrDefault(x => !IsAkamai(x));
            var donors = donor != null ? new List<string> { donor } : originals;
            foreach (var host in hosts)
            {
                foreach (var item in donors)
                {
                    var swapped = SwapHost(item, host);
                    if (swapped != null && sources.All(x => x.Url != swapped))
                    {
                        sources.Add(new RangeSource(swapped));
                    }
                }
            }

            return sources;
        }

        private static string HostOf(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri.Host.ToLowerInvariant() : "";
        }

        private static bool IsAkamai(string url)
        {
            return HostOf(url).EndsWith(".akamaized.net");
        }

        private static string SwapHost(string url, string host)
        {
            // 连端口一起换掉，PCDN 地址常带 :4483 之类的端口
            var swapped = HostRegex.Replace(url, "https://" + host, 1);
            return IsBiliMediaUrl(swapped) ? swapped : null;
        }
    }

    /// <summary>
    /// 一个下载节点（同一个文件在某个节点上的地址）及其测得的速度
    /// </summary>
    public class RangeSource
    {
        public RangeSource(string url)
        {
            Url = url;
        }

        public string Url { get; }

        /// <summary>
        /// 测得的速度（字节/秒），平滑过
        /// </summary>
        public double Speed { get; set; }

        /// <summary>
        /// 是否已经成功下载过
        /// </summary>
        public bool Tested { get; set; }

        /// <summary>
        /// 正在进行的请求数
        /// </summary>
        public int Active { get; set; }

        /// <summary>
        /// 连续失败次数
        /// </summary>
        public int Strikes { get; set; }

        /// <summary>
        /// 停用到这个时间
        /// </summary>
        public DateTime BannedUntil { get; set; } = DateTime.MinValue;
    }
}
