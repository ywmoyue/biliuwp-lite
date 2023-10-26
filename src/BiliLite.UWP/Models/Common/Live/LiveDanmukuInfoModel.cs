using System.Collections.Generic;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Live
{
    public class LiveDanmukuInfoModel
    {
        public string Token { get; set; }

        [JsonProperty("host_list")]
        public List<LiveDanmukuHostModel> HostList { get; set; }
    }
}