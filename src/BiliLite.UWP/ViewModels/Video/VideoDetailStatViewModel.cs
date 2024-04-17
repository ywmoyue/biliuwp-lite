using BiliLite.ViewModels.Common;
using Newtonsoft.Json;

namespace BiliLite.ViewModels.Video
{
    public class VideoDetailStatViewModel : BaseViewModel
    {
        public string Aid { get; set; }

        /// <summary>
        /// 播放
        /// </summary>
        public long View { get; set; }

        /// <summary>
        /// 弹幕
        /// </summary>
        public long Danmaku { get; set; }

        /// <summary>
        /// 评论
        /// </summary>
        public long Reply { get; set; }
        
        /// <summary>
        /// 收藏
        /// </summary>
        public long Favorite { get; set; }
        
        /// <summary>
        /// 投币
        /// </summary>
        public long Coin { get; set; }
        
        /// <summary>
        /// 分享
        /// </summary>
        public long Share { get; set; }
        
        /// <summary>
        /// 点赞
        /// </summary>
        public long Like { get; set; }

        /// <summary>
        /// 不喜欢，固定0
        /// </summary>
        [JsonProperty("dislike")]
        public long DisLike { get; set; }
    }
}