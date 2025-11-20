using BiliLite.Models.Download;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BiliLite.Models.Databases
{
    public class DownloadedItemDTO
    {
        public bool IsSeason { get; set; }

        [Key]
        public string ID { get; set; }

        public string? CoverPath { get; set; }

        public string? Title { get; set; }

        public DateTime UpdateTime { get; set; }

        public List<DownloadedSubItemDTO> Epsidoes { get; set; }

        public string? Path { get; set; }
    }

    public class DownloadedSubItemDTO
    {
        [Key]
        public string CID { get; set; }

        public string? EpisodeID { get; set; }

        public string? Title { get; set; }

        public bool IsDash { get; set; }

        public int QualityID { get; set; }

        public string? QualityName { get; set; }

        [NotMapped]
        public List<string> Paths
        {
            get => JsonConvert.DeserializeObject<List<string>>(PathList);
            set => PathList = JsonConvert.SerializeObject(value);
        }

        public string? PathList { get; set; }

        public string? DanmakuPath { get; set; }

        public int Index { get; set; }

        public string? FilePath { get; set; }

        [NotMapped]
        public List<DownloadSubtitleInfo> SubtitlePath
        {
            get => JsonConvert.DeserializeObject<List<DownloadSubtitleInfo>>(SubtitlePaths);
            set => SubtitlePaths = JsonConvert.SerializeObject(value);
        }

        public string? SubtitlePaths { get; set; }
    }
}
