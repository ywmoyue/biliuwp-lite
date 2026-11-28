using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BiliLite.Converters;
using BiliLite.Extensions;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common.Anime;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api.Home;
using Newtonsoft.Json.Linq;

namespace BiliLite.Services.Biz
{
    /// <summary>
    /// 番剧/国创首页数据（原 BiliLite.WebApi 的 AnimeController 内置化）
    /// </summary>
    [RegisterSingletonService]
    public class AnimeBizService : BaseBizService
    {
        #region Constants

        private const int BANGUMI_SEASON_TYPE = 1;

        /// <summary>
        /// 推荐流里的题材榜块标记
        /// </summary>
        private const string RANK_CARD_STYLE = "rank";

        /// <summary>
        /// 推荐流里普通卡片的聚合分组标题
        /// </summary>
        private const string RECOMMEND_SECTION_TITLE = "猜你喜欢";

        private static readonly TimeSpan _homeCacheTime = TimeSpan.FromHours(2);
        private static readonly TimeSpan _timelineCacheTime = TimeSpan.FromHours(2);

        #endregion

        #region Fields

        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        private readonly AnimeAPI m_animeAPI = new AnimeAPI();
        private readonly CacheService m_cacheService;

        #endregion

        #region Constructors

        public AnimeBizService(CacheService cacheService)
        {
            m_cacheService = cacheService;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 获取番剧/国创首页数据
        /// </summary>
        /// <param name="seasonType">1=番剧，4=国创</param>
        public async Task<AnimeHomeModel> GetHome(int seasonType)
        {
            var isBangumi = seasonType == BANGUMI_SEASON_TYPE;
            var cacheKey = isBangumi ? "AnimeHome-Bangumi" : "AnimeHome-Guochuang";
            var cached = await m_cacheService.GetAsync<AnimeHomeModel>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            var banners = await GetBanners(seasonType);
            var ranks = await GetRanks(seasonType);
            var timelines = await GetTimelines(seasonType);
            var feed = await GetHomeFeed(seasonType);

            var home = new AnimeHomeModel()
            {
                Banners = banners,
                Ranks = ranks,
                Today = timelines.FirstOrDefault(x => x.IsToday)?.Seasons,
                Hots = await GetHots(seasonType),
                Falls = feed.Sections,
                FeedCursor = feed.NextCursor,
                HasMoreFeed = feed.HasMore,
            };

            if (home.Banners.Count == 0 && home.Ranks.Count == 0 && home.Hots.Count == 0)
            {
                throw new CustomizedErrorException("番剧首页数据获取失败，请检查网络后重试");
            }

            if (home.Ranks.Count != 0 && home.Banners.Count != 0)
            {
                await m_cacheService.SetAsync(cacheKey, home, _homeCacheTime);
            }

            return home;
        }

        /// <summary>
        /// 获取番剧/国创时间表
        /// </summary>
        /// <param name="seasonType">1=番剧，4=国创</param>
        public async Task<List<AnimeTimelineModel>> GetTimeline(int seasonType)
        {
            var timelines = await GetTimelines(seasonType);
            if (timelines.Count == 0)
            {
                throw new CustomizedErrorException("时间表数据获取失败，请检查网络后重试");
            }

            return timelines;
        }

        /// <summary>
        /// 获取首页推荐流首屏（题材榜 + 猜你喜欢）
        /// </summary>
        /// <param name="seasonType">1=番剧，4=国创</param>
        public Task<AnimeFeedModel> GetHomeFeed(int seasonType)
        {
            return GetFeed(seasonType, 0);
        }

        /// <summary>
        /// 首页推荐流加载更多
        /// </summary>
        /// <param name="seasonType">1=番剧，4=国创</param>
        /// <param name="cursor">上一页返回的游标</param>
        public Task<AnimeFeedModel> GetFeedMore(int seasonType, int cursor)
        {
            return GetFeed(seasonType, cursor);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 获取首页推荐流：每页 29 个块 = 28 张推荐卡片 + 1 个题材榜（20 条）
        /// </summary>
        private async Task<AnimeFeedModel> GetFeed(int seasonType, int cursor)
        {
            var feed = new AnimeFeedModel();
            try
            {
                var result = await m_animeAPI.Feed(seasonType, cursor).Request();
                if (!result.status)
                {
                    throw new CustomizedErrorException(result.message);
                }

                var data = result.GetJObject()["data"];
                if (data == null || data.Type == JTokenType.Null)
                {
                    _logger.Info($"首页推荐流无数据：seasonType={seasonType}, coursor={cursor}");
                    return feed;
                }

                feed.NextCursor = data["coursor"]?.ToObject<int>() ?? 0;
                feed.HasMore = data["has_next"]?.ToObject<bool>() ?? false;

                var blocks = data["items"] as JArray;
                if (blocks == null)
                {
                    return feed;
                }

                var cards = new List<AnimeFallItemModel>();
                foreach (var block in blocks)
                {
                    var subItems = block["sub_items"] as JArray;
                    if (subItems == null)
                    {
                        continue;
                    }

                    foreach (var subItem in subItems)
                    {
                        if (subItem["card_style"]?.ToString() == RANK_CARD_STYLE)
                        {
                            var rankItems = ToFallItems(subItem["sub_items"] as JArray);
                            if (rankItems.Count == 0)
                            {
                                continue;
                            }

                            feed.Sections.Add(new AnimeFallModel()
                            {
                                Wid = subItem["rank_id"]?.ToObject<int>() ?? 0,
                                Title = subItem["title"]?.ToString(),
                                Items = ToCollection(rankItems),
                            });
                        }
                        else
                        {
                            var card = ToFallItem(subItem);
                            if (card != null)
                            {
                                cards.Add(card);
                            }
                        }
                    }
                }

                if (cards.Count != 0)
                {
                    feed.Sections.Add(new AnimeFallModel()
                    {
                        Title = RECOMMEND_SECTION_TITLE,
                        Items = ToCollection(cards),
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.Error("获取首页推荐流失败", ex);
            }

            return feed;
        }

        private static List<AnimeFallItemModel> ToFallItems(JArray array)
        {
            var items = new List<AnimeFallItemModel>();
            if (array == null)
            {
                return items;
            }

            foreach (var token in array)
            {
                var item = ToFallItem(token);
                if (item != null)
                {
                    items.Add(item);
                }
            }

            return items;
        }

        private static AnimeFallItemModel ToFallItem(JToken token)
        {
            var title = token["title"]?.ToString();
            if (string.IsNullOrEmpty(title))
            {
                return null;
            }

            return new AnimeFallItemModel()
            {
                Title = title,
                Cover = ToImageUrl(token["cover"]?.ToString()),
                Desc = ToDesc(token),
                Link = NormalizeUrl(token["link"]?.ToString()),
                Wid = token["season_id"]?.ToObject<int>() ?? 0,
            };
        }

        /// <summary>
        /// 简介：优先 hover.text（数组或字符串两种形态），其次 sub_title / evaluate
        /// </summary>
        private static string ToDesc(JToken token)
        {
            var text = token["hover"]?["text"];
            if (text is JArray array)
            {
                return string.Join(" · ", array.Select(x => x.ToString()));
            }

            if (text != null && text.Type != JTokenType.Null)
            {
                return text.ToString();
            }

            return token["sub_title"]?.ToString() ?? token["evaluate"]?.ToString();
        }

        private static string ToImageUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            if (url.StartsWith("//"))
            {
                return "https:" + url;
            }

            return url.Replace("http://", "https://");
        }

        /// <summary>
        /// 获取首页轮播图：番剧/国创首页已改为前端渲染，Banner 数据在 window.__INITIAL_STATE__ 里
        /// </summary>
        private async Task<List<AnimeBannerModel>> GetBanners(int seasonType)
        {
            try
            {
                var result = await m_animeAPI.HomePage(seasonType).Request();
                if (!result.status)
                {
                    throw new CustomizedErrorException(result.message);
                }

                var items = ParseInitialState(result.results)?["modules"]?["banner"]?["items"] as JArray;
                if (items == null)
                {
                    _logger.Info($"未从页面中解析到Banner数据：seasonType={seasonType}");
                    return new List<AnimeBannerModel>();
                }

                var banners = new List<AnimeBannerModel>();
                foreach (var item in items)
                {
                    var link = NormalizeUrl(item["link"]?.ToString());
                    if (string.IsNullOrEmpty(link))
                    {
                        continue;
                    }

                    banners.Add(new AnimeBannerModel()
                    {
                        Title = item["title"]?.ToString(),
                        Img = NormalizeUrl(item["cover"]?.ToString()) ??
                              NormalizeUrl(item["big_cover"]?.ToString()),
                        Url = link,
                    });
                }

                return banners.Distinct(new BannerComparer()).ToList();
            }
            catch (Exception ex)
            {
                _logger.Error("获取番剧首页Banner失败", ex);
                return new List<AnimeBannerModel>();
            }
        }

        /// <summary>
        /// 取出 window.__INITIAL_STATE__ 的 JSON：按大括号配对截取，避免 JSON 字符串里的分号干扰
        /// </summary>
        private static JObject ParseInitialState(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return null;
            }

            const string marker = "__INITIAL_STATE__=";
            var markerIndex = html.IndexOf(marker, StringComparison.Ordinal);
            if (markerIndex < 0)
            {
                return null;
            }

            var start = html.IndexOf('{', markerIndex + marker.Length);
            if (start < 0)
            {
                return null;
            }

            var depth = 0;
            var inString = false;
            var escaped = false;
            for (var i = start; i < html.Length; i++)
            {
                var ch = html[i];
                if (inString)
                {
                    if (escaped)
                    {
                        escaped = false;
                    }
                    else if (ch == '\\')
                    {
                        escaped = true;
                    }
                    else if (ch == '"')
                    {
                        inString = false;
                    }

                    continue;
                }

                if (ch == '"')
                {
                    inString = true;
                    continue;
                }

                if (ch == '{')
                {
                    depth++;
                    continue;
                }

                if (ch != '}')
                {
                    continue;
                }

                depth--;
                if (depth != 0)
                {
                    continue;
                }

                try
                {
                    return JObject.Parse(html.Substring(start, i - start + 1));
                }
                catch (Exception ex)
                {
                    _logger.Error("解析页面初始数据失败", ex);
                    return null;
                }
            }

            return null;
        }

        private static string NormalizeUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return null;
            }

            return url.StartsWith("//") ? "https:" + url : url;
        }

        private async Task<List<AnimeRankModel>> GetRanks(int seasonType)
        {
            try
            {
                var result = await m_animeAPI.Rank(seasonType).Request();
                if (!result.status)
                {
                    throw new CustomizedErrorException(result.message);
                }

                var obj = result.GetJObject();

                var ranks = new List<AnimeRankModel>();
                foreach (var item in obj["result"]["list"].Take(20))
                {
                    ranks.Add(new AnimeRankModel()
                    {
                        Display = ToNumberText(item["stat"]["follow"]) + "人追番",
                        Badge = item["badge"].ToString(),
                        Cover = item["cover"].ToString(),
                        SeasonId = Convert.ToInt32(item["season_id"]),
                        Danmaku = Convert.ToInt64(item["stat"]["danmaku"]),
                        Follow = Convert.ToInt64(item["stat"]["follow"]),
                        View = Convert.ToInt64(item["stat"]["view"]),
                        IndexShow = item["new_ep"]["index_show"].ToString(),
                        Title = item["title"].ToString(),
                    });
                }

                return ranks;
            }
            catch (Exception ex)
            {
                _logger.Error("获取番剧首页榜单失败", ex);
                return new List<AnimeRankModel>();
            }
        }

        private async Task<List<AnimeRankModel>> GetHots(int seasonType)
        {
            try
            {
                var result = await m_animeAPI.Hot(seasonType).Request();
                if (!result.status)
                {
                    throw new CustomizedErrorException(result.message);
                }

                var obj = result.GetJObject();

                var hots = new List<AnimeRankModel>();
                foreach (var item in obj["data"]["list"].Take(20))
                {
                    hots.Add(new AnimeRankModel()
                    {
                        Badge = item["badge"].ToString(),
                        Cover = item["cover"].ToString(),
                        SeasonId = Convert.ToInt32(item["season_id"]),
                        Display = item["order"].ToString(),
                        IndexShow = item["index_show"].ToString(),
                        Title = item["title"].ToString(),
                    });
                }

                return hots;
            }
            catch (Exception ex)
            {
                _logger.Error("获取番剧首页热门失败", ex);
                return new List<AnimeRankModel>();
            }
        }

        private async Task<List<AnimeTimelineModel>> GetTimelines(int seasonType)
        {
            var cacheKey = $"AnimeTimeline-{seasonType}";
            var cached = await m_cacheService.GetAsync<List<AnimeTimelineModel>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            try
            {
                var result = await m_animeAPI.Timeline(seasonType).Request();
                if (!result.status)
                {
                    throw new CustomizedErrorException(result.message);
                }

                var obj = result.GetJObject();

                var timelines = new List<AnimeTimelineModel>();
                foreach (var item in obj["result"])
                {
                    var dayWeek = Convert.ToInt32(item["day_of_week"]);
                    var seasons = new List<AnimeTimelineItemModel>();
                    foreach (var item2 in item["seasons"])
                    {
                        var pubIndex = Convert.ToInt32(item2["delay"]) == 1
                            ? item2["delay_reason"].ToString()
                            : item2["pub_index"].ToString();
                        seasons.Add(new AnimeTimelineItemModel()
                        {
                            Cover = item2["cover"].ToString(),
                            PubIndex = pubIndex,
                            PubTime = item2["pub_time"].ToString(),
                            SeasonId = Convert.ToInt32(item2["season_id"]),
                            SquareCover = item2["square_cover"].ToString(),
                            Title = item2["title"].ToString(),
                        });
                    }

                    timelines.Add(new AnimeTimelineModel()
                    {
                        Date = item["date"].ToString(),
                        DayWeek = dayWeek,
                        IsToday = Convert.ToInt32(item["is_today"]) == 1,
                        Week = GetWeekName(dayWeek),
                        Seasons = seasons,
                    });
                }

                if (timelines.Count != 0)
                {
                    await m_cacheService.SetAsync(cacheKey, timelines, _timelineCacheTime);
                }

                return timelines;
            }
            catch (Exception ex)
            {
                _logger.Error("获取番剧时间表失败", ex);
                return new List<AnimeTimelineModel>();
            }
        }

        private static ObservableCollection<AnimeFallItemModel> ToCollection(List<AnimeFallItemModel> items)
        {
            return new ObservableCollection<AnimeFallItemModel>(items);
        }

        private static string GetWeekName(int dayWeek)
        {
            switch (dayWeek)
            {
                case 1: return "周一";
                case 2: return "周二";
                case 3: return "周三";
                case 4: return "周四";
                case 5: return "周五";
                case 6: return "周六";
                case 7: return "周日";
                default: return "未知";
            }
        }

        private static string ToNumberText(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
            {
                return "";
            }

            long.TryParse(token.ToString(), out var number);
            return NumberToStringConvert.Convert(number, null);
        }

        #endregion

        #region Nested Types

        private sealed class BannerComparer : IEqualityComparer<AnimeBannerModel>
        {
            public bool Equals(AnimeBannerModel x, AnimeBannerModel y)
            {
                return x?.Url == y?.Url;
            }

            public int GetHashCode(AnimeBannerModel obj)
            {
                return obj?.Url?.GetHashCode() ?? 0;
            }
        }

        #endregion
    }
}
