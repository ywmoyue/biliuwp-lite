using System.Collections.Generic;
using BiliLite.Modules;

namespace BiliLite.Models.Common.Anime
{
    public class AnimeHomeModel
    {
        public List<AnimeRankModel> Hots { get; set; }

        public List<AnimeBannerModel> Banners { get; set; }

        public List<AnimeRankModel> Ranks { get; set; }

        public List<AnimeTimelineItemModel> Today { get; set; }

        public List<AnimeFallModel> Falls { get; set; }

        /// <summary>
        /// 推荐流下一页游标
        /// </summary>
        public int FeedCursor { get; set; }

        /// <summary>
        /// 推荐流是否还有下一页
        /// </summary>
        public bool HasMoreFeed { get; set; }
    }
}