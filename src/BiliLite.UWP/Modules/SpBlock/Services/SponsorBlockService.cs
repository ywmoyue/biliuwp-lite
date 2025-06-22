using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common.Player;
using BiliLite.Models.Common;
using BiliLite.Modules.ExtraInterface;
using BiliLite.Modules.SpBlock.Api;
using BiliLite.Modules.SpBlock.Models;
using BiliLite.Services;

namespace BiliLite.Modules.SpBlock.Services
{

    [RegisterSingletonService(typeof(ISponsorBlockService))]
    public class SponsorBlockService : ISponsorBlockService
    {
        private readonly SponsorBlockApi m_sponsorBlockApi;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        public SponsorBlockService(SponsorBlockApi sponsorBlockApi)
        {
            m_sponsorBlockApi = sponsorBlockApi;
            m_sponsorBlockApi.BaseUrl =
                    SettingService.GetValue(
                        SettingConstants.Player.SPONSOR_BLOCK_API, DefaultApiUrl);
        }

        public Dictionary<string, List<PlayerSkipItem>> SponsorBlocks { get; set; } = [];

        // SponsorBlock API, 由B站空降助手提供
        public string DefaultApiUrl { get; } = "https://bsbsb.top/api";

        public async Task LoadSponsorBlock(string bvid)
        {
            if (SponsorBlocks.ContainsKey(bvid))
            {
                SponsorBlocks.Remove(bvid);
            }

            var sponsorBlocks = new List<PlayerSkipItem>();

            var results = await m_sponsorBlockApi.GetSponsorBlock(bvid).Request();
            if (!results.status)
            {
                if (results.code is 400 or 404)
                    NotificationShowExtensions.ShowMessageToast($"视频{bvid} SponsorBlock API请求错误: {results.code}");
                _logger.Warn(results.message);
                return;
            }

            var data = await results.GetJson<List<SponsorBlockVideo>>();
            if (data is null)
            {
                _logger.Warn("SponsorBlock转换错误");
                return;
            }

            var video = data.FirstOrDefault(video => video.VideoId == bvid);
            if (video is null) return;
            foreach (var seg in video.Segments)
            {
                var item = new PlayerSkipItem
                {
                    Start = seg.Segment[0] != 0 ? seg.Segment[0] : seg.Segment[0] + 0.75, //完全贴合视频开头的片段会报错. 加偏移量
                    End = Math.Abs(seg.Segment[1] - seg.VideoDuration) > 0.5
                        ? seg.Segment[1]
                        : seg.Segment[1] - 1.5, //完全贴合视频结尾的片段会卡死播放器，加偏移量
                    Category = seg.Category,
                    VideoDuration = seg.VideoDuration,
                    Cid = seg.Cid,
                };
                if (!item.IsSectionValid ||
                    item.CategoryEnum == SponsorBlockType.PoiHighlight ||
                    item.CategoryEnum == SponsorBlockType.None) continue; //暂不支持精彩时刻
                sponsorBlocks.Add(item);
            }

            SponsorBlocks.TryAdd(bvid, sponsorBlocks);
        }

        public List<PlayerSkipItem> GetVideoSponsorBlocks(string bvid, string cid, double duration)
        {
            if (!SponsorBlocks.ContainsKey(bvid)) return new List<PlayerSkipItem>();
            var bvSponsorBlocks = SponsorBlocks[bvid];
            var result = bvSponsorBlocks
                .Where(x => x.Cid == cid) // 区分cid用于多P视频
                .Where(x => Math.Abs(x.VideoDuration - duration) <= 2.0) // 剔除视频长度不等，可能换源的视频
                .OrderBy(x => x.Start) // 排个序
                .ToList();
            return result;
        }

        public void RemoveSponsorBlockCache(string bvid)
        {
            if (!SponsorBlocks.ContainsKey(bvid)) return;
            SponsorBlocks.Remove(bvid);
        }
    }
}
