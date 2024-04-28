using BiliLite.Extensions;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Dynamic
{
    public class DynamicDescModel
    {
        public string Uid { get; set; }

        /// <summary>
        /// 8=视频，512=番剧， 4310=合集
        /// </summary>
        public int Type { get; set; }

        public string Rid { get; set; }

        public long View { get; set; }

        public long Like { get; set; }

        public long Comment { get; set; }

        [JsonProperty("is_liked")]
        public int IsLiked { get; set; }

        [JsonProperty("dynamic_id_str")]
        public string DynamicIdStr { get; set; }

        [JsonProperty("dynamic_id")]
        public string DynamicId { get; set; }

        public int Status { get; set; }

        public long Timestamp { get; set; }

        public string TimeText { get; set; }

        public string DisplayTimeText
        {
            get
            {
                if (!string.IsNullOrEmpty(TimeText))
                {
                    return TimeText;
                }

                var dateTime = Timestamp.TimestampToDatetime();
                return $"更新于 {dateTime.DateTimeToDisplayTimeText()}";
            }
        }
    }
}