using System.Collections.Generic;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Home
{
    public class RegionItem
    {
        public int Tid { get; set; }

        public int Reid { get; set; }

        public string Name { get; set; }

        public string Logo { get; set; }

        public string Uri { get; set; }

        public int Type { get; set; }

        [JsonProperty("is_bangumi")]
        public int IsBangumi { get; set; }

        [JsonProperty("_goto")]
        public string Goto { get; set; }

        public List<RegionChildrenItem> Children { get; set; }
    }
}