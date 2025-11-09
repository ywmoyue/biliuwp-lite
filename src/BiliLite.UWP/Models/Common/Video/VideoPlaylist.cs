using System.Collections.Generic;

namespace BiliLite.Models.Common.Video
{
    public class VideoPlaylist
    {
        public int Index { get; set; }

        public List<VideoPlaylistItem> Playlist { get; set; }

        public string Title { get; set; }

        public bool IsOnlineMediaList { get; set; }

        public string MediaListId { get; set; }

        public int MediaListType { get; set; } = 1;

        public string Info { get; set; }
    }
}