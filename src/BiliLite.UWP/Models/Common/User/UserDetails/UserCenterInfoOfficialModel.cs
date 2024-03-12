namespace BiliLite.Models.Common.User.UserDetails
{
    public class UserCenterInfoOfficialModel
    {
        public int Role { get; set; }

        public string Title { get; set; }

        public string Desc { get; set; }

        public int Type { get; set; }

        public bool ShowOfficial => Type != -1;
    }
}