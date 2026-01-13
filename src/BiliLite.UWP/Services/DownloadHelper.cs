using System;
using System.Threading.Tasks;
using Windows.Storage;
using BiliLite.Models.Common;
using BiliLite.Models.Download;
using BiliLite.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLite.Services
{
    public static class DownloadHelper
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();
        private const string DOWNLOAD_FOLDER_NAME = "哔哩哔哩下载";
        private const string OLD_DOWNLOAD_FOLDER_NAME = "BiliBiliDownload";

        public static async Task AddDownload(DownloadInfo downloadInfo)
        {
            // 使用依赖注入获取下载服务
            var downloadService = App.ServiceProvider.GetRequiredService<IDownloadService>();
            await downloadService.AddDownload(downloadInfo);
        }

        public static async Task<StorageFolder> GetDownloadFolder()
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
        public static async Task<StorageFolder> GetDownloadOldFolder()
        {
            var path = SettingService.GetValue(SettingConstants.Download.OLD_DOWNLOAD_PATH, SettingConstants.Download.DEFAULT_OLD_PATH);
            if (path != SettingConstants.Download.DEFAULT_OLD_PATH)
                return await StorageFolder.GetFolderFromPathAsync(path);
            var folder = KnownFolders.VideosLibrary;
            return await folder.CreateFolderAsync(OLD_DOWNLOAD_FOLDER_NAME, CreationCollisionOption.OpenIfExists);
        }

        public static async Task<bool> UpdateDanmaku(string cid, string epId, string avId, string seasonId, bool isSeason)
        {
            var url = $"{ApiHelper.API_BASE_URL}/x/v1/dm/list.so?oid=" + cid;

            //读取存储文件夹
            StorageFolder folder = await GetDownloadFolder();
            folder = await folder.CreateFolderAsync((isSeason ? ("ss" + seasonId) : avId),
                CreationCollisionOption.OpenIfExists);
            StorageFolder episodeFolder =
                await folder.CreateFolderAsync(isSeason ? epId : cid, CreationCollisionOption.OpenIfExists);

            try
            {
                var buffer = await url.GetBuffer();
                StorageFile file =
                    await episodeFolder.CreateFileAsync("danmaku.xml", CreationCollisionOption.OpenIfExists);
                await FileIO.WriteBufferAsync(file, buffer);
            }
            catch (Exception ex)
            {
                logger.Log("弹幕下载失败:" + url, LogType.Error, ex);
                return false;
            }

            return true;
        }
    }
}