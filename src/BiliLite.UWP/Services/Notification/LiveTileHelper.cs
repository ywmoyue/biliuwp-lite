using BiliLite.Models;
using System.Collections.Generic;

namespace BiliLite.Services.Notification
{
    internal class LiveTileHelper
    {
        internal List<TileModel> TileFurnace = new List<TileModel>();

        internal void FireTile()
        {
            // 最多可以添加5组数据，可以是推荐视频，也可以是动态视频
            TileModel sample = new TileModel()
            {
                Name = "ProJend",
                Description = "Hello World.",
                Url = @"Assets/SplashScreen.png",
                Url = @"https://bilibili.com/image.png",
            };
            TileFurnace.Add(sample);
        }
    }
}
