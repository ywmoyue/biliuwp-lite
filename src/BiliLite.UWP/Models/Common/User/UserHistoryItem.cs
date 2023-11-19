using Newtonsoft.Json;

namespace BiliLite.Models.Common.User
{
    public class UserHistoryItem
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("long_title")]
        public string LongTitle { get; set; }

        [JsonProperty("cover")]
        public string Cover { get; set; }

        [JsonProperty("covers")]
        public object Covers { get; set; }

        [JsonProperty("uri")]
        public string Uri { get; set; }

        [JsonProperty("history")]
        public UserHistoryItemHistory History { get; set; }

        [JsonProperty("videos")]
        public long Videos { get; set; }

        [JsonProperty("author_name")]
        public string AuthorName { get; set; }

        [JsonProperty("author_face")]
        public string AuthorFace { get; set; }

        [JsonProperty("author_mid")]
        public long AuthorMid { get; set; }

        [JsonProperty("view_at")]
        public long ViewAt { get; set; }

        [JsonProperty("progress")]
        public long Progress { get; set; }

        [JsonProperty("badge")]
        public string Badge { get; set; }

        [JsonProperty("show_title")]
        public string ShowTitle { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }

        [JsonProperty("current")]
        public string Current { get; set; }

        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("new_desc")]
        public string NewDesc { get; set; }

        [JsonProperty("is_finish")]
        public long IsFinish { get; set; }

        [JsonProperty("is_fav")]
        public long IsFav { get; set; }

        [JsonProperty("kid")]
        public long Kid { get; set; }

        [JsonProperty("tag_name")]
        public string TagName { get; set; }

        public bool ShowTag => TagName.Length > 0;

        [JsonProperty("live_status")]
        public long LiveStatus { get; set; }
    }
}
