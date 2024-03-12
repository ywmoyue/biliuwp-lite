using Newtonsoft.Json;

namespace BiliLite.Models.Common.User.UserDetails
{
    public class UserCenterInfoLiveRoomModel
    {
        public int RoomStatus { get; set; }

        public int LiveStatus { get; set; }

        public string Url { get; set; }

        public string Title { get; set; }

        public string Cover { get; set; }

        public int Online { get; set; }

        [JsonProperty("roomid")]
        public int RoomId { get; set; }

        public int RoundStatus { get; set; }

        [JsonProperty("broadcast_type")]
        public int BroadcastType { get; set; }
    }
}