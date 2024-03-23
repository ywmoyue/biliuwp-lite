using System;
using BiliLite.Modules.User;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.User.UserDetails
{
    public class UserCenterInfoModel
    {
        public long Mid { get; set; }

        public string Name { get; set; }

        public string Sex { get; set; }

        public string Face { get; set; }

        public string Sign { get; set; }

        public int Rank { get; set; }

        public int Level { get; set; }

        [JsonProperty("jointime")]
        public int JoinTime { get; set; }

        public int Moral { get; set; }

        public int Silence { get; set; }

        public string Birthday { get; set; }

        public double Coins { get; set; }

        [JsonProperty("fans_badge")]
        public bool FansBadge { get; set; }

        public UserCenterInfoOfficialModel Official { get; set; }

        public UserCenterInfoVipModel Vip { get; set; }

        public UserCenterInfoPendantModel Pendant { get; set; }

        [JsonProperty("nameplate")]
        public UserCenterInfoNameplateModel NamePlate { get; set; }

        [JsonProperty("is_followed")]
        public bool IsFollowed { get; set; }

        [JsonProperty("top_photo")]
        public string TopPhoto { get; set; }

        [JsonProperty("live_room")]
        public UserCenterInfoLiveRoomModel LiveRoom { get; set; }

        public UserCenterSpaceStatModel Stat { get; set; }

        [Obsolete] 
        public string PendantStr => "";

        [Obsolete] 
        public string Verify => "";
    }
}