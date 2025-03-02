using System;
using BiliLite.Converters;

namespace BiliLite.Models.Common.Video
{
    public class VideoListItem
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public string Cover { get; set; }

        public TimeSpan? Duration { get; set; }

        public string DurationStr => !Duration.HasValue ? "" : TimeSpanStrFormatConverter.Convert(Duration.Value);
    }
}