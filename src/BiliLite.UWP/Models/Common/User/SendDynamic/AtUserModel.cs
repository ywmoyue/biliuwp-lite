namespace BiliLite.Models.Common.User.SendDynamic
{
    public class AtUserModel
    {
        public long ID { get; set; }
        public string UserName { get; set; }
        public string Face { get; set; }
        public string Display { get { return "@" + UserName; } }
    }
}