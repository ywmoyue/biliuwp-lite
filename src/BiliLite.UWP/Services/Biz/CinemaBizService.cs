using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiliLite.Converters;
using BiliLite.Extensions;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common.Home;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api.Home;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace BiliLite.Services.Biz
{
    /// <summary>
    /// 影视首页数据（原 BiliLite.WebApi 的 CinemaController 内置化）
    /// </summary>
    [RegisterSingletonService]
    public class CinemaBizService : BaseBizService
    {
        #region Constants

        private const int MAX_HOT_ITEM_COUNT = 36;

        private static readonly TimeSpan _homeCacheTime = TimeSpan.FromHours(2);
        private static readonly TimeSpan _hotCacheTime = TimeSpan.FromHours(2);

        #endregion

        #region Fields

        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        private readonly CinemaAPI m_cinemaAPI = new CinemaAPI();
        private readonly CacheService m_cacheService;

        #endregion

        #region Constructors

        public CinemaBizService(CacheService cacheService)
        {
            m_cacheService = cacheService;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 获取影视首页数据
        /// </summary>
        public async Task<CinemaHomeModel> GetHome()
        {
            const string cacheKey = "CinemaHome";
            var cached = await m_cacheService.GetAsync<CinemaHomeModel>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            var home = new CinemaHomeModel()
            {
                Banners = await GetBanners(),
                // 上游 bangumi.bilibili.com/api/fall 已长期返回空数组，"独家策划"没有数据可用，先不展示
                Falls = new List<CinemaHomeFallModel>(),
                Update = await GetOnline(),
                Movie = await GetHot(2),
                Documentary = await GetHot(3),
                Tv = await GetHot(5),
                Variety = await GetHot(7),
            };

            if (home.Banners.Count == 0 && home.Update.Count == 0 && home.Movie.Count == 0 &&
                home.Tv.Count == 0)
            {
                throw new CustomizedErrorException("影视首页数据获取失败，请检查网络后重试");
            }

            await m_cacheService.SetAsync(cacheKey, home, _homeCacheTime);
            return home;
        }

        #endregion

        #region Private Methods

        private async Task<List<CinemaHomeBannerModel>> GetBanners()
        {
            try
            {
                var result = await m_cinemaAPI.HomePage().Request();
                if (!result.status)
                {
                    throw new CustomizedErrorException(result.message);
                }

                var html = result.results;

                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                var node = doc.DocumentNode.SelectSingleNode("//ul[@class='carou-images-wrapper clearfix']");
                var nodes = node.SelectNodes("li[@class='chief-recom-item']/a");

                var banners = new List<CinemaHomeBannerModel>();
                foreach (var item in nodes)
                {
                    var img = item.SelectSingleNode("picture/img");
                    banners.Add(new CinemaHomeBannerModel()
                    {
                        Img = img.Attributes["src"].Value.Split('@')[0].Replace("//", "https://"),
                        Title = img.Attributes["alt"].Value,
                        Url = item.Attributes["href"].Value,
                    });
                }

                return banners.Distinct(new BannerComparer()).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error("获取影视首页Banner失败", ex);
                return new List<CinemaHomeBannerModel>();
            }
        }

        /// <summary>
        /// 热门推荐
        /// </summary>
        /// <param name="oid">2=电影，3=纪录片，5=电视剧，7=综艺</param>
        private async Task<List<CinemaHomeHotItem>> GetHot(int oid)
        {
            var cacheKey = $"CinemaHot-{oid}";
            var cached = await m_cacheService.GetAsync<List<CinemaHomeHotItem>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            try
            {
                var result = await m_cinemaAPI.SeasonRank(oid).Request();
                if (!result.status)
                {
                    throw new CustomizedErrorException(result.message);
                }

                var obj = result.GetJObject();

                var list = new List<CinemaHomeHotItem>();
                foreach (var item in obj["data"]["list"].Take(MAX_HOT_ITEM_COUNT))
                {
                    list.Add(ToHotItem(item, oid));
                }

                if (list.Count != 0)
                {
                    await m_cacheService.SetAsync(cacheKey, list, _hotCacheTime);
                }

                return list;
            }
            catch (Exception ex)
            {
                _logger.Error("获取影视首页热门推荐失败", ex);
                return new List<CinemaHomeHotItem>();
            }
        }

        /// <summary>
        /// 即将上线
        /// </summary>
        private async Task<List<CinemaHomeHotItem>> GetOnline()
        {
            const string cacheKey = "CinemaOnline";
            var cached = await m_cacheService.GetAsync<List<CinemaHomeHotItem>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            try
            {
                var result = await m_cinemaAPI.Online().Request();
                if (!result.status)
                {
                    throw new CustomizedErrorException(result.message);
                }

                var obj = result.GetJObject();

                var list = new List<CinemaHomeHotItem>();
                foreach (var item in obj["result"]["items"].Take(MAX_HOT_ITEM_COUNT))
                {
                    list.Add(ToHotItem(item, -1));
                }

                if (list.Count != 0)
                {
                    await m_cacheService.SetAsync(cacheKey, list, _hotCacheTime);
                }

                return list;
            }
            catch (Exception ex)
            {
                _logger.Error("获取影视首页即将上线失败", ex);
                return new List<CinemaHomeHotItem>();
            }
        }

        /// <summary>
        /// 把接口返回的条目转换为客户端模型
        /// </summary>
        /// <param name="oid">热门推荐的分区，-1 表示即将上线</param>
        private static CinemaHomeHotItem ToHotItem(JToken item, int oid)
        {
            var stat = item["stat"];
            var newEp = item["new_ep"];

            var hotItem = new CinemaHomeHotItem()
            {
                Hat = item["desc"]?.ToString(),
                Cover = item["cover"]?.ToString(),
                Badge = item["badge"]?.ToString(),
                BadgeType = item["badge_type"]?.ToObject<int>() ?? 0,
                Desc = item["desc"]?.ToString(),
                Title = item["title"]?.ToString(),
                Link = item["link"]?.ToString(),
                SeasonId = item["season_id"]?.ToObject<int>() ?? 0,
                SeasonType = item["season_type"]?.ToObject<int>() ?? 0,
                Type = item["type"]?.ToString(),
                Wid = item["wid"]?.ToObject<int>() ?? 0,
                Stat = new CinemaHomeStatModel()
                {
                    View = stat?["view"]?.ToObject<long>() ?? 0,
                    Follow = (stat?["follow"] ?? stat?["follower"])?.ToObject<long>() ?? 0,
                    Danmaku = stat?["danmaku"]?.ToObject<long>() ?? 0,
                },
            };

            if (oid == 5 || oid == 7)
            {
                hotItem.Cover = newEp?["cover"]?.ToString();
            }

            if (oid == 2)
            {
                hotItem.Desc = newEp?["index_show"]?.ToString();
            }

            hotItem.Stat.FollowView = ToNumberText(hotItem.Stat.Follow) + "人追剧";
            return hotItem;
        }

        private static string ToNumberText(long number)
        {
            return NumberToStringConvert.Convert(number, null);
        }

        #endregion

        #region Nested Types

        private sealed class BannerComparer : IEqualityComparer<CinemaHomeBannerModel>
        {
            public bool Equals(CinemaHomeBannerModel x, CinemaHomeBannerModel y)
            {
                return x?.Url == y?.Url;
            }

            public int GetHashCode(CinemaHomeBannerModel obj)
            {
                return obj?.Url?.GetHashCode() ?? 0;
            }
        }

        #endregion
    }
}
