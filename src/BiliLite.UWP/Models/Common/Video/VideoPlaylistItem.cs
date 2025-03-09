using System;

namespace BiliLite.Models.Common.Video
{
    public class VideoPlaylistItem
    {
        public string Id { get; set; }

        public string Author { get; set; }

        public string Cover { get; set; }

        public string Title { get; set; }

        public TimeSpan? Duration { get; set; }
    }
}