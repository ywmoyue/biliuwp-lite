using BiliLite.Controls;
using BiliLite.Controls.Dialogs;
using BiliLite.Models.Requests.Api;
using BiliLite.Services;
using BiliLite.Services.Notification;
using BiliLite.Controls.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using MessageDialog = BiliLite.Controls.Dialogs.MessageDialog;
// using Windows.UI.Popups;

namespace BiliLite.Extensions.Notifications
{
    public static class NotificationShowExtensions
    {
        private static bool dialogShowing = false;

        public static async Task ShowTile()
        {
            //// Create a tile update manager for the specified syndication feed.
            //var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            //updater.Clear();
            //updater.EnableNotificationQueue(true);

            //var liveTileHelper = App.ServiceProvider.GetRequiredService<LiveTileService>();

            //await liveTileHelper.RefreshTile();
            //foreach (var tile in liveTileHelper.TileFurnace)
            //{
            //    var content = TileTemplate.LiveTile(tile);
            //    var notification = new TileNotification(content.GetXml())
            //    {
            //        Tag = tile.Name,
            //    };
            //    updater.Update(notification);
            //}
        }

        public static void ShowToast()
        {

        }

        public static async Task<bool> ShowMessageDialog(string title, string content, Uri uri = null)
        {
            MessageDialog messageDialog = new MessageDialog(content, title);
            messageDialog.Commands.Add(new UICommand()
            {
                Label = "确定",
                Id = true,
                Invoked = async (cmd) =>
                {
                    if (uri != null)
                        await Launcher.LaunchUriAsync(uri);
                }
            });
            messageDialog.Commands.Add(new UICommand() { Label = "取消", Id = false });
            var result = await messageDialog.ShowAsync();
            return (bool)result.Id;
        }

        public static async Task<bool> ShowCommAntifraudDialog(string content)
        {
            MessageDialog messageDialog = new MessageDialog(content, "发评反诈");
            messageDialog.Commands.Add(new UICommand()
            {
                Label = "确定",
                Id = true,
            });
            messageDialog.Commands.Add(new UICommand() { Label = "打开浏览器手动申诉", Id = false, Invoked = command => {
                Launcher.LaunchUriAsync(new Uri("https://www.bilibili.com/blackboard/cmmnty-appeal.html"));
            } });
            var result = await messageDialog.ShowAsync();
            return (bool)result.Id;
        }

        public static async void ShowVideoErrorMessageDialog(string message)
        {
            await ShowMessageDialog($@"播放失败:{message}
你可以进行以下尝试:
1、切换视频清晰度
2、到⌈设置⌋-⌈播放⌋中修改⌈优先视频编码⌋选项
3、到⌈设置⌋-⌈播放⌋中打开或关闭⌈替换PCDN链接⌋选项
4、到⌈设置⌋-⌈代理⌋中打开或关闭⌈尝试替换视频的CDN⌋选项
5、如果视频编码选择了HEVC，请检查是否安装了HEVC扩展
6、如果视频编码选择了AV1，请检查是否安装了AV1扩展
7、如果是付费视频，请在手机或网页端购买后观看
8、尝试更新您的显卡驱动或使用核显打开应用", "播放失败");
        }

        public static void ShowMessageToast(string message, int seconds = 2)
        {
            AppNotification notification = new AppNotificationBuilder()
                .AddText(message)
                .BuildNotification();

            AppNotificationManager.Default.Show(notification);
        }

        public static void ShowMessageToastV2(this FrameworkElement element, string message, int seconds = 2)
        {
            if(element.XamlRoot == null)
            {
                ShowMessageToast(message, seconds);
            }
            MessageToast ms = new MessageToast(element.XamlRoot, message, TimeSpan.FromSeconds(seconds));
            ms.Show();
        }

        public static void ShowMessageToast(string message, List<MyUICommand> commands, int seconds = 15)
        {
            var builder = new AppNotificationBuilder()
                .AddText(message);

            var notification = builder.BuildNotification();

            AppNotificationManager.Default.Show(notification);
        }

        public static async Task ShowContentDialog(ContentDialog ms)
        {
            if (ms.XamlRoot == null) {
                // TODO: 设置当前活动窗口
                ms.XamlRoot = (App.MainWindow.Content as Frame).XamlRoot;
            }
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
                // TODO: 设置当前活动窗口
                ms.XamlRoot = (App.MainWindow.Content as Frame).XamlRoot;
                dialogShowing = true;
                await ShowContentDialog(ms);
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
            // TODO: 设置当前活动窗口
            ms.XamlRoot = (App.MainWindow.Content as Frame).XamlRoot;
            ms.Show(oid, commentMode, commentSort);
        }
    }
}
