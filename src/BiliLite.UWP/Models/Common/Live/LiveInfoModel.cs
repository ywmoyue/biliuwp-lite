using Newtonsoft.Json;

namespace BiliLite.Models.Common.Live
{
    public class LiveInfoModel
    {
        [JsonProperty("room_info")]
        public LiveRoomInfoModel RoomInfo { get; set; }

        [JsonProperty("guard_info")]
        public LiveRoomGuardInfoModel GuardInfo { get; set; }

        [JsonProperty("anchor_info")]
        public LiveAnchorInfoModel AnchorInfo { get; set; }
    }
}