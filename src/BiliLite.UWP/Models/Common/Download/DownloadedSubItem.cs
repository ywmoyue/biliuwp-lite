using System.Collections.Generic;
using System.IO;
using BiliLite.Models.Download;
using System.Linq;

namespace BiliLite.Models.Common.Download
{
    public class DownloadedSubItem
    {
        private string m_filePath;

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

        public string FilePath
        {
            get
            {
                if (Paths != null && Paths.Any())
                {
                    return Path.GetDirectoryName(Paths.FirstOrDefault());
                }

                return m_filePath;
            }
            set => m_filePath = value;
        }

        public List<DownloadSubtitleInfo> SubtitlePath { get; set; }
    }
}