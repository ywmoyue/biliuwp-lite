using Newtonsoft.Json;

namespace BiliLite.Models.Common.User
{
    public class HistoryCursor
    {
        public long Max { get; set; }

        [JsonProperty("view_at")]
        public long ViewAt { get; set; }

        public string Business { get; set; }

        public int Ps { get; set; }
    }
}
