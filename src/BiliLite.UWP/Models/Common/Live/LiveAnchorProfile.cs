using System.Collections.Generic;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Live
{
    public class LiveAnchorProfile
    {
        public long Uid { get; set; }

        public string Uname { get; set; }

        public string Face { get; set; }


        [JsonProperty("verify_type")]
        public int VerifyType { get; set; }

        public string Verify =>
            VerifyType switch
            {
                0 => Constants.App.VERIFY_PERSONAL_IMAGE,
                1 => Constants.App.VERIFY_OGANIZATION_IMAGE,
                _ => Constants.App.TRANSPARENT_IMAGE
            };

        public string Desc { get; set; }

        public int Level { get; set; }

        [JsonProperty("level_color")]
        public int LevelColor { get; set; }

        [JsonProperty("main_vip")]
        public int MainVip { get; set; }

        [JsonProperty("uname_color")]
        public int UnameColor { get; set; }

        [JsonProperty("room_id")]
        public int RoomId { get; set; }

        [JsonProperty("area_name")]
        public string AreaName { get; set; }

        public string Pendant { get; set; }

        [JsonProperty("pendant_from")]
        public int PendantFrom { get; set; }

        [JsonProperty("glory_info")]
        public List<LiveAnchorProfileGloryInfo> GloryInfo { get; set; }

        [JsonProperty("pk_info")]
        public List<LiveAnchorProfilePkInfo> PkInfo { get; set; }

        [JsonProperty("season_info_url")]
        public string SeasonInfoUrl { get; set; }

        [JsonProperty("follow_num")]
        public int FollowNum { get; set; }

        [JsonProperty("is_fans")]
        public bool IsFans { get; set; }

        [JsonProperty("relation_status")]
        public int RelationStatus { get; set; }
    }
}