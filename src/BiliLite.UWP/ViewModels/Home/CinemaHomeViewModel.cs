using System.Collections.Generic;
using BiliLite.Models.Common.Home;

namespace BiliLite.ViewModels.Home
{
    public class CinemaHomeViewModel
    {
        public List<CinemaHomeBannerModel> Banners { get; set; }

        public List<CinemaHomeFallViewModel> Falls { get; set; }

        public List<CinemaHomeHotItem> Update { get; set; }

        /// <summary>
        /// 记录片 87
        /// </summary>
        public List<CinemaHomeHotItem> Documentary { get; set; }

        /// <summary>
        /// 电影 88
        /// </summary>
        public List<CinemaHomeHotItem> Movie { get; set; }

        /// <summary>
        /// 电视剧 89
        /// </summary>
        public List<CinemaHomeHotItem> Tv { get; set; }

        /// <summary>
        /// 综艺 173
        /// </summary>
        public List<CinemaHomeHotItem> Variety { get; set; }
    }
}