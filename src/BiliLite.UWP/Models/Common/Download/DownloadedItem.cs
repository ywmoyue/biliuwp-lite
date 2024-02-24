using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media.Imaging;

namespace BiliLite.Models.Common.Download
{
    /// <summary>
    /// 已下载的列表
    /// </summary>
    public class DownloadedItem
    {
        public bool IsSeason { get; set; }

        public string ID { get; set; }

        public string CoverPath { get; set; }

        public BitmapImage Cover { get; set; }

        public string Title { get; set; }

        public DateTime UpdateTime { get; set; }

        public ObservableCollection<DownloadedSubItem> Epsidoes { get; set; }

        public string Path { get; set; }
    }
}