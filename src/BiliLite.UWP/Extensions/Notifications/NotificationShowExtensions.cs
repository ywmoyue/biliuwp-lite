using System.Threading.Tasks;
using Windows.UI.Notifications;
using BiliLite.Models.Common.Notifications.Template;
using BiliLite.Services.Notification;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLite.Extensions.Notifications
{
    public class NotificationShowExtensions
    {
        public static async Task Tile()
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

        public static void Dialog()
        {

        }

        public static void MessageDialog()
        {

        }

        public static void Toast()
        {

        }
    }
}
