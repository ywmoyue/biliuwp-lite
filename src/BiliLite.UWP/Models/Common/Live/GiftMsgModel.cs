namespace BiliLite.Models.Common.Live
{
    public class GiftMsgModel
    {
        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 礼物的名称
        /// </summary>
        public string GiftName { get; set; }

        /// <summary>
        /// 礼物操作
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// 礼物数量
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// 礼物ID
        /// </summary>
        public int GiftId { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// GIF图片
        /// </summary>
        public string Gif { get; set; }
    }
}