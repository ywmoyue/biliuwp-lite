using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.UI.Xaml.Media.Imaging;

namespace BiliLite.ViewModels.Download
{
    /// <summary>
    /// 正在下载列表
    /// </summary>
    public class DownloadingItemViewModel
    {
        public ICommand DeleteItemCommand { get; set; }

        public string EpisodeID { get; set; }

        public string Title { get; set; }

        public string EpisodeTitle { get; set; }

        public string Path { get; set; }

        public BitmapImage Cover { get; set; }

        /// <summary>
        /// 正在下载的分段内容
        /// </summary>
        public ObservableCollection<DownloadingSubItemViewModel> Items { get; set; }
    }
}