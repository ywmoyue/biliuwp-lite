using System.Collections.Generic;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Msg;

public class BiliReplyMeResponse
{
    public BiliReplyMeCursor Cursor { get; set; }

    public List<BiliReplyMeData> Items { get; set; }
}

public class BiliReplyMeCursor
{
    public long Id { get; set; }

    public long Time { get; set; }

    [JsonProperty("is_end")]
    public bool IsEnd { get; set; }
}

public class BiliReplyMeData
{
    public long Id { get; set; }

    public BiliReplyMeDataUser User { get; set; }

    public BiliReplyMeItem Item { get; set; }

    [JsonProperty("reply_time")]
    public long ReplyTime { get; set; }
}

public class BiliReplyMeItem
{
    [JsonProperty("subject_id")]
    public long SubjectId { get; set; }

    [JsonProperty("root_id")]
    public long RootId { get; set; }

    [JsonProperty("source_id")]
    public long SourceId { get; set; }

    [JsonProperty("target_id")]
    public long TargetId { get; set; }

    public string Type { get; set; }

    [JsonProperty("business_id")]
    public int BusinessId { get; set; }

    public string Business { get; set; }

    public string Title { get; set; }

    public string Uri { get; set; }

    [JsonProperty("native_uri")]
    public string NativeUri { get; set; }

    [JsonProperty("root_reply_content")]
    public string RootReplyContent { get; set; }

    [JsonProperty("source_content")]
    public string SourceContent { get; set; }

    [JsonProperty("target_reply_content")]
    public string TargetReplyContent { get; set; }

    [JsonProperty("like_state")]
    public int LikeState { get; set; }
}

public class BiliReplyMeDataUser
{
    public long Mid { get; set; }

    public string Nickname { get; set; }

    public string Avatar { get; set; }
}