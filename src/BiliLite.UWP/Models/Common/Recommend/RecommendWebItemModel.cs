using Newtonsoft.Json;

namespace BiliLite.Models.Common.Recommend
{
    /// <summary>
    /// 网页版首页推荐流(x/web-interface/wbi/index/top/feed/rcmd)的 <c>data.item[]</c> 一条。
    ///
    /// ★ 与 App 端那套(<see cref="RecommendItemModel"/>)字段名完全不同: 这里是 id / pic /
    ///   owner{mid,name} / stat{view,danmaku}, App 端是 param / cover / args{up_id,up_name} /
    ///   cover_left_text_*。所以单独建一个 DTO, 由 <c>MapperExtensions</c> 里的
    ///   <c>CreateMap&lt;RecommendWebItemModel, RecommendItemModel&gt;</c> 统一转换 ——
    ///   格式化逻辑(播放量/时长/封面补协议)都写在映射配置里, 不散落在业务代码。
    /// </summary>
    public class RecommendWebItemModel
    {
        /// <summary>
        /// 卡片类型。只处理 "av"(视频): 直播 / 横幅 / 广告卡在推荐页没有对应的渲染分支。
        /// </summary>
        [JsonProperty("goto")]
        public string Goto { get; set; }

        /// <summary>
        /// 稿件 aid。★ 网页版把它放在 <c>id</c> 里(不是 <c>aid</c>)，翻页游标的 last_showlist 也要它。
        /// </summary>
        [JsonProperty("id")]
        public long Aid { get; set; }

        [JsonProperty("bvid")]
        public string Bvid { get; set; }

        /// <summary>视频地址。偶尔为空，映射时会按 bvid 拼一个。</summary>
        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("pic")]
        public string Pic { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>时长（秒）。</summary>
        [JsonProperty("duration")]
        public long Duration { get; set; }

        /// <summary>
        /// 本条推荐流的跟踪标识，形如 <c>web_pegasus_0.router-web-pegasus-…</c>。
        /// ★ 「不感兴趣 / 不想看此UP主」接口(x/web-interface/feedback/dislike)必须原样带回去，
        ///   抓包实测三个必填业务字段就是 id(aid) / mid(UP) / track_id。
        /// </summary>
        [JsonProperty("track_id")]
        public string TrackId { get; set; }

        /// <summary>服务端是否给"不感兴趣"入口（1 = 给）。目前只做记录，不影响渲染。</summary>
        [JsonProperty("dislike_switch")]
        public int DislikeSwitch { get; set; }

        /// <summary>推荐理由。reason_type: 0 无 / 1 已关注 / 3 高点赞量(此时才有 content)。</summary>
        [JsonProperty("rcmd_reason")]
        public RecommendWebItemRcmdReasonModel RcmdReason { get; set; }

        [JsonProperty("owner", NullValueHandling = NullValueHandling.Ignore)]
        public RecommendWebItemOwnerModel Owner { get; set; } = new RecommendWebItemOwnerModel();

        [JsonProperty("stat", NullValueHandling = NullValueHandling.Ignore)]
        public RecommendWebItemStatModel Stat { get; set; } = new RecommendWebItemStatModel();
    }

    public class RecommendWebItemRcmdReasonModel
    {
        [JsonProperty("reason_type")]
        public int ReasonType { get; set; }

        /// <summary>推荐理由文案，如"11万点赞"；reason_type 为 3 时才有。</summary>
        [JsonProperty("content")]
        public string Content { get; set; }
    }

    public class RecommendWebItemOwnerModel
    {
        [JsonProperty("mid")]
        public long Mid { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class RecommendWebItemStatModel
    {
        [JsonProperty("view")]
        public long View { get; set; }

        [JsonProperty("danmaku")]
        public long Danmaku { get; set; }
    }
}
