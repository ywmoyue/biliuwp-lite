using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BiliLite.Models.Common.Home
{
    public class LiveHomeItemsItemModel
    {
        [JsonProperty("area_v2_id")]
        public int AreaV2Id { get; set; }

        [JsonProperty("area_v2_parent_id")]
        public int AreaV2ParentId { get; set; }

        [JsonProperty("area_v2_name")]
        public string AreaV2Name { get; set; }

        [JsonProperty("area_v2_parent_name")]
        public string AreaV2ParentName { get; set; }

        public string Title { get; set; }

        public string Cover { get; set; }

        public int Online { get; set; }

        public string Roomid { get; set; }

        public string Uname { get; set; }

        public string Face { get; set; }

        public string Uid { get; set; }

        [JsonProperty("pendant_Info")]
        public JObject PendantInfo { get; set; }

        public LivePendentItemModel Pendent => 
            PendantInfo.ContainsKey("2") ? JsonConvert.DeserializeObject<LivePendentItemModel>(PendantInfo["2"].ToString()) : null;

        public bool ShowPendent => Pendent != null;
    }
}