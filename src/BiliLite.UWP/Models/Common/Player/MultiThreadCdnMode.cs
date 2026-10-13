namespace BiliLite.Models.Common.Player
{
    /// <summary>
    /// 多线程下载使用的节点
    /// </summary>
    public enum MultiThreadCdnMode
    {
        /// <summary>
        /// 大陆节点：海外观看冷门、高码率视频时，海外边缘节点没有缓存，直接去大陆节点取
        /// </summary>
        Mainland = 0,

        /// <summary>
        /// 海外节点：本地连大陆网络很差时使用
        /// </summary>
        Overseas = 1,

        /// <summary>
        /// 只用接口给的原始地址和备用地址
        /// </summary>
        Original = 2,
    }
}
