using Newtonsoft.Json;

namespace BiliLite.Models.Common.User
{
    public class UserHistoryItemHistory
    {
        [JsonProperty("oid")]
        public long Oid { get; set; }

        [JsonProperty("epid")]
        public long Epid { get; set; }

        [JsonProperty("bvid")]
        public string Bvid { get; set; }

        [JsonProperty("page")]
        public long Page { get; set; }

        [JsonProperty("cid")]
        public long Cid { get; set; }

        [JsonProperty("part")]
        public string Part { get; set; }

        [JsonProperty("business")]
        public string Business { get; set; }

        [JsonProperty("dt")]
        public long Dt { get; set; }
    }
}
