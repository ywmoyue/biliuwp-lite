using Newtonsoft.Json;

namespace BiliLite.Models.Common.Comment;

public class SendCommentResult
{
    [JsonProperty("rpid_str")]
    public string RpidStr { get; set; }

    public long Rpid { get; set; }
}