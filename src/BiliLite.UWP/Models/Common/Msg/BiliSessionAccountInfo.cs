using Newtonsoft.Json;

namespace BiliLite.Models.Common.Msg;

/// <summary>
/// 会话信息对象
/// </summary>
public class BiliSessionAccountInfo
{
    /// <summary>
    /// 会话名称
    /// </summary>
    [JsonProperty("name")]
    public string Name { get; set; }

    /// <summary>
    /// 会话头像
    /// </summary>
    [JsonProperty("pic_url")]
    public string PicUrl { get; set; }
}