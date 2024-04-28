using System.Collections.Generic;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.UserDynamic
{
    public class NavDynArticles
    {
        [JsonProperty("has_more")]
        public bool HasMore { get; set; }

        public List<NavDynArticle> Items { get; set; }

        public string Offset { get; set; }

        [JsonProperty("update_baseline")]
        public string UpdateBaseline { get; set; }
    }
}
