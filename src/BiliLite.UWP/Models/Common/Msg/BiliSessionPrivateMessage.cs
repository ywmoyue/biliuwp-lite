using System.Collections.Generic;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Msg;

/// <summary>
/// 私信主体对象
/// </summary>
public class BiliSessionPrivateMessage
{
    /// <summary>
    /// 发送者mid
    /// </summary>
    [JsonProperty("sender_uid")]
    public long SenderUid { get; set; }

    /// <summary>
    /// 接收者类型
    /// 1：用户
    /// 2：粉丝团
    /// </summary>
    [JsonProperty("receiver_type")]
    public int ReceiverType { get; set; }

    /// <summary>
    /// 接收者id
    /// `receiver_type` 为 `1` 时表示用户 mid，为 `2` 时表示粉丝团 id
    /// </summary>
    [JsonProperty("receiver_id")]
    public long ReceiverId { get; set; }

    /// <summary>
    /// 消息类型
    /// 详见[私信消息类型、内容说明](private_msg_content.md)
    /// </summary>
    [JsonProperty("msg_type")]
    public ChatMsgType MsgType { get; set; }

    /// <summary>
    /// 消息内容
    /// [私信内容对象](private_msg_content.md)**经过 JSON 序列化后的文本**
    /// </summary>
    [JsonProperty("content")]
    public string Content { get; set; }

    /// <summary>
    /// 消息序列号
    /// 按照时间顺序从小到大
    /// </summary>
    [JsonProperty("msg_seqno")]
    public long MsgSeqno { get; set; }

    /// <summary>
    /// 消息发送时间
    /// 秒级时间戳
    /// </summary>
    [JsonProperty("timestamp")]
    public long Timestamp { get; set; }

    /// <summary>
    /// at的成员mid
    /// 在粉丝团时有效；此项为 `null` 或 `[0]` 均表示没有 at 成员
    /// </summary>
    [JsonProperty("at_uids")]
    public List<long> AtUids { get; set; }

    /// <summary>
    /// 消息唯一id
    /// 部分库在解析JSON对象中的大数时存在数值的精度丢失问题，因此在处理私信时可能会出现问题，建议使用修复了这一问题的库（如将大数转换成文本）
    /// </summary>
    [JsonProperty("msg_key")]
    public long MsgKey { get; set; }

    /// <summary>
    /// 消息状态
    /// 0：正常
    /// 1：被撤回（接口仍能返回被撤回的私信内容）
    /// 2：被系统撤回（如：消息被举报；私信将不会显示在前端，B站接口也不会返回被系统撤回的私信的信息）
    /// 50：图片已失效（私信内容为一张提示“图片出现问题”的图片）
    /// </summary>
    [JsonProperty("msg_status")]
    public int MsgStatus { get; set; }

    /// <summary>
    /// 是否为系统撤回
    /// 仅当 `msg_type` 为 `5` 且此项值为 `true` 时有此项；若此项值为 `true`，表示目标消息是被系统撤回的，此时前端将不显示该私信且没有提示
    /// </summary>
    [JsonProperty("sys_cancel")]
    public bool SysCancel { get; set; }

    /// <summary>
    /// 通知代码
    /// 发送通知时使用，以下划线 `_` 分割，第 1 项表示主业务 id，第 2 项表示子业务 id；若这条私信非通知则为空文本；详细信息有待补充
    /// </summary>
    [JsonProperty("notify_code")]
    public string NotifyCode { get; set; }

    /// <summary>
    /// 表情包版本
    /// 为 `0` 或无此项表示旧版表情包，此时 B 站会自动转换成新版表情包，例如 `[doge]` -> `[tv_doge]`；`1` 为新版
    /// </summary>
    [JsonProperty("new_face_version")]
    public int NewFaceVersion { get; set; }

    /// <summary>
    /// 消息来源
    /// 见[消息来源列表](#消息来源列表)
    /// </summary>
    [JsonProperty("msg_source")]
    public int MsgSource { get; set; }
}