using System.Collections.Generic;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Live
{
    public class LiveRoomWebUrlPlayurlItemModel
    {
        [JsonProperty("g_qn_desc")]
        public List<LiveRoomWebUrlQualityDescriptionItemModel> GQnDesc { get; set; }

        public List<LiveRoomWebUrlStreamItemModel> Stream { get; set; }
    }
}