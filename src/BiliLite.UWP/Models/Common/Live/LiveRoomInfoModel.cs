using Newtonsoft.Json;

namespace BiliLite.Models.Common.Live
{
    public class LiveRoomInfoModel
    {
        public long Uid { get; set; }

        [JsonProperty("room_id")]
        public int RoomId { get; set; }

        [JsonProperty("short_id")]
        public int ShortId { get; set; }

        public string Title { get; set; }

        public string Cover { get; set; }

        public string Tags { get; set; }

        public string Background { get; set; }

        public string Description { get; set; }

        public int Online { get; set; }

        [JsonProperty("live_status")]
        public int LiveStatus { get; set; }

        [JsonProperty("live_start_time")]
        public long LiveStartTime { get; set; }

        [JsonProperty("live_screen_type")]
        public int LiveScreenType { get; set; }

        [JsonProperty("lock_status")]
        public int LockStatus { get; set; }

        [JsonProperty("lock_time")]
        public int LockTime { get; set; }

        [JsonProperty("hidden_status")]
        public int HiddenStatus { get; set; }

        [JsonProperty("hidden_time")]
        public int HiddenTime { get; set; }

        [JsonProperty("area_id")]
        public int AreaId { get; set; }

        [JsonProperty("area_name")]
        public string AreaName { get; set; }

        [JsonProperty("parent_area_id")]
        public int ParentAreaId { get; set; }

        [JsonProperty("parent_area_name")]
        public string ParentAreaName { get; set; }

        public string Keyframe { get; set; }

        [JsonProperty("special_type")]
        public int SpecialType { get; set; }

        [JsonProperty("up_session")]
        public string UpSession { get; set; }

        [JsonProperty("pk_status")]
        public int PkStatus { get; set; }

        public LiveRoomInfoPendantsModel Pendants { get; set; }

        [JsonProperty("on_voice_join")]
        public int OnVoiceJoin { get; set; }

        [JsonProperty("tv_screen_on")]
        public int TvScreenOn { get; set; }
    }
}