namespace BiliLite.Models.Common.Msg;

/// <summary>
/// 消息来源列表
/// </summary>
public enum BiliSessionMessageSource
{
    /// <summary>
    /// 未知来源
    /// 在以前发送的部分私信的来源代码
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// iOS
    /// </summary>
    iOS = 1,

    /// <summary>
    /// Android
    /// </summary>
    Android = 2,

    /// <summary>
    /// H5
    /// </summary>
    H5 = 3,

    /// <summary>
    /// PC客户端
    /// </summary>
    PC = 4,

    /// <summary>
    /// 官方推送消息
    /// 包括：官方向大多数用户自动发送的私信（如：UP主小助手的推广）等
    /// </summary>
    OfficialPush = 5,

    /// <summary>
    /// 推送/通知消息
    /// 包括：特别关注时稿件的自动推送、因成为契约者而自动发送的私信、包月充电回馈私信、官方发送的特定于自己的消息（如：UP主小助手的稿件审核状态通知）等
    /// </summary>
    PushNotification = 6,

    /// <summary>
    /// Web
    /// </summary>
    Web = 7,

    /// <summary>
    /// 自动回复 - 被关注回复
    /// B站前端会显示“此条消息为自动回复”
    /// </summary>
    AutoReplyFollow = 8,

    /// <summary>
    /// 自动回复 - 收到消息回复
    /// B站前端会显示“此条消息为自动回复”
    /// </summary>
    AutoReplyMessage = 9,

    /// <summary>
    /// 自动回复 - 关键词回复
    /// B站前端会显示“此条消息为自动回复”
    /// </summary>
    AutoReplyKeyword = 10,

    /// <summary>
    /// 自动回复 - 大航海上船回复
    /// B站前端会显示“此条消息为自动回复”
    /// </summary>
    AutoReplyVoyage = 11,

    /// <summary>
    /// 自动推送 - UP 主赠言
    /// 在以前稿件的自动推送与其附带的 UP 主赠言是 2 条不同的私信（其中 UP 主赠言的消息来源代码为 12），现在 UP 主赠言已被合并成为稿件自动推送消息的一部分（`attach_msg`）
    /// </summary>
    AutoPushUP = 12,

    /// <summary>
    /// 粉丝团系统提示
    /// 如：粉丝团中的提示信息“欢迎xxx入群”
    /// </summary>
    FanGroupSystem = 13,

    /// <summary>
    /// 系统
    /// 目前仅在 `msg_type` 为 `51` 时使用该代码
    /// </summary>
    System = 16,

    /// <summary>
    /// 互相关注
    /// 互相关注时自动发送的私信“我们已互相关注，开始聊天吧~”
    /// </summary>
    MutualFollow = 17,

    /// <summary>
    /// 系统提示
    /// 目前仅在 `msg_type` 为 `18` 时使用该代码，如：“对方主动回复或关注你前，最多发送1条消息”
    /// </summary>
    SystemHint = 18,

    /// <summary>
    /// AI
    /// 如：给[搜索AI助手测试版](https://space.bilibili.com/1400565964/)发送私信时对方的自动回复
    /// </summary>
    AI = 19
}