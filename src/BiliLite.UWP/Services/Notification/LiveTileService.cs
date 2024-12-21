using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BiliLite.Models.Common.Notifications;

namespace BiliLite.Services.Notification
{
    public class LiveTileService
    {
        public List<NotificationTile> TileFurnace = new List<NotificationTile>();

        public async Task RefreshTile()
        {
            TileFurnace.Clear();
            // TODO: 取热门/动态/推荐数据
            var sample = new NotificationTile()
            {
                Name = "BiliLIte",
                Description = "",
                Url = @"Assets/SplashScreen.png",
            };
            TileFurnace.Add(sample);
        }
    }
}
