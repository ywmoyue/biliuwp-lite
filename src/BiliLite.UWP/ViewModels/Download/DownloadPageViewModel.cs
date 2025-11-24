using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BiliLite.Models.Common.Download;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using PropertyChanged;
using System.Threading;
using Windows.Networking.BackgroundTransfer;
using AutoMapper;
using BiliLite.Models.Common;
using BiliLite.Models.Attributes;

namespace BiliLite.ViewModels.Download
{
    [RegisterSingletonViewModelAttribute]
    public class DownloadPageViewModel : BaseViewModel
    {
        #region Fields

        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private IDictionary<string, CancellationTokenSource> m_loadDownloadingCts;
        private List<DownloadOperation> m_downloadOperations;
        private List<Task> m_handelList;
        private readonly IMapper m_mapper;

        #endregion

        #region Constructors

        public DownloadPageViewModel(IMapper mapper)
        {
            m_mapper = mapper;
            DownloadedViewModels = new ObservableCollection<DownloadedItem>();
            Downloadings = new ObservableCollection<DownloadingItemViewModel>();
            Downloadeds = new List<DownloadedItem>();
        }

        #endregion

        #region Properties

        [DoNotNotify]
        public ICommand PauseCommand { get; set; }

        [DoNotNotify]
        public ICommand StartCommand { get; set; }

        [DoNotNotify]
        public ICommand DeleteCommand { get; set; }

        [DoNotNotify]
        public ICommand DeleteItemCommand { get; set; }

        [DoNotNotify]
        public ICommand RefreshDownloadedCommand { get; set; }

        public ObservableCollection<DownloadingItemViewModel> Downloadings { get; set; }

        public ObservableCollection<DownloadedItem> DownloadedViewModels { get; set; }

        [DoNotNotify]
        public List<DownloadedItem> Downloadeds { get; set; }

        public bool IsSearching { get; set; }

        public bool LoadingDownloaded { get; set; } = true;

        public double DiskTotal { get; set; }

        public double DiskUse { get; set; }

        public double DiskFree { get; set; }

        public int TotalDownloadedCount { get; set; }

        public int LoadedDownloadedCount { get; set; }

        [DependsOn(nameof(TotalDownloadedCount), nameof(LoadedDownloadedCount))]
        public int LoadingDownloadedPercent => (int)((LoadedDownloadedCount * 1f / TotalDownloadedCount * 1f) * 100);

        [DoNotNotify]
        public DownloadedSortMode DownloadedSortMode { get; set; }

        [DoNotNotify]
        public string SearchKeyword { get; set; }

        #endregion
    }
}
