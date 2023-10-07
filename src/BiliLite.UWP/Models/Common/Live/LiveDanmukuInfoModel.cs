using System.Collections.Generic;
using Newtonsoft.Json;

namespace BiliLite.Modules.LiveRoomDetailModels
{
    public class LiveDanmukuInfoModel
    {
        public string Token { get; set; }

        [JsonProperty("host_list")]
        public List<LiveDanmukuHostModel> HostList { get; set; }
    }
}