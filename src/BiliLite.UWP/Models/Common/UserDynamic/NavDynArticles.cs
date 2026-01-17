using Newtonsoft.Json;
using System.Collections.Generic;

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

    public class ModuleDynArticles
    {
        [JsonProperty("has_more")]
        public bool HasMore { get; set; }

        public List<ModuleDynArticle> Items { get; set; }

        public string Offset { get; set; }

        [JsonProperty("update_baseline")]
        public string UpdateBaseline { get; set; }
    }
}
