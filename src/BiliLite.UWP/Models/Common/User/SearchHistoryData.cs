using System.Collections.Generic;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.User
{
    public class SearchHistoryData
    {
        [JsonProperty("has_more")]
        public bool HasMore { get; set; }

        public List<UserHistoryItem> List { get; set; }
    }
}
