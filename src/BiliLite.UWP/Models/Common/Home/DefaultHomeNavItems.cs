using System.Collections.Generic;

namespace BiliLite.Models.Common.Home
{
    public static class DefaultHomeNavItems
    {
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
                new HomeNavItem()
                {
                    Icon = FontAwesome5.EFontAwesomeIcon.Solid_Compass,
                    Page = typeof(Pages.Other.FindMorePage),
                    Title = "发现",
                    NeedLogin = false,
                    Show = true
                }
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
                }
            };
        }
    }
}
