using BiliLite.ViewModels.Common;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BiliLite.ViewModels.Download
{
    /// <summary>
    /// 一次「添加下载」的批次，右下角悬浮窗按批次逐集显示处理进度
    /// </summary>
    public class DownloadAddBatchViewModel : BaseViewModel
    {
        /// <summary>
        /// 批次标题，一般是下载分组的标题
        /// </summary>
        public string Title { get; set; }

        public ObservableCollection<DownloadEpisodeItemViewModel> Items { get; } = new();

        /// <summary>
        /// 该批次是否已经处理结束
        /// </summary>
        public bool Finished { get; private set; }

        public int Total => Items.Count;

        public int FinishedCount => Items.Count(x => x.State == 2 || x.State == 3 || x.State == 99);

        public int AddedCount => Items.Count(x => x.State == 2);

        public int SkippedCount => Items.Count(x => x.State == 3);

        public int FailedCount => Items.Count(x => x.State == 99);

        public bool HasFailure => FailedCount > 0;

        /// <summary>
        /// 批次右上角的状态文案
        /// </summary>
        public string StatusText
        {
            get
            {
                if (!Finished) return $"{FinishedCount}/{Total}";

                var parts = new List<string> { $"已加入 {AddedCount}" };
                if (SkippedCount > 0) parts.Add($"跳过 {SkippedCount}");
                if (FailedCount > 0) parts.Add($"失败 {FailedCount}");
                return string.Join("，", parts);
            }
        }

        /// <summary>
        /// 单集状态变化后刷新批次计数
        /// </summary>
        public void NotifyProgress()
        {
            OnPropertyChanged(nameof(FinishedCount));
            OnPropertyChanged(nameof(AddedCount));
            OnPropertyChanged(nameof(SkippedCount));
            OnPropertyChanged(nameof(FailedCount));
            OnPropertyChanged(nameof(HasFailure));
            OnPropertyChanged(nameof(StatusText));
        }

        public void MarkFinished()
        {
            Finished = true;
            NotifyProgress();
        }
    }
}
