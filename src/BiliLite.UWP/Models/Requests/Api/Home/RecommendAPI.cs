using System;
using System.Threading.Tasks;
using BiliLite.Models.Common;
using BiliLite.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLite.Models.Requests.Api.Home
{
    public class RecommendAPI : BaseApi
    {
        /// <summary>
        /// 网页版「不感兴趣」的理由 id。官方首页写死两项, 走同一个接口。
        /// </summary>
        public const int WebDislikeReasonNotInterested = 1;

        /// <summary>
        /// 网页版「不想看此UP主」的理由 id。
        /// </summary>
        public const int WebDislikeReasonNotThisUp = 4;

        /// <summary>
        /// 网页版首页请求里一起参与 wbi 签名的客户端元信息(照抄官方请求, 值本身是 URL 编码过的 JSON)。
        /// </summary>
        private const string WebClientMetaQuery =
            "x-bili-locale-json=%7B%22c_locale%22%3A%7B%22language%22%3A%22zh%22%2C%22script%22%3A%22Hans%22%7D%2C%22always_translate%22%3Afalse%7D" +
            "&x-bili-device-req-json=%7B%22platform%22%3A%22web%22%2C%22device%22%3A%22pc%22%2C%22spmid%22%3A%22333.1007%22%2C%22mobi_app%22%3A%22web_cn%22%7D";

        private readonly CookieService m_cookieService;

        /// <summary>
        /// 网页版推荐单页条数，接口硬上限 30。
        /// </summary>
        public const int WebPageSize = 30;

        /// <summary>
        /// 网页版推荐的"列数"。官方前端默认 4，与 <see cref="WebPageSize"/> 一起决定这一页占几行，
        /// 进而决定 fetch_row 每次该加多少。
        /// </summary>
        public const int WebYNum = 4;

        /// <summary>
        /// 本次运行的会话 id（官方前端也是随机生成、整轮不变）。
        /// </summary>
        private readonly string m_uniqId = new Random().Next(100000000, 999999999).ToString();

        public RecommendAPI()
        {
            m_cookieService = App.ServiceProvider.GetRequiredService<CookieService>();
        }

        /// <summary>
        /// App 端首页推荐流（app.bilibili.com/x/v2/feed/index）。
        /// 用 idx 游标翻页：<paramref name="idx"/> 为 "0" 时 pull=true，从推荐池最前面取最新一批。
        /// </summary>
        public ApiModel Recommend(string idx = "0")
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://app.bilibili.com/x/v2/feed/index",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&flush=0&idx={idx}&login_event=2&network=wifi&open_event=&pull={(idx == "0").ToString().ToLower()}&qn=32&style=2",
                headers = ApiHelper.GetAuroraHeaders()
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        /// <summary>
        /// 网页版首页「为你推荐」（x/web-interface/wbi/index/top/feed/rcmd，需要 wbi 签名）。
        /// 与 <see cref="Recommend"/> 是两套**不同的服务端模型**：这个走 web 参数，
        /// 官网前端靠下面这几个参数接力翻页：
        ///   · fresh_idx / fresh_idx_1h —— 当前翻页号，从 1 开始，每次成功请求后 +1；
        ///   · brush —— "刷子"，刷新时 +1（翻页不动它），用来区分"同一轮浏览"与"换了一批"；
        ///   · fetch_row —— 已经抓取到第几行（1 + 累计行数），让服务端知道读到哪了；
        ///   · last_showlist —— 上一屏已经展示过的 av_{aid}，服务端据此避开重复。
        /// 游标状态本身由调用方（RecommendPageViewModel）持有，这里只负责拼参数与签名。
        /// </summary>
        /// <param name="freshIdx">当前翻页号</param>
        /// <param name="brush">刷新次数</param>
        /// <param name="fetchRow">已抓取到第几行</param>
        /// <param name="lastShowList">上一屏展示过的 aid，形如 av_123,av_456；没有就传空</param>
        public async Task<ApiModel> WebRecommend(int freshIdx, int brush, int fetchRow, string lastShowList)
        {
            var parameter = $"fresh_type=4&ps={WebPageSize}" +
                            $"&fresh_idx={freshIdx}&fresh_idx_1h={freshIdx}" +
                            $"&brush={brush}&fetch_row={fetchRow}" +
                            // 官方网页端现行版本号，缺了它服务端会按老版首页的推荐池下发
                            "&feed_version=V8&homepage_ver=1" +
                            $"&y_num={WebYNum}&last_y_num={WebYNum + 1}" +
                            $"&uniq_id={m_uniqId}&web_location=1430650";

            if (!string.IsNullOrEmpty(lastShowList))
            {
                parameter += $"&last_showlist={lastShowList}";
            }

            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                need_cookie = true,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/web-interface/wbi/index/top/feed/rcmd",
                parameter = parameter
            };
            api.parameter = await ApiHelper.GetWbiSign(api.parameter);
            return api;
        }

        /// <summary>
        /// 网页版「内容不感兴趣 / 不想看此UP主」。
        /// 官方首页这两个操作走**同一个接口**，用 <c>reason_id</c> 区分（见 <see cref="WebDislikeReasonNotInterested"/>
        /// / <see cref="WebDislikeReasonNotThisUp"/>），成功后服务端会降低同类内容的推荐权重。
        ///
        /// ★ 业务字段全在 form body，query 里只放客户端元信息并参与 wbi 签名 —— 照抓包原样。
        /// ★ 三个必填业务字段来自卡片本身: <paramref name="aid"/>（item.id）、
        ///   <paramref name="upMid"/>（item.owner.mid）、<paramref name="trackId"/>（item.track_id）。
        /// </summary>
        /// <param name="gotoType">卡片类型，视频为 av</param>
        /// <param name="aid">稿件 aid（网页版放在 item.id 里）</param>
        /// <param name="upMid">UP 主 mid</param>
        /// <param name="trackId">卡片自带的 track_id</param>
        /// <param name="reasonId">1 = 内容不感兴趣，4 = 不想看此UP主</param>
        public async Task<ApiModel> WebDislike(string gotoType, string aid, string upMid, string trackId, int reasonId)
        {
            var csrf = m_cookieService.GetCSRFToken();

            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                need_cookie = true,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/web-interface/feedback/dislike",
                parameter = WebClientMetaQuery,
                body = "app_id=100&platform=5&from_spmid=&spmid=333.1007.0.0" +
                       $"&goto={gotoType}&id={aid}&mid={upMid}" +
                       $"&track_id={Uri.EscapeDataString(trackId ?? "")}" +
                       $"&feedback_page=1&reason_id={reasonId}&csrf={csrf}"
            };
            api.parameter = await ApiHelper.GetWbiSign(api.parameter);
            return api;
        }

        public ApiModel Dislike(RecommondFeedbackParams feedbackParams)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://app.biliapi.net/x/feed/dislike",
                parameter = ApiHelper.MustParameter(AppKey, true) 
                            + $"&goto={feedbackParams.GoTo}&id={feedbackParams.Id}&mid={feedbackParams.Mid}&reason_id={feedbackParams.ReasonId}&rid={feedbackParams.Rid}&tag_id={feedbackParams.TagId}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }
        public ApiModel Feedback(RecommondFeedbackParams feedbackParams)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://app.biliapi.net/x/feed/dislike",
                parameter = ApiHelper.MustParameter(AppKey, true) + 
                            $"&goto={feedbackParams.GoTo}&id={feedbackParams.Id}&mid={feedbackParams.Mid}&feedback_id={feedbackParams.ReasonId}&rid={feedbackParams.Rid}&tag_id={feedbackParams.TagId}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }
    }
}
