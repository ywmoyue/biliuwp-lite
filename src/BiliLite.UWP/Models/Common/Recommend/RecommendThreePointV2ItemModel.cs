using System.Collections.Generic;
using BiliLite.Modules;

namespace BiliLite.Models.Common.Recommend
{
    public class RecommendThreePointV2ItemModel
    {
        public string Idx { get; set; }

        public string Title { get; set; }

        public string Type { get; set; }

        public string Subtitle { get; set; }

        public string Url { get; set; }

        public string UpId { get; set; }

        /// <summary>
        /// 「不感兴趣」的理由 id(仅网页版自己拼的菜单项用)。
        /// 官方首页写死两个: 1 = 内容不感兴趣, 4 = 不想看此UP主。
        /// </summary>
        public int ReasonId { get; set; }

        public List<RecommendThreePointV2ItemReasonsModel> Reasons { get; set; }
    }
}