using System.Collections.Generic;
using BiliLite.Models.Download;

namespace BiliLite.Models.Common.Download
{
    public class DownloadedSubItem
    {
        public string AVID { get; set; }

        public string CID { get; set; }

        public string EpisodeID { get; set; }

        public string Title { get; set; }

        public bool IsDash { get; set; }

        public int QualityID { get; set; }

        public string QualityName { get; set; }

        public List<string> Paths { get; set; }

        public string DanmakuPath { get; set; }

        public int Index { get; set; }

        public string Path { get; set; }

        public List<DownloadSubtitleInfo> SubtitlePath { get; set; }
    }
}