using BiliLite.ViewModels.Common;
using PropertyChanged;

namespace BiliLite.ViewModels.User
{
    public class UserRelationFollowingTagViewModel : BaseViewModel
    {
        [DoNotNotify]
        public long TagId { get; set; }

        [DoNotNotify]
        public string Name { get; set; }

        [DoNotNotify]
        public int Count { get; set; }

        [DoNotNotify]
        public string Tip { get; set; }

        public bool UserInThisTag { get; set; }
    }
}
