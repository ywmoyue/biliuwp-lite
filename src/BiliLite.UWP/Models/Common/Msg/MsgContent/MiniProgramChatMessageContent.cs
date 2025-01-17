using Newtonsoft.Json;

namespace BiliLite.Models.Common.Msg.MsgContent;

public class MiniProgramChatMessageContent : IChatMsgContent
{
    /// <summary>
    /// 小程序图标。
    /// </summary>
    [JsonProperty("avatar")]
    public string Avatar { get; set; }

    /// <summary>
    /// 小程序封面。
    /// </summary>
    [JsonProperty("cover")]
    public string Cover { get; set; }

    /// <summary>
    /// 小程序 ID。
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }

    /// <summary>
    /// 小程序链接。
    /// </summary>
    [JsonProperty("jump_uri")]
    public string JumpUri { get; set; }

    /// <summary>
    /// 标签图标。
    /// </summary>
    [JsonProperty("label_cover")]
    public string LabelCover { get; set; }

    /// <summary>
    /// 标签文字内容。一般为“小程序”。
    /// </summary>
    [JsonProperty("label_name")]
    public string LabelName { get; set; }

    /// <summary>
    /// 小程序名称。
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// 小程序标题。
    /// </summary>
    [JsonProperty("title")]
    public string Title { get; set; }
}