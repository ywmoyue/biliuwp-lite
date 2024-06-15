using System.Collections.Generic;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Video
{
    public class MediaListResources
    {
        [JsonProperty("media_list")]
        public List<MediaListItem> MediaList { get; set; }

        [JsonProperty("has_more")]
        public bool HasMore { get; set; }

        [JsonProperty("total_count")]
        public int TotalCount { get; set; }
    }

    public class MediaListItem
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public string Cover { get; set; }

        public MediaListItemUpper Upper { get; set; }
    }

    public class MediaListItemUpper
    {
        public string Name { get; set; }
    }
}
