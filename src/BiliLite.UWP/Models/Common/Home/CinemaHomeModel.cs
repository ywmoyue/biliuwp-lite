using System.Collections.Generic;

namespace BiliLite.Models.Common.Home
{
    public class CinemaHomeModel
    {
        public List<CinemaHomeBannerModel> Banners { get; set; }

        public List<CinemaHomeFallModel> Falls { get; set; }

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