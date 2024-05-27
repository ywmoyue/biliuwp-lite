using System.Collections.Generic;

namespace BiliLite.Models.Common.Video
{
    public class VideoPlaylist
    {
        public int Index { get; set; }

        public List<VideoPlaylistItem> Playlist { get; set; }

        public string Title { get; set; }
    }
}