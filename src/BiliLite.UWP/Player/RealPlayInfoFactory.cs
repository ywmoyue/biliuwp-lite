using System;
using System.Collections.Generic;
using System.Linq;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Player;
using BiliLite.Models.Common.Video;
using BiliLite.Models.Common.Video.PlayUrlInfos;
using BiliLite.Services;

namespace BiliLite.Player
{
    /// <summary>
    /// 将播放相关输入统一转换为 RealPlayInfo，
    /// 对齐“真实播放信息工厂”职责，收敛 Header/媒体类型/时长/引擎回退链等规则。
    /// </summary>
    public static class RealPlayInfoFactory
    {
        public static RealPlayInfo CreateFromPlayUrlInfo(
            BiliPlayUrlInfo playUrlInfo,
            PlayInfo playInfo = null,
            bool isLocal = false,
            bool? isAutoPlay = null,
            string userAgent = null,
            string referer = null,
            RealPlayerType? preferredPlayerType = null,
            IEnumerable<RealPlayerType> fallbackPlayerTypes = null)
        {
            if (playUrlInfo == null)
            {
                throw new ArgumentNullException(nameof(playUrlInfo));
            }

            userAgent ??= playUrlInfo.UserAgent;
            referer ??= playUrlInfo.Referer;

            var mediaType = ConvertMediaType(playUrlInfo.PlayUrlType);
            var singleIsFlv = ResolveSingleIsFlv(playUrlInfo, isLocal);
            var realPlayInfo = CreateBaseInfo(
                mediaType,
                userAgent,
                referer,
                isLocal,
                isAutoPlay ?? GetDefaultAutoPlay(),
                preferredPlayerType,
                fallbackPlayerTypes,
                singleIsFlv: singleIsFlv);

            switch (playUrlInfo.PlayUrlType)
            {
                case BiliPlayUrlType.DASH:
                    realPlayInfo.DashInfo = playUrlInfo.DashInfo;
                    realPlayInfo.TotalDuration = ResolveTotalDuration(playUrlInfo, playInfo, playUrlInfo.DashInfo?.Duration);
                    realPlayInfo.SegmentDurations = BuildSegmentDurationsForDash(realPlayInfo.TotalDuration);
                    break;
                case BiliPlayUrlType.SingleFLV:
                    realPlayInfo.SingleIsFlv = true;
                    realPlayInfo.SingleUrl = playUrlInfo.FlvInfo?.FirstOrDefault()?.Url;
                    realPlayInfo.TotalDuration = ResolveTotalDuration(playUrlInfo, playInfo);
                    realPlayInfo.SegmentDurations = BuildSegmentDurationsForSingle(realPlayInfo.TotalDuration);
                    break;
                case BiliPlayUrlType.MultiFLV:
                    realPlayInfo.MultiFlvUrls = playUrlInfo.FlvInfo ?? new List<BiliFlvPlayUrlInfo>();
                    realPlayInfo.TotalDuration = ResolveTotalDuration(playUrlInfo, playInfo);
                    realPlayInfo.SegmentDurations = BuildSegmentDurationsForMultiFlv(realPlayInfo.MultiFlvUrls, realPlayInfo.TotalDuration);
                    break;
                default:
                    throw new NotSupportedException($"不支持的播放链接类型: {playUrlInfo.PlayUrlType}");
            }

            return realPlayInfo;
        }

        public static RealPlayInfo CreateFromDashInfo(
            BiliDashPlayUrlInfo dashInfo,
            PlayInfo playInfo = null,
            bool isLocal = false,
            bool? isAutoPlay = null,
            string userAgent = null,
            string referer = null,
            RealPlayerType? preferredPlayerType = null,
            IEnumerable<RealPlayerType> fallbackPlayerTypes = null)
        {
            var realPlayInfo = CreateBaseInfo(
                PlayMediaType.Dash,
                userAgent,
                referer,
                isLocal,
                isAutoPlay ?? GetDefaultAutoPlay(),
                preferredPlayerType,
                fallbackPlayerTypes,
                singleIsFlv: false);

            realPlayInfo.DashInfo = dashInfo;
            realPlayInfo.TotalDuration = ResolveTotalDuration(null, playInfo, dashInfo?.Duration);
            realPlayInfo.SegmentDurations = BuildSegmentDurationsForDash(realPlayInfo.TotalDuration);
            return realPlayInfo;
        }

        public static RealPlayInfo CreateFromSingleUrl(
            string singleUrl,
            bool singleIsFlv,
            double totalDuration = 0,
            bool isLocal = false,
            bool? isAutoPlay = null,
            string userAgent = null,
            string referer = null,
            RealPlayerType? preferredPlayerType = null,
            IEnumerable<RealPlayerType> fallbackPlayerTypes = null)
        {
            var realPlayInfo = CreateBaseInfo(
                PlayMediaType.Single,
                userAgent,
                referer,
                isLocal,
                isAutoPlay ?? GetDefaultAutoPlay(),
                preferredPlayerType,
                fallbackPlayerTypes,
                singleIsFlv);

            realPlayInfo.SingleUrl = singleUrl;
            realPlayInfo.TotalDuration = Math.Max(0, totalDuration);
            realPlayInfo.SegmentDurations = BuildSegmentDurationsForSingle(realPlayInfo.TotalDuration);
            return realPlayInfo;
        }

        public static RealPlayInfo CreateFromMultiFlv(
            List<BiliFlvPlayUrlInfo> flvUrls,
            double totalDuration = 0,
            bool isLocal = false,
            bool? isAutoPlay = null,
            string userAgent = null,
            string referer = null,
            RealPlayerType? preferredPlayerType = null,
            IEnumerable<RealPlayerType> fallbackPlayerTypes = null)
        {
            var realPlayInfo = CreateBaseInfo(
                PlayMediaType.MultiFlv,
                userAgent,
                referer,
                isLocal,
                isAutoPlay ?? GetDefaultAutoPlay(),
                preferredPlayerType,
                fallbackPlayerTypes,
                singleIsFlv: false);

            realPlayInfo.MultiFlvUrls = flvUrls ?? new List<BiliFlvPlayUrlInfo>();
            realPlayInfo.TotalDuration = totalDuration > 0 ? totalDuration : realPlayInfo.MultiFlvUrls.Sum(x => x.Length / 1000d);
            realPlayInfo.SegmentDurations = BuildSegmentDurationsForMultiFlv(realPlayInfo.MultiFlvUrls, realPlayInfo.TotalDuration);
            return realPlayInfo;
        }

        public static RealPlayInfo CreateFromLocalPlayInfo(
            LocalPlayInfo localPlayInfo,
            PlayInfo playInfo = null,
            bool? isAutoPlay = null,
            string userAgent = null,
            string referer = null,
            RealPlayerType? preferredPlayerType = null,
            IEnumerable<RealPlayerType> fallbackPlayerTypes = null)
        {
            if (localPlayInfo?.Info == null)
            {
                throw new ArgumentNullException(nameof(localPlayInfo));
            }

            return CreateFromPlayUrlInfo(
                localPlayInfo.Info,
                playInfo,
                isLocal: true,
                isAutoPlay,
                userAgent,
                referer,
                preferredPlayerType,
                fallbackPlayerTypes);
        }

        private static RealPlayInfo CreateBaseInfo(
            PlayMediaType mediaType,
            string userAgent,
            string referer,
            bool isLocal,
            bool isAutoPlay,
            RealPlayerType? preferredPlayerType,
            IEnumerable<RealPlayerType> fallbackPlayerTypes,
            bool singleIsFlv)
        {
            var resolvedUserAgent = string.IsNullOrWhiteSpace(userAgent) ? Constants.CHROME_USER_AGENT : userAgent;
            var resolvedReferer = string.IsNullOrWhiteSpace(referer) ? "https://www.bilibili.com/" : referer;
            var preferred = ResolvePreferredPlayerType(mediaType, singleIsFlv, preferredPlayerType);
            var fallback = BuildFallbackPlayerTypes(mediaType, singleIsFlv, preferred, fallbackPlayerTypes);

            return new RealPlayInfo
            {
                PlayMediaType = mediaType,
                SingleIsFlv = singleIsFlv,
                IsLocal = isLocal,
                IsAutoPlay = isAutoPlay,
                UserAgent = resolvedUserAgent,
                Referer = resolvedReferer,
                Headers = BuildHeaders(resolvedUserAgent, resolvedReferer),
                PreferredPlayerType = preferred,
                FallbackPlayerTypes = fallback,
            };
        }

        private static PlayMediaType ConvertMediaType(BiliPlayUrlType playUrlType)
        {
            return playUrlType switch
            {
                BiliPlayUrlType.DASH => PlayMediaType.Dash,
                BiliPlayUrlType.SingleFLV => PlayMediaType.Single,
                BiliPlayUrlType.MultiFLV => PlayMediaType.MultiFlv,
                _ => PlayMediaType.Single,
            };
        }

        private static bool ResolveSingleIsFlv(BiliPlayUrlInfo playUrlInfo, bool isLocal)
        {
            if (playUrlInfo?.PlayUrlType != BiliPlayUrlType.SingleFLV)
            {
                return false;
            }

            var url = playUrlInfo.FlvInfo?.FirstOrDefault()?.Url;
            if (!string.IsNullOrWhiteSpace(url) && isLocal)
            {
                if (url.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase)
                    || url.EndsWith(".m4v", StringComparison.OrdinalIgnoreCase)
                    || url.EndsWith(".mov", StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        private static double ResolveTotalDuration(BiliPlayUrlInfo playUrlInfo, PlayInfo playInfo, double? dashDuration = null)
        {
            if (dashDuration.HasValue && dashDuration.Value > 0)
            {
                return dashDuration.Value;
            }

            if (playUrlInfo != null)
            {
                if (playUrlInfo.Timelength > 0)
                {
                    return playUrlInfo.Timelength / 1000d;
                }

                if (playUrlInfo.FlvInfo != null && playUrlInfo.FlvInfo.Count > 0)
                {
                    var sum = playUrlInfo.FlvInfo.Sum(x => x.Length / 1000d);
                    if (sum > 0)
                    {
                        return sum;
                    }
                }

                if (playUrlInfo.DashInfo?.Duration > 0)
                {
                    return playUrlInfo.DashInfo.Duration;
                }
            }

            if (playInfo?.duration > 0)
            {
                return playInfo.duration;
            }

            return 0;
        }

        private static List<double> BuildSegmentDurationsForDash(double totalDuration)
        {
            return totalDuration > 0 ? new List<double> { totalDuration } : new List<double>();
        }

        private static List<double> BuildSegmentDurationsForSingle(double totalDuration)
        {
            return totalDuration > 0 ? new List<double> { totalDuration } : new List<double>();
        }

        private static List<double> BuildSegmentDurationsForMultiFlv(List<BiliFlvPlayUrlInfo> urls, double totalDuration)
        {
            if (urls != null && urls.Count > 0)
            {
                var segments = urls
                    .Select(x => x.Length / 1000d)
                    .Where(x => x > 0)
                    .ToList();
                if (segments.Count > 0)
                {
                    return segments;
                }
            }

            return totalDuration > 0 ? new List<double> { totalDuration } : new List<double>();
        }

        private static Dictionary<string, string> BuildHeaders(string userAgent, string referer)
        {
            var headers = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(userAgent))
            {
                headers["User-Agent"] = userAgent;
            }

            if (!string.IsNullOrWhiteSpace(referer))
            {
                headers["Referer"] = referer;
            }

            return headers;
        }

        private static RealPlayerType ResolvePreferredPlayerType(PlayMediaType mediaType, bool singleIsFlv, RealPlayerType? preferredPlayerType)
        {
            var settingPreferred = (RealPlayerType)SettingService.GetValue(
                SettingConstants.Player.USE_REAL_PLAYER_TYPE,
                (int)SettingConstants.Player.DEFAULT_USE_REAL_PLAYER_TYPE);

            var candidate = preferredPlayerType ?? settingPreferred;

            if (mediaType == PlayMediaType.Dash)
            {
                return candidate switch
                {
                    RealPlayerType.Native => RealPlayerType.Native,
                    RealPlayerType.FFmpegInterop => RealPlayerType.FFmpegInterop,
                    RealPlayerType.ShakaPlayer => RealPlayerType.ShakaPlayer,
                    _ => RealPlayerType.Native,
                };
            }

            if (mediaType == PlayMediaType.Single && singleIsFlv)
            {
                // 单段 FLV 不走 Shaka，默认 FFmpeg，必要时降级到 SYEngine 路径（用 Native 槽位表示）
                return candidate == RealPlayerType.Native ? RealPlayerType.Native : RealPlayerType.FFmpegInterop;
            }

            if (mediaType == PlayMediaType.MultiFlv)
            {
                // 当前 MultiFlv 仅走 SYEngine 子播放器，沿用 FFmpegInterop 槽位。
                return RealPlayerType.FFmpegInterop;
            }

            // 单链接 mp4 等
            return candidate == RealPlayerType.FFmpegInterop
                ? RealPlayerType.FFmpegInterop
                : RealPlayerType.Native;
        }

        private static List<RealPlayerType> BuildFallbackPlayerTypes(
            PlayMediaType mediaType,
            bool singleIsFlv,
            RealPlayerType preferred,
            IEnumerable<RealPlayerType> fallbackPlayerTypes)
        {
            if (fallbackPlayerTypes != null)
            {
                var customChain = fallbackPlayerTypes
                    .Distinct()
                    .ToList();
                if (customChain.Count > 0)
                {
                    return customChain;
                }
            }

            List<RealPlayerType> defaultChain;
            if (mediaType == PlayMediaType.Dash)
            {
                defaultChain = new List<RealPlayerType>
                {
                    RealPlayerType.ShakaPlayer,
                    RealPlayerType.Native,
                    RealPlayerType.FFmpegInterop,
                };
            }
            else if (mediaType == PlayMediaType.Single && singleIsFlv)
            {
                defaultChain = new List<RealPlayerType>
                {
                    RealPlayerType.FFmpegInterop,
                    RealPlayerType.Native,
                };
            }
            else if (mediaType == PlayMediaType.MultiFlv)
            {
                defaultChain = new List<RealPlayerType>
                {
                    RealPlayerType.FFmpegInterop,
                };
            }
            else
            {
                defaultChain = new List<RealPlayerType>
                {
                    RealPlayerType.Native,
                    RealPlayerType.FFmpegInterop,
                };
            }

            // 将优先引擎放在队首，保持其余顺序。
            var orderedChain = new List<RealPlayerType> { preferred };
            orderedChain.AddRange(defaultChain.Where(x => x != preferred));
            return orderedChain.Distinct().ToList();
        }

        private static bool GetDefaultAutoPlay()
        {
            return SettingService.GetValue(SettingConstants.Player.AUTO_PLAY, false);
        }
    }
}
