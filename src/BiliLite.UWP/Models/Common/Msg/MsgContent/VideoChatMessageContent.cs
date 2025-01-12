using Newtonsoft.Json;

namespace BiliLite.Models.Common.Msg.MsgContent;

// msg_type=11
public class VideoChatMessageContent
{
    /// <summary>
    /// 视频标题。接收私信时实时更新此项，若视频失效则为空文本。
    /// </summary>
    [JsonProperty("title")]
    public string Title { get; set; }

    /// <summary>
    /// 视频时长。以秒为单位，接收私信时实时更新此项，若视频失效则为 0。
    /// </summary>
    [JsonProperty("times")]
    public long Times { get; set; }

    /// <summary>
    /// 视频封面。接收私信时实时更新此项，若视频失效则为空文本。
    /// </summary>
    [JsonProperty("cover")]
    public string Cover { get; set; }

    /// <summary>
    /// 视频 AV 号。
    /// </summary>
    [JsonProperty("rid")]
    public long Rid { get; set; }

    /// <summary>
    /// 未知字段。作用尚不明确。
    /// </summary>
    [JsonProperty("type_")]
    public int? Type { get; set; }

    /// <summary>
    /// 视频简介。接收私信时实时更新此项，若视频失效则为空文本。
    /// </summary>
    [JsonProperty("desc")]
    public string Desc { get; set; }

    /// <summary>
    /// 视频 BV 号。
    /// </summary>
    [JsonProperty("bvid")]
    public string Bvid { get; set; }

    /// <summary>
    /// 视频播放量。接收私信时实时更新此项，若视频失效则为 0。
    /// </summary>
    [JsonProperty("view")]
    public long View { get; set; }

    /// <summary>
    /// 视频弹幕数。接收私信时实时更新此项，若视频失效则为 0。
    /// </summary>
    [JsonProperty("danmaku")]
    public long Danmaku { get; set; }

    /// <summary>
    /// 视频发布时间。秒级时间戳，若视频失效则为 0。
    /// </summary>
    [JsonProperty("pub_date")]
    public long PubDate { get; set; }

    /// <summary>
    /// UP 主赠言。无效时为 null。
    /// </summary>
    [JsonProperty("attach_msg")]
    public VideoChatMessageContentAttachMessage AttachMsg { get; set; }
}

public class VideoChatMessageContentAttachMessage
{
    /// <summary>
    /// 赠言 ID。
    /// </summary>
    [JsonProperty("id")]
    public long Id { get; set; }

    /// <summary>
    /// 赠言内容。会自动加上“UP主赠言：”前缀，可能包含私信表情。
    /// </summary>
    [JsonProperty("content")]
    public string Content { get; set; }
}