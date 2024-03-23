﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Download;
using BiliLite.Models.Download;
using BiliLite.Modules;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using System.Threading;
using Windows.Networking.BackgroundTransfer;
using AutoMapper;
using BiliLite.Extensions;

namespace BiliLite.ViewModels.Download
{
    public class DownloadPageViewModel : BaseViewModel
    {
        #region Fields

        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private IDictionary<string, CancellationTokenSource> m_loadDownloadingCts;
        private List<DownloadOperation> m_downloadOperations;
        private List<Task> m_handelList;
        private const string DOWNLOAD_FOLDER_NAME = "哔哩哔哩下载";
        private const string OLD_DOWNLOAD_FOLDER_NAME = "BiliBiliDownload";
        private readonly IMapper m_mapper;

        #endregion

        #region Constructors


        public DownloadPageViewModel(IMapper mapper)
        {
            m_mapper = mapper;
            DownloadedViewModels = new ObservableCollection<DownloadedItem>();
            Downloadings = new ObservableCollection<DownloadingItemViewModel>();
            Downloadeds = new List<DownloadedItem>();

            RefreshDownloadedCommand = new RelayCommand(RefreshDownloaded);
            PauseItemCommand = new RelayCommand<DownloadingSubItemViewModel>(PauseItem);
            ResumeItemCommand = new RelayCommand<DownloadingSubItemViewModel>(ResumeItem);
            DeleteItemCommand = new RelayCommand<DownloadingItemViewModel>(DeleteItem);
            PauseCommand = new RelayCommand(PauseAll);
            StartCommand = new RelayCommand(StartAll);
            DeleteCommand = new RelayCommand(DeleteAll);
        }

        #endregion

        #region Properties

        [DoNotNotify]
        public ICommand PauseCommand { get; private set; }

        [DoNotNotify]
        public ICommand StartCommand { get; private set; }

        [DoNotNotify]
        public ICommand DeleteCommand { get; private set; }

        [DoNotNotify]
        public ICommand DeleteItemCommand { get; private set; }

        [DoNotNotify]
        public ICommand PauseItemCommand { get; private set; }

        [DoNotNotify]
        public ICommand ResumeItemCommand { get; private set; }

        [DoNotNotify]
        public ICommand RefreshDownloadedCommand { get; private set; }

        public ObservableCollection<DownloadingItemViewModel> Downloadings { get; set; }

        public ObservableCollection<DownloadedItem> DownloadedViewModels { get; set; }

        [DoNotNotify]
        public List<DownloadedItem> Downloadeds { get; set; }

        public bool LoadingDownloaded { get; set; } = true;

        public double DiskTotal { get; set; }

        public double DiskUse { get; set; }

        public double DiskFree { get; set; }

        public int TotalDownloadedCount { get; set; }

        public int LoadedDownloadedCount { get; set; }

        [DependsOn(nameof(TotalDownloadedCount), nameof(LoadedDownloadedCount))]
        public int LoadingDownloadedPercent => (int)((LoadedDownloadedCount * 1f / TotalDownloadedCount * 1f) * 100);

        #endregion

        #region Events

        #endregion

        #region Private Methods


        /// <summary>
        /// 下载目录
        /// </summary>
        /// <returns></returns>
        private static async Task<StorageFolder> GetDownloadFolder()
        {
            var path = SettingService.GetValue(SettingConstants.Download.DOWNLOAD_PATH, SettingConstants.Download.DEFAULT_PATH);
            if (path != SettingConstants.Download.DEFAULT_PATH) return await StorageFolder.GetFolderFromPathAsync(path);
            var folder = KnownFolders.VideosLibrary;
            return await folder.CreateFolderAsync(DOWNLOAD_FOLDER_NAME, CreationCollisionOption.OpenIfExists);

        }

        /// <summary>
        /// 旧版下载目录
        /// </summary>
        /// <returns></returns>
        private static async Task<StorageFolder> GetDownloadOldFolder()
        {
            var path = SettingService.GetValue(SettingConstants.Download.OLD_DOWNLOAD_PATH, SettingConstants.Download.DEFAULT_OLD_PATH);
            if (path != SettingConstants.Download.DEFAULT_OLD_PATH)
                return await StorageFolder.GetFolderFromPathAsync(path);
            var folder = KnownFolders.VideosLibrary;
            return await folder.CreateFolderAsync(OLD_DOWNLOAD_FOLDER_NAME, CreationCollisionOption.OpenIfExists);

        }

        private async Task Handel(DownloadOperation downloadOperation, CancellationTokenSource cancellationTokenSource)
        {
            var success = true;
            try
            {

                var progressCallback = new Progress<DownloadOperation>(DownloadProgress);
                if (cancellationTokenSource != null)
                {
                    await downloadOperation.AttachAsync().AsTask(cancellationTokenSource.Token, progressCallback);
                }
                else
                {
                    await downloadOperation.AttachAsync().AsTask(progressCallback);
                }

                //var ls = list_Downing.ItemsSource as ObservableCollection<DisplayModel>;
                RefreshDownloaded();

            }
            catch (TaskCanceledException)
            {
                success = false;
            }
            catch (Exception ex)
            {
                success = false;
                if (ex.Message.Contains("0x80072EF1") || ex.Message.Contains("0x80070002") || ex.Message.Contains("0x80004004"))
                {
                    return;
                }
                var guid = downloadOperation.Guid.ToString();
                var item = Downloadings.FirstOrDefault(x => x.Items.FirstOrDefault(y => y.GUID == guid) != null);
                await Notify.ShowDialog("下载出现问题", $"失败视频:{item.Title ?? ""} {item.EpisodeTitle ?? ""}\r\n" + ex.Message);
            }
            finally
            {
                RemoveItem(downloadOperation.Guid.ToString(), success);
            }
        }

        private void DownloadProgress(DownloadOperation op)
        {
            try
            {
                if (Downloadings == null || Downloadings.Count == 0)
                {
                    return;
                }
                var guid = op.Guid.ToString();

                var item = Downloadings.FirstOrDefault(x => x.Items.Count(y => y.GUID == guid) != 0);
                var subItem = item?.Items.FirstOrDefault(x => x.GUID == guid);
                if (subItem == null) return;
                subItem.ProgressBytes = op.Progress.BytesReceived;
                subItem.Status = op.Progress.Status;
                subItem.TotalBytes = op.Progress.TotalBytesToReceive;
                subItem.Progress = GetProgress(subItem.ProgressBytes, subItem.TotalBytes);
            }
            catch (Exception ex)
            {
                _logger.Warn("下载进度条更新 未知错误", ex);
            }
        }

        private double GetProgress(ulong progress, ulong total)
        {
            if (total >= progress)
            {
                return ((double)progress / total) * 100;
            }

            return 0;
        }

        private void RemoveItem(string guid, bool success = true)
        {
            try
            {
                if (Downloadings == null || Downloadings.Count == 0)
                {
                    return;
                }

                var item = Downloadings.FirstOrDefault(x => x.Items.Count(y => y.GUID == guid) != 0);
                if (item == null) return;

                if (item.Items.Count > 1)
                {
                    var subItem = item.Items.FirstOrDefault(x => x.GUID == guid);
                    if (subItem != null)
                    {
                        item.Items.Remove(subItem);
                    }
                }
                else
                {
                    if (success && SettingService.GetValue<bool>(SettingConstants.Download.SEND_TOAST, false))
                    {
                        Notify.ShowMessageToast("《" + item.Title + " " + item.EpisodeTitle + "》下载完成");
                    }

                    Downloadings.Remove(item);
                }
            }
            catch (Exception ex)
            {
                _logger.Warn("移除下载项目，未知错误", ex);
            }
        }

        /// <summary>
        /// 暂停下载
        /// </summary>
        /// <param name="item"></param>
        private void PauseItem(DownloadingSubItemViewModel item)
        {
            try
            {
                var downloadOperation = m_downloadOperations.FirstOrDefault(x => x.Guid.ToString().Equals(item.GUID));
                downloadOperation.Pause();
            }
            catch (Exception ex)
            {
                _logger.Warn("暂停下载，未知错误", ex);
            }
        }

        /// <summary>
        /// 开始下载
        /// </summary>
        /// <param name="item"></param>
        private void ResumeItem(DownloadingSubItemViewModel item)
        {
            try
            {
                var downloadOperation = m_downloadOperations.FirstOrDefault(x => x.Guid.ToString().Equals(item.GUID));
                downloadOperation.Resume();
            }
            catch (Exception ex)
            {
                _logger.Warn("继续下载，未知错误", ex);
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="item"></param>
        private async void DeleteItem(DownloadingItemViewModel data)
        {
            try
            {
                if (!await Notify.ShowDialog("取消任务", "确定要取消任务吗?"))
                {
                    return;
                }
                m_loadDownloadingCts[data.EpisodeID].Cancel();
                var folder = await StorageFolder.GetFolderFromPathAsync(data.Path);
                await folder.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
            catch (Exception ex)
            {
                _logger.Warn("删除下载，未知错误", ex);
            }
        }

        private void StartAll()
        {
            if (Downloadings.Count == 0) return;
            foreach (var item in Downloadings)
            {
                foreach (var subItem in item.Items)
                {
                    ResumeItem(subItem);
                }
            }
        }

        private void PauseAll()
        {
            if (Downloadings.Count == 0) return;
            foreach (var item in Downloadings)
            {
                foreach (var subItem in item.Items)
                {
                    PauseItem(subItem);
                }
            }
        }

        private async void DeleteAll()
        {
            if (Downloadings.Count == 0) return;
            if (!await Notify.ShowDialog("取消任务", "确定要取消全部任务吗?"))
            {
                return;
            }
            foreach (var item in Downloadings.ToList())
            {
                try
                {
                    m_loadDownloadingCts[item.EpisodeID].Cancel();
                    var folder = await StorageFolder.GetFolderFromPathAsync(item.Path);
                    await folder.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
                catch (Exception ex)
                {
                    _logger.Warn("取消下载任务，未知错误", ex);
                }
            }
        }

        private void LoadDownloadingAddSubItemFromDownloadOperation(IList<DownloadingSubItemViewModel> subItems, DownloadOperation downloadOperation)
        {
            CancellationTokenSource cancellationTokenSource = null;

            var data = SettingService.GetValue<DownloadGUIDInfo>(downloadOperation.Guid.ToString(), null);
            if (data?.CID == null) return;
            if (m_loadDownloadingCts.TryGetValue(data.CID, out var cts))
            {
                cancellationTokenSource = cts;
            }
            else
            {
                cancellationTokenSource = new CancellationTokenSource();
                m_loadDownloadingCts.Add(data.CID, cancellationTokenSource);
            }

            if (!m_downloadOperations.Contains(downloadOperation))
            {
                m_downloadOperations.Add(downloadOperation);
                m_handelList.Add(Handel(downloadOperation, cancellationTokenSource));
            }

            subItems.Add(new DownloadingSubItemViewModel()
            {
                ProgressBytes = downloadOperation.Progress.BytesReceived,
                TotalBytes = downloadOperation.Progress.TotalBytesToReceive,
                Progress = GetProgress(downloadOperation.Progress.BytesReceived, downloadOperation.Progress.TotalBytesToReceive),
                Status = downloadOperation.Progress.Status,
                Title = data.Title,
                FileName = data.FileName,
                EpisodeTitle = data.EpisodeTitle,
                Path = data.Path,
                CID = data.CID,
                GUID = data.GUID,
                PauseItemCommand = PauseItemCommand,
                ResumeItemCommand = ResumeItemCommand
            });
        }

        private void LoadDownloadingAddDownloadingItem(IGrouping<string, DownloadingSubItemViewModel> subItemGroup)
        {
            var subItemList = new ObservableCollection<DownloadingSubItemViewModel>();
            foreach (var subItem in subItemGroup)
            {
                subItemList.Add(subItem);
            }
            Downloadings.Add(new DownloadingItemViewModel()
            {
                EpisodeID = subItemGroup.Key,
                Items = subItemList,
                Title = subItemGroup.FirstOrDefault().Title,
                EpisodeTitle = subItemGroup.FirstOrDefault().EpisodeTitle,
                Path = subItemGroup.FirstOrDefault().Path,
                DeleteItemCommand = DeleteItemCommand,
            });
        }

        #endregion

        #region Public Methods

        public void RefreshDownloaded()
        {
            if (LoadingDownloaded) return;
            LoadDownloaded();
        }

        public void SearchDownloaded(string keyword)
        {
            var searchResult = Downloadeds.Where(x => x.Title.Contains(keyword)).ToList();
            DownloadedViewModels.Clear();
            DownloadedViewModels.AddRange(searchResult);
        }

        /// <summary>
        /// 读取下载的视频
        /// </summary>
        /// <returns></returns>
        public async void LoadDownloaded()
        {
            LoadingDownloaded = true;
            DownloadedViewModels.Clear();
            Downloadeds.Clear();
            var folder = await GetDownloadFolder();
            await LoadDiskSize(folder);
            var folders = await folder.GetFoldersAsync();
            LoadedDownloadedCount = 0;
            TotalDownloadedCount = folders.Count;
            foreach (var item in folders)
            {
                try
                {
                    //检查是否存在info.json
                    if (!(await item.TryGetItemAsync("info.json") is StorageFile infoFile)) continue;

                    var info = JsonConvert.DeserializeObject<DownloadSaveInfo>(await FileIO.ReadTextAsync(infoFile));
                    //旧版无Cover字段，跳过
                    if (string.IsNullOrEmpty(info.Cover)) continue;
                    var lsEpisodes = new List<DownloadedSubItem>();
                    var downloadedItem = new DownloadedItem()
                    {
                        CoverPath = Path.Combine(item.Path, "cover.jpg"),
                        Epsidoes = new ObservableCollection<DownloadedSubItem>(),
                        ID = info.ID,
                        Title = info.Title,
                        UpdateTime = infoFile.DateCreated.LocalDateTime,
                        IsSeason = info.Type == DownloadType.Season,
                        Path = item.Path
                    };
                    if (await item.TryGetItemAsync("cover.jpg") is StorageFile coverFile)
                    {
                        var bitmapImage = new BitmapImage();
                        var buffer = await FileIO.ReadBufferAsync(coverFile);
                        using IRandomAccessStream stream = new InMemoryRandomAccessStream();
                        await stream.WriteAsync(buffer);
                        stream.Seek(0);
                        bitmapImage.SetSource(stream);
                        downloadedItem.Cover = bitmapImage;
                    }

                    foreach (var episodeItem in await item.GetFoldersAsync())
                    {
                        //检查是否存在info.json
                        if (!(await episodeItem.TryGetItemAsync("info.json") is StorageFile episodeInfoFile)) continue;
                        var files = (await episodeItem.GetFilesAsync()).Where(x => x.FileType == ".blv" || x.FileType == ".mp4" || x.FileType == ".m4s");
                        if (!files.Any())
                        {
                            continue;
                        }
                        var flag = false;
                        foreach (var subfile in files)
                        {
                            var size = (await subfile.GetBasicPropertiesAsync()).Size;
                            if (size != 0) continue;
                            flag = true;
                            break;
                        }
                        files = null;
                        if (flag)
                        {
                            continue;
                        }

                        var episodeInfo = JsonConvert.DeserializeObject<DownloadSaveEpisodeInfo>(await FileIO.ReadTextAsync(episodeInfoFile));
                        var downloadedSubItem = m_mapper.Map<DownloadedSubItem>(episodeInfo);
                        //设置视频
                        foreach (var path in episodeInfo.VideoPath)
                        {
                            downloadedSubItem.Paths.Add(Path.Combine(episodeItem.Path, path));
                        }
                        if (!string.IsNullOrEmpty(episodeInfo.DanmakuPath))
                        {
                            downloadedSubItem.DanmakuPath = Path.Combine(episodeItem.Path, episodeInfo.DanmakuPath);
                        }
                        if (episodeInfo.SubtitlePath != null)
                        {
                            foreach (var subtitle in episodeInfo.SubtitlePath)
                            {
                                downloadedSubItem.SubtitlePath.Add(new DownloadSubtitleInfo()
                                {
                                    Name = subtitle.Name,
                                    Url = Path.Combine(episodeItem.Path, subtitle.Url)
                                });
                            }
                        }

                        lsEpisodes.Add(downloadedSubItem);
                    }
                    //排序
                    foreach (var episode in lsEpisodes.OrderBy(x => x.Index))
                    {
                        downloadedItem.Epsidoes.Add(episode);
                    }

                    Downloadeds.Add(downloadedItem);
                    DownloadedViewModels.Add(downloadedItem);
                    LoadedDownloadedCount++;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    _logger.Warn(ex.Message);
                    continue;
                }
            }

            if (SettingService.GetValue(SettingConstants.Download.LOAD_OLD_DOWNLOAD, false))
            {
                await LoadDownloadedOld();
            }

            LoadingDownloaded = false;
        }


        /// <summary>
        /// 读取旧版下载的视频
        /// </summary>
        /// <returns></returns>
        public async Task LoadDownloadedOld()
        {

            var folder = await GetDownloadOldFolder();

            //var list = new List<DownloadedItem>();
            var folders = await folder.GetFoldersAsync();
            TotalDownloadedCount += folders.Count;
            foreach (var item in folders)
            {
                try
                {
                    //检查是否存在info.json
                    if (!(await item.TryGetItemAsync("info.json") is StorageFile infoFile)) continue;

                    var info = JObject.Parse(await FileIO.ReadTextAsync(infoFile));
                    //新版下载无thumb字段
                    if (!info.ContainsKey("thumb"))
                    {
                        continue;
                    }
                    List<DownloadedSubItem> lsEpisodes = new List<DownloadedSubItem>();
                    DownloadedItem downloadedItem = new DownloadedItem()
                    {
                        CoverPath = Path.Combine(item.Path, "thumb.jpg"),
                        Epsidoes = new ObservableCollection<DownloadedSubItem>(),
                        ID = info["id"].ToString(),
                        Title = info["title"].ToString(),
                        UpdateTime = infoFile.DateCreated.LocalDateTime,
                        IsSeason = info["type"].ToString() != "video",
                        Path = item.Path
                    };
                    var coverFile = await item.TryGetItemAsync("thumb.jpg") as StorageFile;
                    if (coverFile != null)
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.SetSource(await coverFile.GetScaledImageAsThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.VideosView));
                        downloadedItem.Cover = bitmapImage;
                    }

                    foreach (var episodeItem in await item.GetFoldersAsync())
                    {
                        //检查是否存在info.json
                        var episodeInfoFile = await episodeItem.TryGetItemAsync("info.json") as StorageFile;
                        if (episodeInfoFile == null)
                        {
                            continue;
                        }
                        var files = (await episodeItem.GetFilesAsync()).Where(x => x.FileType == ".blv" || x.FileType == ".flv" || x.FileType == ".mp4" || x.FileType == ".m4s");
                        if (files.Count() == 0)
                        {
                            continue;
                        }
                        var flag = false;
                        foreach (var subfile in files)
                        {
                            var size = (await subfile.GetBasicPropertiesAsync()).Size;
                            if (size == 0)
                            {
                                flag = true;
                                break;
                            }
                        }

                        if (flag)
                        {
                            continue;
                        }

                        var episodeInfo = JObject.Parse(await FileIO.ReadTextAsync(episodeInfoFile));
                        var downloadedSubItem = new DownloadedSubItem()
                        {
                            AVID = "",
                            CID = episodeInfo["cid"].ToString(),
                            Index = episodeInfo["index"].ToInt32(),
                            EpisodeID = episodeInfo["epid"]?.ToString() ?? "",
                            IsDash = await episodeItem.TryGetItemAsync("video.m4s") != null,
                            Paths = new List<string>(),
                            Title = episodeInfo["title"].ToString(),
                            SubtitlePath = new List<DownloadSubtitleInfo>(),
                            Path = episodeItem.Path
                        };
                        //设置视频
                        foreach (var file in files)
                        {
                            downloadedSubItem.Paths.Add(file.Path);
                        }

                        files = null;
                        downloadedSubItem.DanmakuPath = Path.Combine(episodeItem.Path, downloadedSubItem.CID + ".xml");


                        lsEpisodes.Add(downloadedSubItem);
                    }
                    //排序
                    foreach (var episode in lsEpisodes.OrderBy(x => x.Index))
                    {
                        downloadedItem.Epsidoes.Add(episode);
                    }

                    Downloadeds.Add(downloadedItem);
                    DownloadedViewModels.Add(downloadedItem);
                    LoadedDownloadedCount++;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    _logger.Warn(ex.Message);
                    continue;
                }
            }
        }

        /// <summary>
        /// 读取磁盘可用空间
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        public async Task LoadDiskSize(StorageFolder folder)
        {
            var properties = await folder.Properties.RetrievePropertiesAsync(new string[] { "System.FreeSpace", "System.Capacity" });
            DiskFree = (ulong)properties["System.FreeSpace"] / 1024d / 1024d / 1024d;
            DiskTotal = (ulong)properties["System.Capacity"] / 1024d / 1024d / 1024d;
            DiskUse = DiskTotal - DiskFree;
        }

        /// <summary>
        /// 读取下载中
        /// </summary>
        public async void LoadDownloading()
        {
            m_loadDownloadingCts = new Dictionary<string, CancellationTokenSource>();
            m_handelList ??= new List<Task>();
            m_downloadOperations ??= new List<DownloadOperation>();
            var subItems = new ObservableCollection<DownloadingSubItemViewModel>();
            Downloadings.Clear();
            var downloadOperations = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(DownloadHelper.group);
            foreach (var downloadOperation in downloadOperations)
            {
                LoadDownloadingAddSubItemFromDownloadOperation(subItems, downloadOperation);
            }
            foreach (var subItemGroup in subItems.GroupBy(x => x.CID))
            {
                LoadDownloadingAddDownloadingItem(subItemGroup);
            }
            await Task.WhenAll(m_handelList);
        }

        public async void UpdateSetting()
        {
            var downList = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(DownloadHelper.group);
            var parallelDownload = SettingService.GetValue<bool>(SettingConstants.Download.PARALLEL_DOWNLOAD, true);
            var allowCostNetwork = SettingService.GetValue<bool>(SettingConstants.Download.ALLOW_COST_NETWORK, false);
            //设置下载模式
            foreach (var item in downList)
            {
                item.TransferGroup.TransferBehavior = parallelDownload ? BackgroundTransferBehavior.Parallel : BackgroundTransferBehavior.Serialized;
                item.CostPolicy = allowCostNetwork ? BackgroundTransferCostPolicy.Always : BackgroundTransferCostPolicy.UnrestrictedOnly;
            }
        }

        #endregion
    }
}
