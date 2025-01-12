using Newtonsoft.Json;
using System.Collections.Generic;

namespace BiliLite.Models.Common.Msg.MsgContent;

// msg_type=10
public class NotificationChatMessageContent : IChatMsgContent
{
    /// <summary>
    /// 通知标题。
    /// </summary>
    [JsonProperty("title")]
    public string Title { get; set; }

    /// <summary>
    /// 通知内容。
    /// </summary>
    [JsonProperty("text")]
    public string Text { get; set; }

    /// <summary>
    /// 按钮1提示文字。若按钮1不存在则为空；若按钮1存在此项也可能为空，此时前端显示文字为“查看详情”。
    /// </summary>
    [JsonProperty("jump_text")]
    public string JumpText { get; set; }

    public bool ShowJumpBtn => !string.IsNullOrEmpty(JumpText);

    /// <summary>
    /// 按钮1跳转链接。若按钮1不存在则为空。
    /// </summary>
    [JsonProperty("jump_uri")]
    public string JumpUri { get; set; }

    /// <summary>
    /// 详细信息。无效时为 null。
    /// </summary>
    [JsonProperty("modules")]
    public List<NotificationChatMessageContentModule> Modules { get; set; }

    /// <summary>
    /// 按钮2提示文字。若按钮2不存在则为空；若按钮2存在此项也可能为空，此时前端显示文字为“查看详情”。
    /// </summary>
    [JsonProperty("jump_text_2")]
    public string JumpText2 { get; set; }

    public bool ShowJumpBtn2 => !string.IsNullOrEmpty(JumpText2);

    /// <summary>
    /// 按钮2跳转链接。若按钮2不存在则为空。
    /// </summary>
    [JsonProperty("jump_uri_2")]
    public string JumpUri2 { get; set; }

    /// <summary>
    /// 按钮3提示文字。若按钮3不存在则为空；若按钮3存在此项也可能为空，此时前端显示文字为“查看详情”。
    /// </summary>
    [JsonProperty("jump_text_3")]
    public string JumpText3 { get; set; }

    public bool ShowJumpBtn3 => !string.IsNullOrEmpty(JumpText3);

    /// <summary>
    /// 按钮3跳转链接。若按钮3不存在则为空。
    /// </summary>
    [JsonProperty("jump_uri_3")]
    public string JumpUri3 { get; set; }

    /// <summary>
    /// 发送者信息。无效时为 null。
    /// </summary>
    [JsonProperty("notifier")]
    public NotificationChatMessageContentNotifier Notifier { get; set; }

    /// <summary>
    /// 按钮1配置。
    /// </summary>
    [JsonProperty("jump_uri_config")]
    public NotificationChatMessageContentJumpUriConfig JumpUriConfig { get; set; }

    /// <summary>
    /// 按钮2配置。
    /// </summary>
    [JsonProperty("jump_uri_2_config")]
    public NotificationChatMessageContentJumpUriConfig JumpUri2Config { get; set; }

    /// <summary>
    /// 按钮3配置。
    /// </summary>
    [JsonProperty("jump_uri_3_config")]
    public NotificationChatMessageContentJumpUriConfig JumpUri3Config { get; set; }

    /// <summary>
    /// 扩展信息。无效时为 null。
    /// </summary>
    [JsonProperty("biz_content")]
    public NotificationChatMessageContentBizContent BizContent { get; set; }
}

public class NotificationChatMessageContentModule
{
    /// <summary>
    /// 标题。
    /// </summary>
    [JsonProperty("title")]
    public string Title { get; set; }

    /// <summary>
    /// 内容。
    /// </summary>
    [JsonProperty("detail")]
    public string Detail { get; set; }
}

public class NotificationChatMessageContentNotifier
{
    /// <summary>
    /// 发送者头像。
    /// </summary>
    [JsonProperty("avatar_url")]
    public string AvatarUrl { get; set; }

    /// <summary>
    /// 发送者名称。
    /// </summary>
    [JsonProperty("nickname")]
    public string Nickname { get; set; }

    /// <summary>
    /// 发送者链接。可为空。
    /// </summary>
    [JsonProperty("jump_url")]
    public string JumpUrl { get; set; }
}

public class NotificationChatMessageContentJumpUriConfig
{
    /// <summary>
    /// 所有设备的跳转链接。若按钮不存在则无此项。
    /// </summary>
    [JsonProperty("all_uri")]
    public string AllUri { get; set; }

    /// <summary>
    /// Android客户端的跳转链接。若按钮不存在则无此项。
    /// </summary>
    [JsonProperty("android_uri")]
    public string AndroidUri { get; set; }

    /// <summary>
    /// iPhone客户端的跳转链接。若按钮不存在则无此项。
    /// </summary>
    [JsonProperty("iphone_uri")]
    public string IphoneUri { get; set; }

    /// <summary>
    /// iPad客户端的跳转链接。若按钮不存在则无此项。
    /// </summary>
    [JsonProperty("ipad_uri")]
    public string IpadUri { get; set; }

    /// <summary>
    /// 网页上的跳转链接。若按钮不存在则无此项。
    /// </summary>
    [JsonProperty("web_uri")]
    public string WebUri { get; set; }

    /// <summary>
    /// 按钮提示文字。若按钮不存在则为空。
    /// </summary>
    [JsonProperty("text")]
    public string Text { get; set; }
}

public class NotificationChatMessageContentBizContent
{
    /// <summary>
    /// 封面 URL。
    /// </summary>
    [JsonProperty("cover")]
    public string Cover { get; set; }

    /// <summary>
    /// 备用封面 URL。
    /// </summary>
    [JsonProperty("backup_cover")]
    public string BackupCover { get; set; }

    /// <summary>
    /// 刷新类型。作用尚不明确。
    /// </summary>
    [JsonProperty("refresh_type")]
    public int? RefreshType { get; set; }

    /// <summary>
    /// 业务类型。作用尚不明确。
    /// </summary>
    [JsonProperty("biz_type")]
    public int? BizType { get; set; }

    /// <summary>
    /// 业务 ID1。作用尚不明确。
    /// </summary>
    [JsonProperty("biz_id1")]
    public string BizId1 { get; set; }

    /// <summary>
    /// 业务 ID2。作用尚不明确。
    /// </summary>
    [JsonProperty("biz_id2")]
    public string BizId2 { get; set; }

    /// <summary>
    /// 业务状态。作用尚不明确。
    /// </summary>
    [JsonProperty("biz_status")]
    public int? BizStatus { get; set; }
}