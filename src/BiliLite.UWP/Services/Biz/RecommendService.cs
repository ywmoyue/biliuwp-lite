using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiliLite.Extensions;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Recommend;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api.Home;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BiliLite.Services.Biz;

/// <summary>
/// 首页推荐流。
///
/// ★ 两个来源是**两套不同的服务端模型**（不是同一个接口换参数）：
///   · App 端 —— <c>app.bilibili.com/x/v2/feed/index</c>，idx 游标，一次 3 页共 30 条，默认；
///   · 网页版 —— <c>x/web-interface/wbi/index/top/feed/rcmd</c>（wbi 签名），一次 30 条，
///     官网前端靠 fresh_idx / brush / fetch_row / last_showlist 接力翻页。
/// 两者在服务里统一整理成页面直接可用的 <see cref="RecommendItemModel"/>，
/// 页面只负责列表本身（刷新 / 加载更多 / 右键菜单 / 屏蔽过滤），不需要知道来源差异。
/// </summary>
[RegisterTransientService]
public class RecommendService : BaseBizService
{
    /// <summary>App 端一次请求几页（每页 10 条）。</summary>
    private const int AppRequestPage = 3;

    /// <summary>网页版 last_showlist 最多带多少条（官方实测 12 条，再多对服务端没有额外意义）。</summary>
    private const int WebLastShowMax = 12;

    private readonly RecommendAPI m_recommendAPI = new();
    private readonly IMapper m_mapper;

    /// <summary>"刷子"：每刷新一次 +1，让服务端知道"这是新的一轮"（翻页不动它）。</summary>
    private int m_webBrush = 1;

    /// <summary>网页版当前翻页号，从 1 开始。</summary>
    private int m_webFreshIdx = 1;

    /// <summary>已抓取到第几行（1 + 累计行数），配合 y_num 让服务端知道读到哪了。</summary>
    private int m_webFetchRow = 1;

    /// <summary>上一屏展示过的 av_{aid}，服务端据此避开重复。</summary>
    private string m_webLastShowList = "";

    public RecommendService(IMapper mapper)
    {
        m_mapper = mapper;
    }

    /// <summary>
    /// 当前设置的推荐来源是否为网页版。
    /// 设置项缺失时回落 App 端 —— 那是加入该设置之前唯一的来源。
    /// </summary>
    public bool IsWebSource => SettingService.GetValue<int>(SettingConstants.UI.RECOMMEND_SOURCE,
        SettingConstants.UI.DEFAULT_RECOMMEND_SOURCE) == SettingConstants.UI.RECOMMEND_SOURCE_WEB;

    /// <summary>
    /// 拉一批推荐流。
    /// <paramref name="idx"/> 沿用 App 端那套语义：<c>"0"</c> = 刷新（换一批），其它 = 带着游标继续往下翻。
    /// </summary>
    /// <returns>
    /// 失败返回 null —— 错误已由 <see cref="BaseBizService.HandleError"/> 提示过，
    /// 调用方保留当前列表不要清空（清空会把失败放大成"首页空白"）。
    /// </returns>
    public async Task<List<RecommendItemModel>> GetRecommendItems(string idx = "0")
    {
        try
        {
            return IsWebSource ? await GetWebRecommendItems(idx) : await GetAppRecommendItems(idx);
        }
        catch (Exception ex)
        {
            HandleError(ex);
            return null;
        }
    }

    /// <summary>
    /// 网页版「内容不感兴趣 / 不想看此UP主」。
    /// 官方首页这两个操作走同一个接口，用 reason_id 区分（<see cref="RecommendAPI.WebDislikeReasonNotInterested"/>
    /// / <see cref="RecommendAPI.WebDislikeReasonNotThisUp"/>），是**服务端反馈**，会真的影响后续推荐。
    /// </summary>
    /// <returns>失败返回 false（错误已提示过），调用方不要把卡片从列表里拿掉。</returns>
    public async Task<bool> DislikeWeb(string gotoType, string aid, string upMid, string trackId, int reasonId)
    {
        try
        {
            var api = await m_recommendAPI.WebDislike(gotoType, aid, upMid, trackId, reasonId);
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

            return true;
        }
        catch (Exception ex)
        {
            HandleError(ex);
            return false;
        }
    }

    /// <summary>
    /// App 端推荐：一次请求 3 页（每页 10 条），用上一批最后一条的 idx 接力往下翻。
    /// </summary>
    private async Task<List<RecommendItemModel>> GetAppRecommendItems(string idx)
    {
        var recommendItems = new List<RecommendItemModel>();
        var requestIdx = idx;

        for (var i = 0; i < AppRequestPage; i++)
        {
            if (i > 0) requestIdx = recommendItems.LastOrDefault()?.Idx;
            var result = await m_recommendAPI.Recommend(requestIdx).Request();
            if (!result.status)
            {
                throw new CustomizedErrorException(result.message);
            }
            var obj = result.GetJObject();
            if (obj["code"].ToInt32() != 0)
            {
                throw new CustomizedErrorException(obj["message"].ToString());
            }

            recommendItems.AddRange(JsonConvert.DeserializeObject<List<RecommendItemModel>>(obj["data"]["items"].ToString().Replace("left_bottom_rcmd_reason_style", "rcmd_reason_style")));
        }

        return recommendItems;
    }

    /// <summary>
    /// 网页版推荐：一次拿 30 条（接口上限），用内部游标接力翻页。
    /// </summary>
    private async Task<List<RecommendItemModel>> GetWebRecommendItems(string idx)
    {
        if (idx == "0") ResetWebCursor();

        // WebRecommend 是 async 的(要等 wbi 签名), 所以先拿到 api 再发请求
        var api = await m_recommendAPI.WebRecommend(m_webFreshIdx, m_webBrush, m_webFetchRow, m_webLastShowList);
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

        var recommendItems = new List<RecommendItemModel>();
        var aids = new List<long>();
        var itemArray = obj["data"]?["item"];
        if (itemArray != null && itemArray.Type == JTokenType.Array)
        {
            // 只收视频卡：直播 / 横幅 / 广告卡在推荐页没有对应的渲染分支（与 App 端只认 av 一致）
            var webItems = JsonConvert.DeserializeObject<List<RecommendWebItemModel>>(itemArray.ToString())
                               ?.Where(x => x.Goto == "av" && !string.IsNullOrEmpty(x.Bvid))
                               .ToList()
                           ?? new List<RecommendWebItemModel>();

            // 字段映射统一在 MapperExtensions 的 CreateMap<RecommendWebItemModel, RecommendItemModel> 里
            recommendItems = m_mapper.Map<List<RecommendItemModel>>(webItems);

            // 翻页号是客户端游标状态而不是接口字段: 映射完再逐条填(页面「加载更多」靠它接力)
            foreach (var item in recommendItems)
            {
                item.Idx = m_webFreshIdx.ToString();
                aids.Add(item.Args.Aid);
            }
        }

        // ★ 成功才推进游标：失败时原地不动，重试复用同一组参数（与 App 端 idx 的语义一致）
        AdvanceWebCursor(aids, obj["data"]);
        return recommendItems;
    }

    /// <summary>刷新：换一批（刷子 +1，翻页号与行号归 1，清掉上一屏清单）。</summary>
    private void ResetWebCursor()
    {
        m_webBrush++;
        m_webFreshIdx = 1;
        m_webFetchRow = 1;
        m_webLastShowList = "";
    }

    /// <summary>
    /// 一次**成功**请求之后推进游标（失败时不要调用，见 <see cref="GetWebRecommendItems"/>）。
    /// </summary>
    private void AdvanceWebCursor(List<long> aids, JToken data)
    {
        if (aids.Count > 0)
        {
            m_webFetchRow += Math.Max(1, (int)Math.Ceiling(aids.Count / (double)RecommendAPI.WebYNum));
        }

        // 翻页号以服务端回显为准（data.userfeature 里套着 fresh_idx），拿不到再本地 +1
        m_webFreshIdx = NextWebIndex(m_webFreshIdx, ReadServerFreshIdx(data));

        m_webLastShowList = string.Join(",", aids.Where(a => a > 0).Take(WebLastShowMax).Select(a => "av_" + a));
    }

    /// <summary>
    /// 下一轮页码：服务端给了就用"服务端 + 1"，否则本地 +1；两种都**不允许倒退**
    /// （服务端偶尔回一个更小的值是风控 / 缓存导致的，倒退等于把刚翻过的几页重看一遍）。
    /// </summary>
    private static int NextWebIndex(int current, int server)
    {
        var next = server > 0 ? server + 1 : current + 1;
        return next > current ? next : current + 1;
    }

    /// <summary>
    /// 读响应里回显的 fresh_idx。★ 这个字段是**字符串里套 JSON**
    /// （实测形如 "{\"enter_rank\":1500,\"fresh_idx\":3,...}"），不是对象，直接按对象取值取不到；
    /// 解析失败一律返回 0（退回本地自增），不能抛 —— 它只是让翻页更准的补充信息。
    /// </summary>
    private static int ReadServerFreshIdx(JToken data)
    {
        try
        {
            var userFeature = data?["userfeature"];
            if (userFeature == null) return 0;
            var obj = userFeature.Type == JTokenType.String
                ? JObject.Parse(userFeature.ToString())
                : userFeature as JObject;
            return obj?["fresh_idx"]?.ToInt32() ?? 0;
        }
        catch
        {
            return 0;
        }
    }
}
