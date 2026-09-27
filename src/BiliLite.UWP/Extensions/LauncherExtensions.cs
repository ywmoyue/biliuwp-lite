using BiliLite.Extensions.Notifications;
using BiliLite.Services;
using System;
using System.Threading.Tasks;
using Windows.System;

namespace BiliLite.Extensions
{
    /// <summary>
    /// 使用系统默认浏览器打开链接
    /// </summary>
    public static class LauncherExtensions
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        /// <summary>
        /// 使用系统默认浏览器打开链接，失败时给出提示
        /// </summary>
        /// <param name="url">链接地址</param>
        /// <returns>是否成功唤起默认浏览器</returns>
        public static async Task<bool> OpenInBrowserAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url) || !Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                _logger.Warn($"打开链接失败，链接无效：{url}");
                NotificationShowExtensions.ShowMessageToast("链接无效，无法打开");
                return false;
            }

            return await OpenInBrowserAsync(uri);
        }

        /// <summary>
        /// 使用系统默认浏览器打开链接，失败时给出提示
        /// </summary>
        /// <param name="uri">链接地址</param>
        /// <returns>是否成功唤起默认浏览器</returns>
        public static async Task<bool> OpenInBrowserAsync(Uri uri)
        {
            if (uri == null)
            {
                _logger.Warn("打开链接失败，链接为空");
                NotificationShowExtensions.ShowMessageToast("链接无效，无法打开");
                return false;
            }

            var success = false;
            try
            {
                success = await Launcher.LaunchUriAsync(uri);
            }
            catch (Exception ex)
            {
                _logger.Error($"打开链接失败：{uri}", ex);
            }

            if (!success)
            {
                _logger.Warn($"打开链接失败，请检查系统默认浏览器设置：{uri}");
                NotificationShowExtensions.ShowMessageToast("未能打开浏览器，请检查系统默认浏览器设置");
            }

            return success;
        }
    }
}
