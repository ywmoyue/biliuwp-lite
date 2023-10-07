using System.Collections.Generic;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Live
{
    public class LiveRoomWebUrlStreamItemModel
    {
        public List<LiveRoomWebUrlFormatItemModel> Format { get; set; }

        [JsonProperty("protocol_name")]
        public string ProtocolName { get; set; }
    }
}