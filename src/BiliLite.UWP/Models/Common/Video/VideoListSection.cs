using System.Collections.Generic;

namespace BiliLite.Models.Common.Video
{
    public class VideoListSection
    {
        public long Id { get; set; }

        public string Title { get; set; }

        public bool Selected { get; set; }

        public List<VideoListItem> Items { get; set; }

        public VideoListItem SelectedItem { get; set; }
    }
}