using System.Collections.Generic;

namespace BiliLite.Models.Common.Anime
{
    /// <summary>
    /// 番剧/国创首页推荐流：题材榜 + 猜你喜欢卡片，按游标分页
    /// </summary>
    public class AnimeFeedModel
    {
        public List<AnimeFallModel> Sections { get; set; } = new List<AnimeFallModel>();

        /// <summary>
        /// 下一页游标
        /// </summary>
        public int NextCursor { get; set; }

        /// <summary>
        /// 是否还有下一页
        /// </summary>
        public bool HasMore { get; set; }
    }
}
