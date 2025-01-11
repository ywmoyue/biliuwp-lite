using BiliLite.ViewModels.Messages;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Msg;

public class BiliMessageSession
{
    /// <summary>
    /// 聊天对象的id
    /// `session_type` 为 `1` 时表示用户 mid，为 `2` 时表示粉丝团 id
    /// </summary>
    [JsonProperty("talker_id")]
    public long TalkerId { get; set; }

    /// <summary>
    /// 聊天对象的类型
    /// 1：用户
    /// 2：粉丝团
    /// </summary>
    [JsonProperty("session_type")]
    public int SessionType { get; set; }

    /// <summary>
    /// 最近一次未读at自己的消息的序列号
    /// 在粉丝团会话中有效，若没有未读的 at 自己的消息则为 `0`
    /// </summary>
    [JsonProperty("at_seqno")]
    public long AtSeqno { get; set; }

    /// <summary>
    /// 置顶该会话的时间
    /// 微秒级时间戳；若未置顶该会话则为 `0`；用于判断是否置顶了会话
    /// </summary>
    [JsonProperty("top_ts")]
    public long TopTs { get; set; }

    /// <summary>
    /// 粉丝团名称
    /// 在粉丝团会话中有效，其他会话中为空字符串
    /// </summary>
    [JsonProperty("group_name")]
    public string GroupName { get; set; }

    /// <summary>
    /// 粉丝团头像
    /// 在粉丝团会话中有效，其他会话中为空字符串
    /// </summary>
    [JsonProperty("group_cover")]
    public string GroupCover { get; set; }

    /// <summary>
    /// 是否关注了对方
    /// 在用户会话中有效，系统会话中为 `1`, 其他会话中为 `0`
    /// </summary>
    [JsonProperty("is_follow")]
    public int IsFollow { get; set; }

    /// <summary>
    /// 是否对会话设置了免打扰
    /// </summary>
    [JsonProperty("is_dnd")]
    public int IsDnd { get; set; }

    /// <summary>
    /// 最近一次已读的消息序列号
    /// 用于快速跳转到首条未读的消息
    /// </summary>
    [JsonProperty("ack_seqno")]
    public long AckSeqno { get; set; }

    /// <summary>
    /// 最近一次已读时间
    /// 微秒级时间戳
    /// </summary>
    [JsonProperty("ack_ts")]
    public long AckTs { get; set; }

    /// <summary>
    /// 会话时间
    /// 微秒级时间戳
    /// </summary>
    [JsonProperty("session_ts")]
    public long SessionTs { get; set; }

    /// <summary>
    /// 未读消息数
    /// </summary>
    [JsonProperty("unread_count")]
    public int UnreadCount { get; set; }

    /// <summary>
    /// 最近的一条消息
    /// 详见[私信主体对象](#私信主体对象)
    /// </summary>
    [JsonProperty("last_msg")]
    public BiliSessionPrivateMessage LastMsg { get; set; }

    /// <summary>
    /// 粉丝团类型
    /// 在粉丝团时有效
    /// 0：应援团
    /// 2：官方群（如：ID 为 10 的粉丝团）
    /// </summary>
    [JsonProperty("group_type")]
    public int GroupType { get; set; }

    /// <summary>
    /// 会话是否可被折叠入未关注人消息
    /// 在用户会话中有效
    /// </summary>
    [JsonProperty("can_fold")]
    public int CanFold { get; set; }

    /// <summary>
    /// 会话状态
    /// 详细信息有待补充
    /// </summary>
    [JsonProperty("status")]
    public int Status { get; set; }

    /// <summary>
    /// 最近一条消息的序列号
    /// </summary>
    [JsonProperty("max_seqno")]
    public long MaxSeqno { get; set; }

    /// <summary>
    /// 是否有新推送的消息
    /// </summary>
    [JsonProperty("new_push_msg")]
    public int NewPushMsg { get; set; }

    /// <summary>
    /// 推送设置
    /// 0：接收推送
    /// 1：不接收推送
    /// 2：（？）
    /// </summary>
    [JsonProperty("setting")]
    public int Setting { get; set; }

    /// <summary>
    /// 自己是否为对方的骑士（？）
    /// 在用户会话中有效
    /// 0：否
    /// 2：是（？）
    /// </summary>
    [JsonProperty("is_guardian")]
    public int IsGuardian { get; set; }

    /// <summary>
    /// 会话是否被拦截
    /// </summary>
    [JsonProperty("is_intercept")]
    public int IsIntercept { get; set; }

    /// <summary>
    /// 是否信任此会话
    /// 若为 `1`，则表示此会话之前被拦截过，但用户选择信任本会话
    /// </summary>
    [JsonProperty("is_trust")]
    public int IsTrust { get; set; }

    /// <summary>
    /// 系统会话类型
    /// 0：非系统会话
    /// 1：主播小助手
    /// 5：系统通知（？）
    /// 7：UP主小助手
    /// 8：客服消息
    /// 9：支付小助手
    /// </summary>
    [JsonProperty("system_msg_type")]
    public int SystemMsgType { get; set; }

    /// <summary>
    /// 会话信息
    /// 仅在系统会话中出现
    /// </summary>
    [JsonProperty("account_info")]
    public BiliSessionAccountInfo AccountInfo { get; set; }

    /// <summary>
    /// 用户是否正在直播
    /// 在用户会话中有效，其他会话中为 `0`
    /// </summary>
    [JsonProperty("live_status")]
    public int LiveStatus { get; set; }

    /// <summary>
    /// 未读通知消息数
    /// </summary>
    [JsonProperty("biz_msg_unread_count")]
    public int BizMsgUnreadCount { get; set; }

    /// <summary>
    /// （？）
    /// 作用尚不明确
    /// </summary>
    [JsonProperty("user_label")]
    public object UserLabel { get; set; }
}