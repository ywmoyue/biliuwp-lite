using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiliLite.Models.Common.Notifications;
using BiliLite.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLite.Services.Notification
{
    public class LiveTileService
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private const int MaxTileCount = 5;

        public List<NotificationTile> TileFurnace = new List<NotificationTile>();

        public async Task RefreshTile()
        {
            TileFurnace.Clear();
            try
            {
                var grpcService = App.ServiceProvider.GetRequiredService<GrpcService>();
                var dynVideoReply = await grpcService.GetDynVideo(1, null, null);
                var items = dynVideoReply?.DynamicList?.List;
                if (items != null && items.Count > 0)
                {
                    foreach (var item in items.Take(MaxTileCount))
                    {
                        var archive = item.Modules
                            ?.FirstOrDefault(m => m.ModuleType == Bilibili.App.Dynamic.V2.DynModuleType.ModuleDynamic)
                            ?.ModuleDynamic?.DynArchive;
                        var authorModule = item.Modules
                            ?.FirstOrDefault(m => m.ModuleType == Bilibili.App.Dynamic.V2.DynModuleType.ModuleAuthor)
                            ?.ModuleAuthor;

                        if (archive == null) continue;

                        TileFurnace.Add(new NotificationTile()
                        {
                            Name = archive.Title,
                            Description = authorModule?.Author?.Name ?? "",
                            Url = archive.Cover,
                            AvatarUrl = authorModule?.Author?.Face ?? "",
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("获取动态视频数据失败", ex);
            }

            if (TileFurnace.Count == 0)
            {
                TileFurnace.Add(new NotificationTile()
                {
                    Name = "哔哩哔哩",
                    Description = "",
                    Url = @"Assets/SplashScreen.png",
                });
            }
        }
    }
}

