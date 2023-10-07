using Newtonsoft.Json;

namespace BiliLite.Models.Common.Live
{
    public class LiveAnchorInfoBaseInfoModel
    {
        public string Uname { get; set; }

        public string Face { get; set; }

        public string Gender { get; set; }

        [JsonProperty("official_info")]
        public LiveAnchorInfoOfficialInfoModel OfficialInfo { get; set; }
    }
}