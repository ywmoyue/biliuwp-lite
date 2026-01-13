using System.Windows.Input;
using Windows.Networking.BackgroundTransfer;
using BiliLite.ViewModels.Common;
using PropertyChanged;
using BiliLite.Services;

namespace BiliLite.ViewModels.Download
{
    public class DownloadingSubItemViewModel : BaseViewModel
    {
        [DoNotNotify]
        public string GUID { get; set; }

        /// <summary>
        /// 下载状态
        /// </summary>
        public DownloadStatus Status { get; set; }

        /// <summary>
        /// 文件总大小
        /// </summary>
        public ulong TotalBytes { get; set; }

        /// <summary>
        /// 已下载大小
        /// </summary>
        public ulong ProgressBytes { get; set; }

        public double Progress { get; set; } = 0;

        [DoNotNotify]
        public string CID { get; set; }

        [DoNotNotify]
        public string Title { get; set; }

        [DoNotNotify]
        public string FileName { get; set; }

        [DoNotNotify]
        public string EpisodeTitle { get; set; }

        [DoNotNotify]
        public string Path { get; set; }

        [DependsOn(nameof(TotalBytes))]
        public bool ShowPause { get { return Status == DownloadStatus.Running; } }

        [DependsOn(nameof(TotalBytes))]
        public bool ShowStart { get { return Status != DownloadStatus.Running; } }
    }
}
