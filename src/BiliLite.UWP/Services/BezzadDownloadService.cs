using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Models.Download;
using Downloader;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLite.Services
{
    public class BezzadDownloadService : IDownloadService
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private const string DOWNLOAD_FOLDER_NAME = "哔哩哔哩下载";
        private readonly ConcurrentDictionary<string, BezzadDownloadOperation> m_activeDownloads;
        private readonly ConcurrentDictionary<string, Downloader.DownloadService> m_downloaderServices;
        
        public BezzadDownloadService()
        {
            m_activeDownloads = new ConcurrentDictionary<string, BezzadDownloadOperation>();
            m_downloaderServices = new ConcurrentDictionary<string, Downloader.DownloadService>();
        }

        public async Task AddDownload(DownloadInfo downloadInfo)
        {
            //读取存储文件夹
            StorageFolder folder = await GetDownloadFolder();
            folder = await folder.CreateFolderAsync((downloadInfo.Type == DownloadType.Season ? ("ss" + downloadInfo.SeasonID) : downloadInfo.AVID), CreationCollisionOption.OpenIfExists);
            StorageFolder episodeFolder = await folder.CreateFolderAsync(downloadInfo.Type == DownloadType.Season ? downloadInfo.EpisodeID : downloadInfo.CID, CreationCollisionOption.OpenIfExists);

            if (downloadInfo.AddOthersTrack)
            {
                foreach (var item in downloadInfo.Urls)
                {
                    await DownloadVideo(downloadInfo, item, episodeFolder);
                }
                await SaveInfo(downloadInfo, folder, episodeFolder);
                return;
            }

            //下载封面
            await DownloadCover(downloadInfo.CoverUrl, folder);
            //下载弹幕
            await DownloadDanmaku(downloadInfo.DanmakuUrl, episodeFolder);
            //下载字幕
            if (downloadInfo.Subtitles != null)
            {
                foreach (var item in downloadInfo.Subtitles)
                {
                    await DownloadSubtitle(item, episodeFolder);
                }
            }

            //下载视频
            foreach (var item in downloadInfo.Urls)
            {
                DownloadVideo(downloadInfo, item, episodeFolder);
            }
            //保存文件
            await SaveInfo(downloadInfo, folder, episodeFolder);
        }

        public async Task PauseDownload(string guid)
        {
            if (m_activeDownloads.TryGetValue(guid, out var downloadOperation))
            {
                downloadOperation.Pause();
            }
        }

        public async Task ResumeDownload(string guid)
        {
            if (m_activeDownloads.TryGetValue(guid, out var downloadOperation))
            {
                downloadOperation.Resume();
            }
        }

        public async Task<IEnumerable<IDownloadOperation>> GetCurrentDownloads()
        {
            var operations = m_activeDownloads.Values.Cast<IDownloadOperation>().ToList();
            return operations.AsEnumerable();
        }

        private async Task<StorageFolder> GetDownloadFolder()
        {
            var path = SettingService.GetValue(SettingConstants.Download.DOWNLOAD_PATH, SettingConstants.Download.DEFAULT_PATH);
            if (path != SettingConstants.Download.DEFAULT_PATH) return await StorageFolder.GetFolderFromPathAsync(path);
            var folder = KnownFolders.VideosLibrary;
            return await folder.CreateFolderAsync(DOWNLOAD_FOLDER_NAME, CreationCollisionOption.OpenIfExists);
        }

        private async Task DownloadCover(string url, StorageFolder folder)
        {
            try
            {
                var buffer = await (url + "@200w.jpg").GetBuffer();
                StorageFile file = await folder.CreateFileAsync("cover.jpg", CreationCollisionOption.OpenIfExists);
                await FileIO.WriteBufferAsync(file, buffer);
            }
            catch (Exception ex)
            {
                _logger.Log("封面下载失败:" + url, LogType.Error, ex);
            }
        }

        private async Task DownloadDanmaku(string url, StorageFolder episodeFolder)
        {
            try
            {
                var buffer = await url.GetBuffer();
                StorageFile file = await episodeFolder.CreateFileAsync("danmaku.xml", CreationCollisionOption.OpenIfExists);
                await FileIO.WriteBufferAsync(file, buffer);
            }
            catch (Exception ex)
            {
                _logger.Log("弹幕下载失败:" + url, LogType.Error, ex);
            }
        }

        private async Task SaveInfo(DownloadInfo info, StorageFolder folder, StorageFolder episodeFolder)
        {
            try
            {
                if (info.AddOthersTrack)
                {
                    var episodeFile = await episodeFolder.GetFileAsync("info.json");
                    var text = await FileIO.ReadTextAsync(episodeFile);
                    var downloadSaveEpisodeInfo = await text.DeserializeJson<DownloadSaveEpisodeInfo>();

                    foreach (var item in info.Urls)
                    {
                        downloadSaveEpisodeInfo.VideoPath.Add(item.FileName);
                    }
                    await FileIO.WriteTextAsync(episodeFile, Newtonsoft.Json.JsonConvert.SerializeObject(downloadSaveEpisodeInfo));

                    if (SettingService.GetValue(SettingConstants.Download.USE_DOWNLOAD_INDEX,
                            SettingConstants.Download.DEFAULT_USE_DOWNLOAD_INDEX))
                    {
                        var downloadService = App.ServiceProvider.GetRequiredService<DownloadService>();
                        await downloadService.AddOtherTracksToDownloadedSubItemIndex(info, downloadSaveEpisodeInfo);
                    }
                    return;
                }

                {
                    DownloadSaveInfo downloadSaveInfo = new DownloadSaveInfo()
                    {
                        Cover = info.CoverUrl,
                        SeasonType = info.SeasonType,
                        Title = info.Title,
                        Type = info.Type,
                        ID = info.Type == DownloadType.Season ? info.SeasonID.ToString() : info.AVID,
                        Path = folder.Path,
                        CreatedTime = DateTime.Now,
                    };
                    DownloadSaveEpisodeInfo downloadSaveEpisodeInfo = new DownloadSaveEpisodeInfo()
                    {
                        CID = info.CID,
                        DanmakuPath = "danmaku.xml",
                        EpisodeID = info.EpisodeID,
                        EpisodeTitle = info.EpisodeTitle,
                        AVID = info.AVID,
                        SubtitlePath = new List<DownloadSubtitleInfo>(),
                        Index = info.Index,
                        VideoPath = new List<string>(),
                        QualityID = info.QualityID,
                        QualityName = info.QualityName,
                        Path = episodeFolder.Path,
                    };
                    if (info.Subtitles != null)
                    {
                        foreach (var item in info.Subtitles)
                        {
                            downloadSaveEpisodeInfo.SubtitlePath.Add(new DownloadSubtitleInfo()
                            {
                                Name = item.Name,
                                Url = item.Name + ".json"
                            });
                        }
                    }

                    foreach (var item in info.Urls)
                    {
                        downloadSaveEpisodeInfo.VideoPath.Add(item.FileName);
                    }

                    downloadSaveEpisodeInfo.IsDash =
                        downloadSaveEpisodeInfo.VideoPath.FirstOrDefault(x => x.Contains(".m4s")) != null;
                    StorageFile file =
                        await folder.CreateFileAsync("info.json", CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteTextAsync(file, Newtonsoft.Json.JsonConvert.SerializeObject(downloadSaveInfo));
                    StorageFile episodeFile =
                        await episodeFolder.CreateFileAsync("info.json", CreationCollisionOption.ReplaceExisting);
                    await FileIO.WriteTextAsync(episodeFile, Newtonsoft.Json.JsonConvert.SerializeObject(downloadSaveEpisodeInfo));

                    if (SettingService.GetValue(SettingConstants.Download.USE_DOWNLOAD_INDEX,
                            SettingConstants.Download.DEFAULT_USE_DOWNLOAD_INDEX))
                    {
                        var downloadService = App.ServiceProvider.GetRequiredService<DownloadService>();
                        downloadService.AddDownloadItemsIndex(downloadSaveInfo, downloadSaveEpisodeInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log("文件保存失败:" + episodeFolder.Path, LogType.Error, ex);
            }
        }

        private async Task DownloadSubtitle(DownloadSubtitleInfo subtitleInfo, StorageFolder episodeFolder)
        {
            try
            {
                var url = subtitleInfo.Url;
                if (!url.Contains("http:") || !url.Contains("https:"))
                {
                    url = "https:" + url;
                }
                var buffer = await url.GetBuffer();
                StorageFile file = await episodeFolder.CreateFileAsync(subtitleInfo.Name + ".json", CreationCollisionOption.OpenIfExists);
                await FileIO.WriteBufferAsync(file, buffer);
            }
            catch (Exception ex)
            {
                _logger.Log($"字幕下载失败:{subtitleInfo.Name}={subtitleInfo.Url}", LogType.Error, ex);
            }
        }
        

        private async Task DownloadVideo(DownloadInfo downloadInfo, DownloadUrlInfo urlInfo, StorageFolder episodeFolder)
        {
            try
            {
                var filePath = Path.Combine(episodeFolder.Path, urlInfo.FileName);

                var downloadConfiguration = new DownloadConfiguration()
                {
                    ParallelDownload = true,
                    ChunkCount = 8,
                    MaxTryAgainOnFailure = 5,
                    BufferBlockSize = 10240,
                    Timeout = 10000,
                    RequestConfiguration = new RequestConfiguration()
                    {
                        UserAgent = urlInfo.HttpHeader["User-Agent"],
                        Referer = urlInfo.HttpHeader["Referer"],
                    }
                };

                var downloadService = new Downloader.DownloadService(downloadConfiguration);
                
                var downloadPackage = new DownloadPackage()
                {
                    FileName = urlInfo.FileName,
                    Urls = new[] { urlInfo.Url }
                };

                var downloadOperation = new BezzadDownloadOperation(downloadService, downloadPackage, filePath, downloadInfo);
                var guid = downloadOperation.Guid.ToString();

                m_activeDownloads.TryAdd(guid, downloadOperation);
                m_downloaderServices.TryAdd(guid, downloadService);

                await downloadOperation.StartAsync();
            }
            catch (Exception ex)
            {
                _logger.Error($"下载文件失败: {urlInfo.FileName}", ex);
                throw;
            }
        }
    }
}
