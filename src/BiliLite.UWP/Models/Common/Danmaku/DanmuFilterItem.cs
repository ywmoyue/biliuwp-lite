using Newtonsoft.Json;

namespace BiliLite.Models.Common.Danmaku
{
    public class DanmuFilterItem
    {
        public int Id { get; set; }

        [JsonIgnore]
        public long Mid { get; set; }

        public int Type { get; set; }

        public string Filter { get; set; }

        [JsonIgnore]
        public string Comment { get; set; }

        [JsonIgnore]
        public long Ctime { get; set; }


        [JsonIgnore]
        public long Mtime { get; set; }

        public bool Opened { get; set; }
    }
}