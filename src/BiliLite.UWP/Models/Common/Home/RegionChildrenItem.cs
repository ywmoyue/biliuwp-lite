using Newtonsoft.Json;

namespace BiliLite.Models.Common.Home
{
    public class RegionChildrenItem
    {
        public int Tid { get; set; }

        public int Reid { get; set; }

        public string Name { get; set; }

        public string Logo { get; set; }

        public int Type { get; set; }

        [JsonProperty("_goto")]
        public string Goto { get; set; }

        public string Param { get; set; }
    }
}