using Newtonsoft.Json;

namespace BiliLite.Models.Common.User
{
    public class SubmitCollectionItemModel
    {
        /// <summary>
        /// 本合集的视频数量
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 本合集的封面
        /// </summary>
        public string Cover { get; set; }

        /// <summary>
        /// 跳转格式? 常见值: av
        /// </summary>
        public string Goto { get; set; }

        /// <summary>
        /// 可能是收费视频合集?
        /// </summary>
        [JsonProperty("is_pay")]
        public bool IsPay { get; set; }

        /// <summary>
        /// 合集最后更新时间戳
        /// </summary>
        [JsonProperty("mtime")]
        public int LatestUpdateTime { get; set; }

        /// <summary>
        /// 未知参数
        /// 对于直播回放列表为其纯数字列表id.
        /// </summary>
        public string Param { get; set; }

        /// <summary>
        /// 合集标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 合集类型
        /// 目前发现用户自己创建的为season, 直播回放列表为series
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 跳转链接
        /// 以bilibili://开头.
        /// 对于直播回放, 其特别的, 为: bilibili://music/playlist/playpage/{Id}
        /// Id可能为合集的纯数字id.
        /// </summary>
        public string Uri { get; set; }
    }
}
