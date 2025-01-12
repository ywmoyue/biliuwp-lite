using System.Collections.Generic;
using BiliLite.ViewModels.Messages;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Msg;

public class BiliSessionMessagesResponse
{
    public List<BiliSessionPrivateMessage> Messages { get; set; }

    [JsonProperty("has_more")]
    public bool HasMore { get; set; }

    // 所有消息中最小的序列号（最早）
    // 若无私信则为 18446744073709551615
    [JsonProperty("min_seqno")]
    public long MinSeqno { get; set; }

    // 所有消息中最大的序列号（最晚）
    // 若无私信则为 0
    [JsonProperty("max_seqno")]
    public long MaxSeqno { get; set; }

    // 聊天表情列表
    [JsonProperty("e_infos")]
    public List<SessionEmoteInfo> EInfos { get; set; }
}