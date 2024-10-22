namespace BiliLite.Services.Notification
{
    public class Show
    {
        public static void Tile()
        {
            // Create a tile update manager for the specified syndication feed.
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            updater.Clear();
            updater.EnableNotificationQueue(true);

            var liveTileHelper = new LiveTileHelper();
            liveTileHelper.FireTile();

            for (var i = 0; i < 5; i++)
            {
                var content = TileTemplate.LiveTile(liveTileHelper.TileFurnace[i]);
                var notification = new TileNotification(content.GetXml());
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
