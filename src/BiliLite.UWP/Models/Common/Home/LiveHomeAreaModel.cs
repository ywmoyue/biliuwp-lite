using Newtonsoft.Json;

namespace BiliLite.Models.Common.Home
{
    public class LiveHomeAreaModel
    {
        public int Id { get; set; }

        [JsonProperty("area_v2_id")]
        public int AreaV2Id { get; set; }

        [JsonProperty("area_v2_parent_id")]
        public int AreaV2ParentId { get; set; }

        [JsonProperty("tag_type")]
        public int TagType { get; set; }

        public string Title { get; set; }

        public string Pic { get; set; }

        public string Link { get; set; }
    }
}