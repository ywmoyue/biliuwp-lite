using Newtonsoft.Json;

namespace BiliLite.Models.Common.Msg.MsgContent;

public class ShareChatMessageContent : IChatMsgContent
{
    /// <summary>
    /// 分享内容作者。此项不实时更新，在发送私信时设置。
    /// </summary>
    [JsonProperty("author")]
    public string Author { get; set; }

    /// <summary>
    /// 分享内容主标题。比 title 更突出；此项不实时更新，在发送私信时设置。
    /// </summary>
    [JsonProperty("headline")]
    public string Headline { get; set; }

    /// <summary>
    /// 分享内容 ID。
    /// </summary>
    [JsonProperty("id")]
    public long Id { get; set; }

    /// <summary>
    /// 分享内容类型。
    /// 1：小视频（已弃用）
    /// 2：相簿
    /// 3：纯文字
    /// 4：直播（此类型不常用，见分享其他内容消息）
    /// 5：视频
    /// 6：专栏
    /// 7：番剧（id 为 season_id）
    /// 8：音乐
    /// 9：国产动画（id 为 AV 号）
    /// 10：图片
    /// 11：动态
    /// 16：番剧（id 为 epid）
    /// 17：番剧
    /// </summary>
    [JsonProperty("source")]
    public int Source { get; set; }

    /// <summary>
    /// 分享内容类型说明。仅当 source 值为 16 时有此项。
    /// </summary>
    [JsonProperty("source_desc")]
    public string SourceDesc { get; set; }

    /// <summary>
    /// 分享内容封面。此项不实时更新，在发送私信时设置。
    /// </summary>
    [JsonProperty("thumb")]
    public string Thumb { get; set; }

    /// <summary>
    /// 分享内容标题。此项不实时更新，在发送私信时设置。
    /// </summary>
    [JsonProperty("title")]
    public string Title { get; set; }

    /// <summary>
    /// 分享内容 URL。（非必要）
    /// </summary>
    [JsonProperty("url")]
    public string Url { get; set; }

    /// <summary>
    /// 视频 BV 号。当 source 值为 5 时有效。（非必要）
    /// </summary>
    [JsonProperty("bvid")]
    public string Bvid { get; set; }
}