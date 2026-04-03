namespace BiliLite.Models.Common.Notifications
{
    public class NotificationTile
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Url { get; set; }

        /// <summary>
        /// UP主头像 URL，用于磁贴翻转前展示
        /// </summary>
        public string AvatarUrl { get; set; }
    }
}