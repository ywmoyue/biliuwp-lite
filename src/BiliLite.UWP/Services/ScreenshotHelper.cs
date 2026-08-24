using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using BiliLite.Models.Common;

namespace BiliLite.Services
{
    public static class ScreenshotHelper
    {
        private const string DEFAULT_SCREENSHOT_FOLDER_NAME = "哔哩哔哩截图";

        public static async Task<StorageFolder> GetScreenshotFolder()
        {
            var path = SettingService.GetValue(SettingConstants.Player.SCREENSHOT_PATH,
                SettingConstants.Player.DEFAULT_SCREENSHOT_PATH);
            if (path == SettingConstants.Player.DEFAULT_SCREENSHOT_PATH)
            {
                var folder = KnownFolders.PicturesLibrary;
                return await folder.CreateFolderAsync(DEFAULT_SCREENSHOT_FOLDER_NAME, CreationCollisionOption.OpenIfExists);
            }

            // 自定义目录经由 FolderPicker 授权，只能通过 FutureAccessList 令牌访问，无法按路径访问
            var token = SettingService.GetValue(SettingConstants.Player.SCREENSHOT_PATH_TOKEN, "");
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync(token);
                }
                catch (Exception ex)
                {
                    // 令牌失效时回退到按路径访问
                    GlobalLogger.FromCurrentType().Warn("通过 FutureAccessList 令牌获取截图目录失败，回退到路径访问", ex);
                }
            }

            return await StorageFolder.GetFolderFromPathAsync(path);
        }
    }
}
