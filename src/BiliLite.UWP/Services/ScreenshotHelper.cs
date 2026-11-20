using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using BiliLite.Models.Common;

namespace BiliLite.Services
{
    public static class ScreenshotHelper
    {
        private const string DEFAULT_SCREENSHOT_FOLDER_NAME = "哔哩哔哩截图";

        private static readonly ILogger logger = GlobalLogger.FromCurrentType();

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
                    logger.Warn("通过 FutureAccessList 令牌获取截图目录失败，回退到路径访问", ex);
                }
            }

            return await StorageFolder.GetFolderFromPathAsync(path);
        }

        /// <summary>
        /// 保存截图。优先走 UWP 的 StorageFolder（默认图片库目录走这条）；
        /// 令牌失效、目录带 hidden/system 属性、目录被删等情况 WinRT 会直接拒绝，
        /// 此时回退到纯 .NET 的 System.IO —— 打包的 WinUI3 应用带 runFullTrust，
        /// 进程用的是用户完整令牌，普通文件读写不受 UWP 存储规则限制。
        /// </summary>
        public static async Task<string> SaveScreenshotAsync(string fileName, byte[] bytes)
        {
            try
            {
                var folder = await GetScreenshotFolder();
                var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
                await FileIO.WriteBytesAsync(file, bytes);
                return file.Path;
            }
            catch (Exception ex)
            {
                logger.Warn("通过 StorageFolder 保存截图失败，回退到 System.IO", ex);
            }

            var directory = ResolveFallbackDirectory();
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, fileName);
            await File.WriteAllBytesAsync(path, bytes);
            return path;
        }

        /// <summary>
        /// System.IO 回退目录：自定义目录直接用配置里的绝对路径，默认目录用「我的图片\哔哩哔哩截图」
        /// </summary>
        private static string ResolveFallbackDirectory()
        {
            var path = SettingService.GetValue(SettingConstants.Player.SCREENSHOT_PATH,
                SettingConstants.Player.DEFAULT_SCREENSHOT_PATH);
            if (string.IsNullOrWhiteSpace(path) || path == SettingConstants.Player.DEFAULT_SCREENSHOT_PATH)
            {
                var pictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                return Path.Combine(pictures, DEFAULT_SCREENSHOT_FOLDER_NAME);
            }

            return path;
        }
    }
}
