namespace BiliLite.Models.Common.Live
{
    public class RoomChangeMsgModel
    {
        /// <summary>
        /// 房间标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 分区ID
        /// </summary>
        public int AreaID { get; set; }

        /// <summary>
        /// 父分区ID
        /// </summary>
        public int ParentAreaID { get; set; }

        /// <summary>
        /// 分区名称
        /// </summary>
        public string AreaName { get; set; }

        /// <summary>
        /// 父分区名称
        /// </summary>
        public string ParentAreaName { get; set; }

        /// <summary>
        /// 未知
        /// </summary>
        public string LiveKey { get; set; }

        /// <summary>
        /// 未知
        /// </summary>
        public string SubSessionKey { get; set; }
    }
}