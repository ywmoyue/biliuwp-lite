using Newtonsoft.Json;

namespace BiliLite.Models.Common.Home
{
    public class HotTopItemModel
    {
        [JsonProperty("entrance_id")]
        public int EntranceId { get; set; }

        public string Icon { get; set; }

        [JsonProperty("module_id")]
        public string ModuleId { get; set; }

        public string Uri { get; set; }

        public string Title { get; set; }
    }
}