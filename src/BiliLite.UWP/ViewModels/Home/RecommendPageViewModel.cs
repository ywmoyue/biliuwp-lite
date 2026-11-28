using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Recommend;
using BiliLite.Models.Common.Settings;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests;
using BiliLite.Models.Requests.Api.Home;
using BiliLite.Models.Requests.Api.User;
using BiliLite.Modules;
using BiliLite.Services;
using BiliLite.Services.Biz;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BiliLite.ViewModels.Home
{
    public class RecommendPageViewModel : BaseViewModel
    {
        #region Fields

        private readonly RecommendAPI m_recommendApi;
        private readonly FollowAPI m_followApi;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly ContentFilterService m_contentFilterService;

        /// <summary>
        /// 推荐流来源、游标状态与两套接口的解析都在这个业务服务里（见 Services/Biz/RecommendService）。
        /// ★ 它持有网页版的翻页游标，所以必须是同一个实例跨调用复用（构造函数注入一次，别再 GetRequiredService）。
        /// </summary>
        private readonly RecommendService m_recommendService;

        /// <summary>
        /// 当前列表是用哪个来源拉的（null = 还没拉过）。
        /// 只用来识别"用户中途切换了推荐来源" —— 那种情况下旧列表与旧游标都要作废，见 GetRecommend。
        /// </summary>
        private bool? m_itemsFromWeb;

        #endregion

        #region Constructors

        public RecommendPageViewModel(ContentFilterService contentFilterService, RecommendService recommendService)
        {
            m_contentFilterService = contentFilterService;
            m_recommendService = recommendService;
            m_recommendApi = new RecommendAPI();
            m_followApi = new FollowAPI();
            Banner = new ObservableCollection<RecommendBannerItemModel>();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
        }

        #endregion

        #region Properties

        public ICommand RefreshCommand { get; private set; }

        public ICommand LoadMoreCommand { get; private set; }

        [DoNotNotify]
        public double ScrollViewLoadMoreBottomOffset { get; } =
            SettingService.GetValue(SettingConstants.UI.SCROLL_VIEW_LOAD_MORE_BOTTOM_OFFSET, SettingConstants.UI.DEFAULT_SCROLL_VIEW_LOAD_MORE_BOTTOM_OFFSET);

        public bool Loading { get; set; } = true;

        public ObservableCollection<RecommendBannerItemModel> Banner { get; set; }

        public ObservableCollection<RecommendItemModel> Items { get; set; }

        [DependsOn(nameof(Banner))]
        public bool ShowBanner
        {
            get
            {
                if (!SettingService.GetValue(SettingConstants.UI.DISPLAY_RECOMMEND_BANNER,
                        SettingConstants.UI.DEFAULT_DISPLAY_RECOMMEND_BANNER))
                {
                    return false;
                }
                return Banner != null && Banner.Any();
            }
        }

        #endregion

        #region Private Methods

        private async void LoadMore()
        {
            try
            {
                if (Items == null || Items.Count == 0)
                {
                    return;
                }
                if (Loading)
                {
                    return;
                }

                var idx = Items.LastOrDefault()?.Idx;

                if (string.IsNullOrEmpty(idx))
                {
                    await GetRecommend();
                    return;
                }

                await GetRecommend(idx);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
                NotificationShowExtensions.ShowMessageToast(ex.Message);
            }
        }

        private void LoadBanner(RecommendItemModel banner)
        {
            try
            {
                if (Banner.Any()) return;
                foreach (var item in banner.BannerItem)
                {
                    if (item["type"].ToString() == "static")
                    {
                        Banner.Add(JsonConvert.DeserializeObject<RecommendBannerItemModel>(item["static_banner"].ToString()));
                    }
                    if (item["type"].ToString() == "ad")
                    {
                        Banner.Add(JsonConvert.DeserializeObject<RecommendBannerItemModel>(item["ad_banner"].ToString()));
                    }
                }
                Set(nameof(ShowBanner));
            }
            catch (Exception ex)
            {
                _logger.Debug($"catch error: {ex.Message}");
            }
        }

        #endregion

        #region Public Methods

        public async Task GetRecommend(string idx = "0")
        {
            try
            {
                Loading = true;

                // 来源被中途切换过(用户改了设置又回到本页): 旧列表与旧游标都不再适用 ——
                // 按新来源从第一页重来, 否则「加载更多」会把两套推荐混在同一个列表里,
                // 而且 App 端的 idx 对网页版游标没有意义(反之亦然)。
                var useWeb = m_recommendService.IsWebSource;
                var sourceChanged = m_itemsFromWeb.HasValue && m_itemsFromWeb.Value != useWeb;

                // 拉取与解析在业务服务里, 这里只拿整理好的条目
                var recommendItems = await m_recommendService.GetRecommendItems(sourceChanged ? "0" : idx);

                // 失败时服务已经提示过: 保持现有列表不动, 别把失败放大成"首页空白"
                if (recommendItems == null) return;

                // 拿到新来源的内容之后, 才丢掉旧列表(失败时旧列表还在, 用户可以继续看)
                if (sourceChanged) Items = null;
                m_itemsFromWeb = useWeb;

                var banner = recommendItems.FirstOrDefault(x => x.CardGoto == "banner");
                if (banner != null)
                {
                    //处理banner
                    LoadBanner(banner);
                    recommendItems.Remove(banner);
                }
                for (var i = recommendItems.Count - 1; i >= 0; i--)
                {
                    if (recommendItems[i].ShowAd)
                    {
                        recommendItems.Remove(recommendItems[i]);
                        continue;
                    }

                    if (recommendItems[i].AdInfo != null && recommendItems[i].AdInfo.CreativeContent == null)
                    {
                        recommendItems.Remove(recommendItems[i]);
                        continue;
                    }

                    var item = recommendItems[i];
                    if (item.ThreePointV2 != null && item.ThreePointV2.Count > 0 && item.CardGoto == "av")
                    {
                        item.ThreePointV2.Insert(1, new RecommendThreePointV2ItemModel()
                        {
                            Idx = item.Idx,
                            Url = $"https://b23.tv/av{item.Param}",
                            Title = "使用浏览器打开",
                            Type = "browser"
                        });
                    }

                    if (item.ThreePointV2 != null)
                    {
                        // ★ 插入位置必须跟着表的长度走: App 端服务端至少给一条, 插到第 1 位排在"不感兴趣"之后;
                        //   网页版接口不下发 three_point_v2, 拿到的是**空表**, 空表 Insert(1) 直接抛
                        //   "Index must be within the bounds of the List"(空表只允许 index 0) ——
                        //   2026-11-25 用户报的"设置网页版后推荐源报错"就是这一句。
                        var insertIndex = Math.Min(1, item.ThreePointV2.Count);
                        item.ThreePointV2.Insert(insertIndex, new RecommendThreePointV2ItemModel()
                        {
                            Idx = item.Idx,
                            Title = $"过滤UP主",
                            Subtitle = item.Args.UpName,
                            Type = "fastFilter"
                        });
                        item.ThreePointV2.Insert(insertIndex, new RecommendThreePointV2ItemModel()
                        {
                            Idx = item.Idx,
                            Title = $"拉黑UP主",
                            Subtitle = item.Args.UpName,
                            UpId = item.Args.UpId,
                            Type = "blockUser"
                        });
                    }

                    // ★ 网页版接口不下发 three_point_v2(抓包实测 12/12 条都没有), 所以官方首页那两个操作
                    //   (内容不感兴趣 / 不想看此UP主)只能自己拼菜单: 它们走同一个服务端接口,
                    //   靠 reason_id 区分, aid / mid / track_id 都从这张卡带过去(见 WebDislike)。
                    if (useWeb && item.ThreePointV2 != null)
                    {
                        item.ThreePointV2.Insert(0, new RecommendThreePointV2ItemModel()
                        {
                            Idx = item.Idx,
                            Title = "不想看此UP主",
                            Subtitle = item.Args.UpName,
                            ReasonId = RecommendAPI.WebDislikeReasonNotThisUp,
                            Type = "webDislike"
                        });
                        item.ThreePointV2.Insert(0, new RecommendThreePointV2ItemModel()
                        {
                            Idx = item.Idx,
                            Title = "内容不感兴趣",
                            Subtitle = "(将减少相似内容推荐)",
                            ReasonId = RecommendAPI.WebDislikeReasonNotInterested,
                            Type = "webDislike"
                        });
                    }
                }

                recommendItems = m_contentFilterService.FilterRecommendItems(recommendItems);
                var items = new ObservableCollection<RecommendItemModel>(recommendItems);

                if (Items == null)
                {
                    Items = items;
                    //await GetRecommend(items.LastOrDefault().idx);
                }
                else
                {
                    foreach (var item in items)
                    {
                        Items.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is CustomizedErrorException)
                {
                    _logger.Error(ex.Message, ex);
                    NotificationShowExtensions.ShowMessageToast(ex.Message);
                    return;
                }
                var handel = HandelError<RecommendPageViewModel>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

        public async void Refresh()
        {
            if (Loading)
            {
                NotificationShowExtensions.ShowMessageToast("正在加载中....");
                return;
            }
            // Banner = null;
            Items = null;
            await GetRecommend();
        }

        /// <summary>
        /// 网页版「内容不感兴趣 / 不想看此UP主」。
        /// 与 App 端不同: 这两个是**服务端反馈**(x/web-interface/feedback/dislike),
        /// 成功之后才把这张卡从列表里拿掉; 失败保持原样, 免得"点了没生效还以为成功"。
        /// </summary>
        public async Task WebDislike(RecommendItemModel item, int reasonId)
        {
            if (item == null) return;

            if (!SettingService.Account.Logined && !await NotificationShowExtensions.ShowLoginDialog())
            {
                NotificationShowExtensions.ShowMessageToast("请先登录");
                return;
            }

            var ok = await m_recommendService.DislikeWeb(item.CardGoto, item.Param, item.Args.UpId,
                item.TrackId, reasonId);
            if (!ok) return;

            Items?.Remove(item);
            NotificationShowExtensions.ShowMessageToast(
                reasonId == RecommendAPI.WebDislikeReasonNotThisUp ? "已反馈: 不想看此UP主" : "已反馈: 内容不感兴趣");
        }

        public async Task Dislike(string idx, RecommendThreePointV2ItemModel threePointV2Item, RecommendThreePointV2ItemReasonsModel itemReasons)
        {
            try
            {
                if (!SettingService.Account.Logined && await NotificationShowExtensions.ShowLoginDialog())
                {
                    NotificationShowExtensions.ShowMessageToast("请先登录");
                    return;
                }
                var recommendItem = Items.FirstOrDefault(x => x.Idx == idx);
                var feedbackParams = new RecommondFeedbackParams()
                {
                    GoTo = recommendItem.CardGoto,
                    Id = recommendItem.Param,
                    Mid = recommendItem.Args.UpId,
                    ReasonId = itemReasons?.Id ?? 0,
                    Rid = recommendItem.Args.Rid,
                    TagId = recommendItem.Args.Tid,
                };
                var api = m_recommendApi.Dislike(feedbackParams);
                if (threePointV2Item.Type == "feedback")
                {
                    api = m_recommendApi.Feedback(feedbackParams);
                }
                var result = await api.Request();

                if (!result.status)
                {
                    throw new CustomizedErrorException(result.message);
                }
                var obj = result.GetJObject();
                if (obj["code"].ToInt32() != 0)
                {
                    throw new CustomizedErrorException(obj["message"].ToString());
                }
                Items.Remove(recommendItem);
            }
            catch (Exception ex)
            {
                if (ex is CustomizedErrorException)
                {
                    _logger.Error(ex.Message, ex);
                    NotificationShowExtensions.ShowMessageToast(ex.Message);
                    return;
                }
                var handel = HandelError<RecommendPageViewModel>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
        }

        public void AddFilterUser(string name)
        {
            m_contentFilterService.AddRecommendFilterRule(new FilterRule()
            {
                FilterRuleType = FilterRuleType.Recommend,
                FilterType = FilterType.Word,
                ContentType = FilterContentType.User,
                Enable = true,
                Rule = name,
            });
            var filterItems = Items.Where(x => x.Args.UpName == name).ToList();
            foreach (var filterItem in filterItems)
            {
                Items.Remove(filterItem);
            }
        }

        public async Task BlockUser(string upId, string upName)
        {
            try
            {
                if (!SettingService.Account.Logined && !await NotificationShowExtensions.ShowLoginDialog())
                {
                    NotificationShowExtensions.ShowMessageToast("请先登录");
                    return;
                }
                var results = await m_followApi.BlockUser(upId).Request();
                if (!results.status)
                {
                    throw new CustomizedErrorException(results.message);
                }
                var data = await results.GetJson<ApiDataModel<object>>();
                if (!data.success)
                {
                    throw new CustomizedErrorException(data.message);
                }
                NotificationShowExtensions.ShowMessageToast($"已拉黑 {upName}");
                var blockItems = Items.Where(x => x.Args.UpId == upId).ToList();
                foreach (var blockItem in blockItems)
                {
                    Items.Remove(blockItem);
                }
            }
            catch (Exception ex)
            {
                if (ex is CustomizedErrorException)
                {
                    _logger.Error(ex.Message, ex);
                    NotificationShowExtensions.ShowMessageToast(ex.Message);
                    return;
                }
                var handel = HandelError<RecommendPageViewModel>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
        }

        #endregion
    }
}
