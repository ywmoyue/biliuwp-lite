using Newtonsoft.Json;

namespace BiliLite.Models.Common.Live
{
    public class LiveAnchorInfoModel
    {
        [JsonProperty("base_info")]
        public LiveAnchorInfoBaseInfoModel BaseInfo { get; set; }

        [JsonProperty("live_info")]
        public LiveAnchorInfoLiveInfoModel LiveInfo { get; set; }

        [JsonProperty("relation_info")]
        public LiveAnchorInfoRelationInfoModel RelationInfo { get; set; }
    }
}