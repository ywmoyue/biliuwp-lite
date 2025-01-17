using System.Collections.Generic;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Msg;

public class BiliMessageSessionResponse
{
    // 会话列表
    [JsonProperty("session_list")]
    public List<BiliMessageSession> SessionList { get; set; }

    // 是否有更多会话
    [JsonProperty("has_more")]
    public int HasMore { get; set; }

    // 是否开启了“一键防骚扰”功能
    [JsonProperty("anti_distrub_cleaning")]
    public bool AntiDistrubCleaning { get; set; }

    [JsonProperty("is_address_list_empty")]
    public int IsAddressListEmpty { get; set; }

    // 是否在会话列表中显示用户等级
    [JsonProperty("show_level")]
    public bool ShowLevel { get; set; }
}