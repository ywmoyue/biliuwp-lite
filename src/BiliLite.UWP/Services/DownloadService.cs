using MapsterMapper;
using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Download;
using BiliLite.Models.Databases;
using BiliLite.Models.Download;
using BiliLite.Modules;
using BiliLite.ViewModels.Download;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Streams;
using Microsoft.UI.Xaml.Media.Imaging;

namespace BiliLite.Services
{
    public class DownloadService
    {
        #region Fields

        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly DownloadPageViewModel m_downloadPageViewModel;
        private List<DownloadOperation> m_downloadOperations;
        private SettingSqlService m_settingSqlService;
        private IDictionary<string, CancellationTokenSource> m_loadDownloadingCts;
        private List<Task> m_handelList;
        private readonly IMapper m_mapper;
        private readonly BiliLiteDbContext m_biliLiteDbContext;
        private bool m_useDownloadIndex = false;

        #endregion

        #region Constructors

        public DownloadService(DownloadPageViewModel downloadPageViewModel, IMapper mapper, SettingSqlService settingSqlService, BiliLiteDbContext biliLiteDbContext)
        {
            m_downloadPageViewModel = downloadPageViewModel;
            m_mapper = mapper;
            m_settingSqlService = settingSqlService;
            m_biliLiteDbContext = biliLiteDbContext;

            m_downloadPageViewModel.RefreshDownloadedCommand = new RelayCommand(RefreshDownloaded);
            m_downloadPageViewModel.DeleteItemCommand = new RelayCommand<DownloadingItemViewModel>(DeleteItem);
            m_downloadPageViewModel.PauseCommand = new RelayCommand(PauseAll);
            m_downloadPageViewModel.StartCommand = new RelayCommand(StartAll);
            m_downloadPageViewModel.DeleteCommand = new RelayCommand(DeleteAll);

            m_useDownloadIndex = SettingService.GetValue(SettingConstants.Download.USE_DOWNLOAD_INDEX,
                SettingConstants.Download.DEFAULT_USE_DOWNLOAD_INDEX);
        }

        #endregion

        #region Private Methods

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
                var item = m_downloadPageViewModel.Downloadings.FirstOrDefault(x => x.Items.FirstOrDefault(y => y.GUID == guid) != null);
                await NotificationShowExtensions.ShowMessageDialog("下载出现问题", $"失败视频:{item.Title ?? ""} {item.EpisodeTitle ?? ""}\r\n" + ex.Message);
                // TODO: 启用重试策略
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
                if (m_downloadPageViewModel.Downloadings == null || m_downloadPageViewModel.Downloadings.Count == 0)
                {
                    return;
                }
                var guid = op.Guid.ToString();

                var item = m_downloadPageViewModel.Downloadings.FirstOrDefault(x => x.Items.Count(y => y.GUID == guid) != 0);
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
                if (m_downloadPageViewModel.Downloadings == null || m_downloadPageViewModel.Downloadings.Count == 0)
                {
                    return;
                }

                var item = m_downloadPageViewModel.Downloadings.FirstOrDefault(x => x.Items.Count(y => y.GUID == guid) != 0);
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
                        NotificationShowExtensions.ShowMessageToast("《" + item.Title + " " + item.EpisodeTitle + "》下载完成");
                    }

                    m_downloadPageViewModel.Downloadings.Remove(item);
                }
            }
            catch (Exception ex)
            {
                _logger.Warn("移除下载项目，未知错误", ex);
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
                if (!await NotificationShowExtensions.ShowMessageDialog("取消任务", "确定要取消任务吗?"))
                {
                    return;
                }
                m_loadDownloadingCts[data.EpisodeID].Cancel();
                if (Directory.Exists(data.Path))
                {
                    var folder = await StorageFolder.GetFolderFromPathAsync(data.Path);
                    await folder.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }

                m_downloadPageViewModel.Downloadings.Remove(data);
                RemoveDbSubItem(data.EpisodeID);
            }
            catch (Exception ex)
            {
                _logger.Warn("删除下载，未知错误", ex);
            }
        }

        private void StartAll()
        {
            if (m_downloadPageViewModel.Downloadings.Count == 0) return;
            foreach (var item in m_downloadPageViewModel.Downloadings)
            {
                foreach (var subItem in item.Items)
                {
                    ResumeItem(subItem);
                }
            }
        }

        private void PauseAll()
        {
            if (m_downloadPageViewModel.Downloadings.Count == 0) return;
            foreach (var item in m_downloadPageViewModel.Downloadings)
            {
                foreach (var subItem in item.Items)
                {
                    PauseItem(subItem);
                }
            }
        }

        private async void DeleteAll()
        {
            if (m_downloadPageViewModel.Downloadings.Count == 0) return;
            if (!await NotificationShowExtensions.ShowMessageDialog("取消任务", "确定要取消全部任务吗?"))
            {
                return;
            }
            foreach (var item in m_downloadPageViewModel.Downloadings.ToList())
            {
                try
                {
                    m_loadDownloadingCts[item.EpisodeID].Cancel();
                    var folder = await StorageFolder.GetFolderFromPathAsync(item.Path);
                    await folder.DeleteAsync(StorageDeleteOption.PermanentDelete);
                    RemoveDbSubItem(item.EpisodeID);
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
            });
        }

        private void LoadDownloadingAddDownloadingItem(IGrouping<string, DownloadingSubItemViewModel> subItemGroup)
        {
            var subItemList = new ObservableCollection<DownloadingSubItemViewModel>();
            foreach (var subItem in subItemGroup)
            {
                subItemList.Add(subItem);
            }
            m_downloadPageViewModel.Downloadings.Add(new DownloadingItemViewModel()
            {
                EpisodeID = subItemGroup.Key,
                Items = subItemList,
                Title = subItemGroup.FirstOrDefault().Title,
                EpisodeTitle = subItemGroup.FirstOrDefault().EpisodeTitle,
                Path = subItemGroup.FirstOrDefault().Path,
                DeleteItemCommand = m_downloadPageViewModel.DeleteItemCommand,
            });
        }

        private async Task LoadDownloadFromIndex()
        {
            var downloadedDtos = m_biliLiteDbContext.DownloadedItems
                .Include(x => x.Epsidoes)
                .OrderByDescending(x => x.UpdateTime)
                .ToList();
            var downloadedItems = m_mapper.Map<List<DownloadedItem>>(downloadedDtos);

            m_downloadPageViewModel.LoadedDownloadedCount = 0;
            m_downloadPageViewModel.TotalDownloadedCount = downloadedItems.Count;

            foreach (var downloadedItem in downloadedItems)
            {
                var coverFile = await StorageFile.GetFileFromPathAsync(downloadedItem.CoverPath);
                var bitmapImage = new BitmapImage();
                var buffer = await FileIO.ReadBufferAsync(coverFile);
                using IRandomAccessStream stream = new InMemoryRandomAccessStream();
                await stream.WriteAsync(buffer);
                stream.Seek(0);
                bitmapImage.SetSource(stream);
                downloadedItem.Cover = bitmapImage;

                downloadedItem.Epsidoes = new ObservableCollection<DownloadedSubItem>(downloadedItem.Epsidoes.OrderBy(x => x.Index).ToList());

                m_downloadPageViewModel.LoadedDownloadedCount++;
            }
            m_downloadPageViewModel.Downloadeds = downloadedItems;
            m_downloadPageViewModel.DownloadedViewModels = new ObservableCollection<DownloadedItem>(downloadedItems);
        }

        private IEnumerable<DownloadedItem> QueryDownloaded()
        {
            var query = m_downloadPageViewModel.Downloadeds.AsEnumerable();

            // 提前处理搜索条件
            var searchKeyword = m_downloadPageViewModel.SearchKeyword?.ToLower();
            if (!string.IsNullOrEmpty(searchKeyword))
            {
                query = query.Where(x => x.Title.ToLower().Contains(searchKeyword));
            }

            // 使用 switch 表达式简化排序逻辑
            query = m_downloadPageViewModel.DownloadedSortMode switch
            {
                DownloadedSortMode.TimeDesc => query.OrderByDescending(x => x.UpdateTime),
                DownloadedSortMode.TimeAsc => query.OrderBy(x => x.UpdateTime),
                DownloadedSortMode.TitleDesc => query.OrderByDescending(x => x.Title),
                DownloadedSortMode.TitleAsc => query.OrderBy(x => x.Title),
                _ => query // 默认不排序
            };

            return query;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 检查视频是否已下载或正在下载
        /// </summary>
        /// <returns>state:2-下载中，3-已下载</returns>
        public int CheckExist(string id, bool isSeason = false)
        {
            // Check if the item is currently downloading
            if (m_downloadPageViewModel.Downloadings.Any(x => x.EpisodeID == id))
            {
                return 2; // Item is downloading
            }

            // Define the property to check based on the isSeason flag
            var downloadedPredicate = isSeason
                ? (Func<DownloadedSubItem, bool>)(subItem => subItem.EpisodeID == id)
                : (Func<DownloadedSubItem, bool>)(subItem => subItem.CID == id);

            // Check if the item is already downloaded
            if (m_downloadPageViewModel.Downloadeds.Any(x => x.Epsidoes.Any(downloadedPredicate)))
            {
                return 3; // Item is downloaded
            }

            // If neither condition is met, return 0
            return 0;
        }

        public DownloadedSubItem FindDownloadSubItemById(string id, bool isSeason = false)
        {
            if (m_downloadPageViewModel.Downloadings.Any(x => x.EpisodeID == id))
            {
                return null; // Item is downloading
            }

            var downloadedPredicate = isSeason
                ? (Func<DownloadedSubItem, bool>)(subItem => subItem.EpisodeID == id)
                : (Func<DownloadedSubItem, bool>)(subItem => subItem.CID == id);

            return m_downloadPageViewModel.Downloadeds
                .Select(downloadedItem => downloadedItem.Epsidoes.FirstOrDefault(downloadedPredicate))
                .FirstOrDefault(subItem => subItem != null);
        }

        /// <summary>
        /// 暂停下载
        /// </summary>
        /// <param name="item"></param>
        public void PauseItem(DownloadingSubItemViewModel item)
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
        public void ResumeItem(DownloadingSubItemViewModel item)
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

        public void RemoveDbItem(string id)
        {
            var entity = m_biliLiteDbContext.DownloadedItems
                .Include(x => x.Epsidoes)
                .FirstOrDefault(x => x.ID == id);
            if (entity == null) return;
            m_biliLiteDbContext.DownloadedSubItems.RemoveRange(entity.Epsidoes);
            m_biliLiteDbContext.DownloadedItems.Remove(entity);
            m_biliLiteDbContext.SaveChanges();
        }

        public void RemoveDbSubItem(string id)
        {
            var entity = m_biliLiteDbContext.DownloadedSubItems
                .FirstOrDefault(x => x.CID == id);
            if (entity == null) return;
            m_biliLiteDbContext.DownloadedSubItems.Remove(entity);
            m_biliLiteDbContext.SaveChanges();
        }

        public void RefreshDownloaded()
        {
            if (m_downloadPageViewModel.LoadingDownloaded) return;
            LoadDownloaded();
        }

        public void SearchDownloaded(string keyword)
        {
            m_downloadPageViewModel.SearchKeyword = keyword;
            m_downloadPageViewModel.IsSearching = !string.IsNullOrEmpty(keyword);

            var searchResult = QueryDownloaded();
            m_downloadPageViewModel.DownloadedViewModels.Clear();
            m_downloadPageViewModel.DownloadedViewModels.AddRange(searchResult);
        }

        /// <summary>
        /// 读取下载的视频
        /// </summary>
        /// <returns></returns>
        public async Task LoadDownloaded(bool buildIndex = false)
        {
            m_downloadPageViewModel.LoadingDownloaded = true;
            m_downloadPageViewModel.DownloadedViewModels.Clear();
            m_downloadPageViewModel.Downloadeds.Clear();
            var folder = await DownloadHelper.GetDownloadFolder();
            await LoadDiskSize(folder);

            var initIndex = false;
            if (m_useDownloadIndex && m_biliLiteDbContext.DownloadedItems.Any() && !buildIndex)
            {
                await LoadDownloadFromIndex();
                m_downloadPageViewModel.LoadingDownloaded = false;
                return;
            }

            if (m_useDownloadIndex && !m_biliLiteDbContext.DownloadedItems.Any())
            {
                initIndex = true;
            }

            var folders = await folder.GetFoldersAsync();
            m_downloadPageViewModel.LoadedDownloadedCount = 0;
            m_downloadPageViewModel.TotalDownloadedCount = folders.Count;
            foreach (var item in folders)
            {
                try
                {
                    //检查是否存在info.json
                    if (!(await item.TryGetItemAsync("info.json") is StorageFile infoFile)) continue;

                    var info = JsonConvert.DeserializeObject<DownloadSaveInfo>(await FileIO.ReadTextAsync(infoFile));
                    info.Path = item.Path;
                    //旧版无Cover字段，跳过
                    if (string.IsNullOrEmpty(info.Cover)) continue;
                    var lsEpisodes = new List<DownloadedSubItem>();
                    var lsEpisodeInfos = new List<DownloadSaveEpisodeInfo>();
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
                        episodeInfo.Path = episodeItem.Path;
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
                        lsEpisodeInfos.Add(episodeInfo);
                    }

                    if ((buildIndex && m_useDownloadIndex) ||
                        initIndex)
                        AddDownloadItemsIndex(downloadedItem, lsEpisodes);
                    //排序
                    foreach (var episode in lsEpisodes.OrderBy(x => x.Index))
                    {
                        downloadedItem.Epsidoes.Add(episode);
                    }

                    m_downloadPageViewModel.Downloadeds.Add(downloadedItem);
                    m_downloadPageViewModel.DownloadedViewModels.Add(downloadedItem);
                    m_downloadPageViewModel.LoadedDownloadedCount++;
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

            m_downloadPageViewModel.LoadingDownloaded = false;
        }


        /// <summary>
        /// 读取旧版下载的视频
        /// </summary>
        /// <returns></returns>
        public async Task LoadDownloadedOld()
        {
            var folder = await DownloadHelper.GetDownloadOldFolder();

            //var list = new List<DownloadedItem>();
            var folders = await folder.GetFoldersAsync();
            m_downloadPageViewModel.TotalDownloadedCount += folders.Count;
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
                            FilePath = episodeItem.Path
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

                    m_downloadPageViewModel.Downloadeds.Add(downloadedItem);
                    m_downloadPageViewModel.DownloadedViewModels.Add(downloadedItem);
                    m_downloadPageViewModel.LoadedDownloadedCount++;
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
            m_downloadPageViewModel.DiskFree = (ulong)properties["System.FreeSpace"] / 1024d / 1024d / 1024d;
            m_downloadPageViewModel.DiskTotal = (ulong)properties["System.Capacity"] / 1024d / 1024d / 1024d;
            m_downloadPageViewModel.DiskUse = m_downloadPageViewModel.DiskTotal - m_downloadPageViewModel.DiskFree;
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
            m_downloadPageViewModel.Downloadings.Clear();
            //var downloadOperations = await BackgroundDownloader.GetCurrentDownloadsForTransferGroupAsync(DownloadHelper.group);

            // 在 WinUI 3 中，你需要使用不同的方式来获取当前下载
            var downloadOperations = await BackgroundDownloader.GetCurrentDownloadsAsync();

            // 然后通过 Group 属性进行筛选
            var groupDownloads = downloadOperations.Where(op => op.Group == DownloadHelper.groupName);


            foreach (var downloadOperation in groupDownloads)
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

        public async Task AddOtherTracksToDownloadedSubItemIndex(DownloadInfo info, DownloadSaveEpisodeInfo downloadSaveEpisodeInfo)
        {
            var downloadedSubItemDTO = await m_biliLiteDbContext.DownloadedSubItems.FirstOrDefaultAsync(x => x.CID == info.CID);

            foreach (var item in info.Urls)
            {
                var paths = downloadedSubItemDTO.Paths;
                paths.Add(Path.Combine(downloadSaveEpisodeInfo.Path, item.FileName));
                downloadedSubItemDTO.Paths = paths;
            }

            var downloadedSubItem = FindDownloadSubItemById(info.CID, info.Type == DownloadType.Season);
            downloadedSubItem.Paths = downloadedSubItemDTO.Paths;

            await m_biliLiteDbContext.SaveChangesAsync();
        }

        public void AddDownloadItemsIndex(DownloadSaveInfo downloadSaveInfo, DownloadSaveEpisodeInfo downloadSaveEpisodeInfo)
        {
            var downloadedItem = new DownloadedItem()
            {
                CoverPath = Path.Combine(downloadSaveInfo.Path, "cover.jpg"),
                Epsidoes = new ObservableCollection<DownloadedSubItem>(),
                ID = downloadSaveInfo.ID,
                Title = downloadSaveInfo.Title,
                UpdateTime = downloadSaveInfo.CreatedTime,
                IsSeason = downloadSaveInfo.Type == DownloadType.Season,
                Path = downloadSaveInfo.Path
            };
            var downloadedSubItem = m_mapper.Map<DownloadedSubItem>(downloadSaveEpisodeInfo);
            foreach (var path in downloadSaveEpisodeInfo.VideoPath)
            {
                downloadedSubItem.Paths.Add(Path.Combine(downloadSaveEpisodeInfo.Path, path));
            }

            foreach (var downloadSubtitleInfo in downloadSaveEpisodeInfo.SubtitlePath)
            {
                if (!downloadSubtitleInfo.Url.IsUrl())
                {
                    downloadSubtitleInfo.Url =
                        Path.Combine(downloadSaveEpisodeInfo.Path, downloadSubtitleInfo.Url);
                }

                downloadedSubItem.SubtitlePath.Add(downloadSubtitleInfo);
            }

            if (!downloadSaveEpisodeInfo.DanmakuPath.IsUrl())
            {
                downloadedSubItem.DanmakuPath = Path.Combine(downloadSaveEpisodeInfo.Path, downloadSaveEpisodeInfo.DanmakuPath);
            }

            var downloadItemDto = m_mapper.Map<DownloadedItemDTO>(downloadedItem);
            var downloadSubItemDto = m_mapper.Map<DownloadedSubItemDTO>(downloadedSubItem);
            var downloadItemData = m_biliLiteDbContext.DownloadedItems.FirstOrDefault(x => x.ID == downloadItemDto.ID);
            if (downloadItemData != null)
            {
                downloadItemData.Epsidoes.Add(downloadSubItemDto);
                downloadItemData.UpdateTime = downloadItemDto.UpdateTime;
            }
            else
            {
                downloadItemDto.Epsidoes = new List<DownloadedSubItemDTO>() { downloadSubItemDto };
                m_biliLiteDbContext.DownloadedItems.Add(downloadItemDto);
            }

            m_biliLiteDbContext.SaveChanges();
        }

        public void AddDownloadItemsIndex(DownloadedItem downloadedItem, List<DownloadedSubItem> downloadedSubItems)
        {
            var downloadItemDto = m_mapper.Map<DownloadedItemDTO>(downloadedItem);
            downloadItemDto.Epsidoes = m_mapper.Map<List<DownloadedSubItemDTO>>(downloadedSubItems);
            m_biliLiteDbContext.DownloadedItems.Add(downloadItemDto);
            m_biliLiteDbContext.SaveChanges();
        }

        public void ClearIndex()
        {
            m_biliLiteDbContext.DownloadedSubItems.RemoveRange(m_biliLiteDbContext.DownloadedSubItems);
            m_biliLiteDbContext.DownloadedItems.RemoveRange(m_biliLiteDbContext.DownloadedItems);
        }

        public void SetDownloadedSortMode(DownloadedSortMode mode)
        {
            m_downloadPageViewModel.DownloadedSortMode = mode;
            m_downloadPageViewModel.DownloadedViewModels.Clear();

            var query = QueryDownloaded();

            m_downloadPageViewModel.DownloadedViewModels.AddRange(query);
        }

        #endregion
    }
}