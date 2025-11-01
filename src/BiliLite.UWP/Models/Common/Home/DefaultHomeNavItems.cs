using BiliLite.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BiliLite.Models.Common.Home
{
    public static class DefaultHomeNavItems
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        public static List<HomeNavItem> CheckHomeNavItems(List<HomeNavItem> navList)
        {
            var defaultItems = GetDefaultHomeNavItems();
            defaultItems.AddRange(GetDefaultHideHomeNavItems());
            var result = new List<HomeNavItem>(navList);
            foreach (var homeNavItem in navList.Where(homeNavItem =>
                         defaultItems.All(x => x.Title != homeNavItem.Title || x.Page != homeNavItem.Page)))
            {
                result.Remove(homeNavItem);
            }
            SettingService.SetValue(SettingConstants.UI.HOEM_ORDER, result);

            return result;
        }

        public static List<HomeNavItem> GetHomeNavItems()
        {
            var homeNavItemList = new List<HomeNavItem>();
            var tempHomeNavItemList = SettingService.GetValue<List<object>>(SettingConstants.UI.HOEM_ORDER,
                null);

            if (tempHomeNavItemList == null)
            {
                homeNavItemList = DefaultHomeNavItems.GetDefaultHomeNavItems();
                return homeNavItemList;
            }
            else
            {
                foreach (var item in tempHomeNavItemList)
                {
                    try
                    {
                        var navItem = JsonConvert.DeserializeObject<HomeNavItem>(JsonConvert.SerializeObject(item));
                        homeNavItemList.Add(navItem);
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(ex.Message, ex);
                    }
                }
            }
            homeNavItemList = DefaultHomeNavItems.CheckHomeNavItems(homeNavItemList);
            return homeNavItemList;
        }

        public static List<HomeNavItem> GetDefaultHomeNavItems()
        {
            return new List<HomeNavItem>()
            {
                new HomeNavItem()
                {
                    Icon = FontAwesome5.EFontAwesomeIcon.Solid_Home,
                    Page = typeof(Pages.Home.RecommendPage),
                    Title = "推荐",
                    NeedLogin = false,
                    Show = true
                },
                new HomeNavItem()
                {
                    Icon = FontAwesome5.EFontAwesomeIcon.Solid_Fire,
                    Page = typeof(Pages.Home.HotPage),
                    Title = "热门",
                    NeedLogin = false,
                    Show = true
                },
                new HomeNavItem()
                {
                    Icon = FontAwesome5.EFontAwesomeIcon.Solid_Heart,
                    Page = typeof(Pages.Home.UserDynamicPage),
                    Title = "动态",
                    NeedLogin = true,
                    Show = false
                },
                new HomeNavItem()
                {
                    Icon = FontAwesome5.EFontAwesomeIcon.Solid_Heart,
                    Page = typeof(Pages.Home.DynamicPage),
                    Title = "视频动态",
                    NeedLogin = true,
                    Show = false
                },
                new HomeNavItem()
                {
                    Icon = FontAwesome5.EFontAwesomeIcon.Solid_Paw,
                    Page = typeof(Pages.Home.AnimePage),
                    Title = "番剧",
                    Parameters = AnimeType.Bangumi,
                    NeedLogin = false,
                    Show = true
                },
                new HomeNavItem()
                {
                    Icon = FontAwesome5.EFontAwesomeIcon.Solid_Feather,
                    Page = typeof(Pages.Home.AnimePage),
                    Title = "国创",
                    Parameters = AnimeType.GuoChuang,
                    NeedLogin = false,
                    Show = true
                },
                new HomeNavItem()
                {
                    Icon = FontAwesome5.EFontAwesomeIcon.Solid_Video,
                    Page = typeof(Pages.Home.LivePage),
                    Title = "直播",
                    NeedLogin = false,
                    Show = true
                },
                new HomeNavItem()
                {
                    Icon = FontAwesome5.EFontAwesomeIcon.Solid_Film,
                    Page = typeof(Pages.Home.MoviePage),
                    Title = "放映厅",
                    NeedLogin = false,
                    Show = true
                },
                new HomeNavItem()
                {
                    Icon = FontAwesome5.EFontAwesomeIcon.Solid_Shapes,
                    Page = typeof(Pages.Home.RegionsPage),
                    Title = "分区",
                    NeedLogin = false,
                    Show = true
                },
                //new HomeNavItem(){
                //    Icon=FontAwesome5.EFontAwesomeIcon.Solid_Bars,
                //    Page=typeof(Pages.Home.ChannelPage),
                //    Title="频道",
                //    NeedLogin=false,
                //    Show=true
                //},
                new HomeNavItem()
                {
                    Icon = FontAwesome5.EFontAwesomeIcon.Solid_Trophy,
                    Page = typeof(Pages.RankPage),
                    Title = "排行榜",
                    NeedLogin = false,
                    Show = true
                },
            };
        }

        public static List<HomeNavItem> GetDefaultHideHomeNavItems()
        {
            return new List<HomeNavItem>
            {
                new HomeNavItem()
                {
                    Icon = FontAwesome5.EFontAwesomeIcon.Solid_File,
                    Page = typeof(Pages.NewPage),
                    Title = "新标签页",
                    NeedLogin = false,
                    Show = true
                },
                new HomeNavItem()
                {
                    Icon = FontAwesome5.EFontAwesomeIcon.Regular_PlayCircle,
                    Page = typeof(Pages.User.WatchlaterPage),
                    Title = "稍后再看",
                    NeedLogin = true,
                    Show = false
                },
                new HomeNavItem()
                {
                    Icon = FontAwesome5.EFontAwesomeIcon.Solid_History,
                    Page = typeof(Pages.User.HistoryPage),
                    Title = "历史记录",
                    NeedLogin = true,
                    Show = false
                },
                new HomeNavItem()
                {
                    Icon = FontAwesome5.EFontAwesomeIcon.Solid_Tv,
                    Page = typeof(Pages.Live.LiveCenterPage),
                    Title = "直播中心",
                    NeedLogin = true,
                    Show = false
                },
                new HomeNavItem()
                {
                    Icon = FontAwesome5.EFontAwesomeIcon.Regular_Star,
                    Page = typeof(Pages.User.FavoritePage),
                    Title = "我的收藏",
                    NeedLogin = true,
                    Show = false
                },
            };
        }
    }
}
