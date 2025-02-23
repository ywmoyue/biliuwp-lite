using BiliLite.Controls;
using BiliLite.Dialogs;
using BiliLite.Models.Common.Notifications.Template;
using BiliLite.Models.Requests.Api;
using BiliLite.Services;
using BiliLite.Services.Notification;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace BiliLite.Extensions.Notifications
{
    public static class NotificationShowExtensions
    {
        private static bool dialogShowing = false;

        public static async Task ShowTile()
        {
            // Create a tile update manager for the specified syndication feed.
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.Clear();
            updater.EnableNotificationQueue(true);

            var liveTileHelper = App.ServiceProvider.GetRequiredService<LiveTileService>();

            await liveTileHelper.RefreshTile();
            foreach (var tile in liveTileHelper.TileFurnace)
            {
                var content = TileTemplate.LiveTile(tile);
                var notification = new TileNotification(content.GetXml())
                {
                    Tag = tile.Name,
                };
                updater.Update(notification);
            }
        }

        public static void ShowToast()
        {

        }

        public static async Task<bool> ShowMessageDialog(string title, string content)
        {
            MessageDialog messageDialog = new MessageDialog(content, title);
            messageDialog.Commands.Add(new UICommand() { Label = "确定", Id = true });
            messageDialog.Commands.Add(new UICommand() { Label = "取消", Id = false });
            var result = await messageDialog.ShowAsync();
            return (bool)result.Id;
        }

        public static void ShowMessageToast(string message, int seconds = 2)
        {
            MessageToast ms = new MessageToast(message, TimeSpan.FromSeconds(seconds));
            ms.Show();
        }

        public static void ShowMessageToast(string message, List<MyUICommand> commands, int seconds = 15)
        {
            MessageToast ms = new MessageToast(message, TimeSpan.FromSeconds(seconds), commands);
            ms.Show();
        }

        public static async Task ShowContentDialog(ContentDialog ms)
        {
            await ms.ShowAsync();
        }

        /// <summary>
        /// 登录专用
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> ShowLoginDialog()
        {
            if (!dialogShowing)
            {
                LoginDialog ms = new LoginDialog();
                dialogShowing = true;
                await ms.ShowAsync();
                dialogShowing = false;
            }
            if (SettingService.Account.Logined)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Comment 专用
        /// </summary>
        /// <param name="oid"></param>
        /// <param name="commentMode"></param>
        /// <param name="commentSort"></param>
        public static void ShowCommentDialog(string oid, int commentMode, CommentApi.CommentSort commentSort)
        {
            CommentDialog ms = new CommentDialog();
            ms.Show(oid, commentMode, commentSort);
        }
    }
}
