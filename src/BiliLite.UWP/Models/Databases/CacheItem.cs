using System.ComponentModel.DataAnnotations;

namespace BiliLite.Models.Databases
{
    /// <summary>
    /// 通用缓存项：按 Key 存 JSON，带过期时间
    /// </summary>
    public class CacheItem
    {
        [Key]
        public string Key { get; set; }

        public string Value { get; set; }

        /// <summary>
        /// 过期时间（UTC 毫秒时间戳）
        /// </summary>
        public long ExpireAt { get; set; }
    }
}
