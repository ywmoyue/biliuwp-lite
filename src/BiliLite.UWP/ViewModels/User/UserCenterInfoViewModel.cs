using Windows.UI;
using Windows.UI.Xaml.Media;
using BiliLite.Models.Common;
using BiliLite.Models.Common.User.UserDetails;
using BiliLite.ViewModels.Common;
using PropertyChanged;

namespace BiliLite.ViewModels.User
{
    public class UserCenterInfoViewModel : BaseViewModel
    {
        [DoNotNotify]
        public long Mid { get; set; }

        [DoNotNotify]
        public string Name { get; set; }

        [DoNotNotify]
        public string Sex { get; set; }

        [DoNotNotify]
        public string Face { get; set; }

        [DoNotNotify]
        public string Sign { get; set; }

        [DoNotNotify]
        public int Rank { get; set; }

        [DoNotNotify]
        public int Level { get; set; }

        [DoNotNotify]
        public SolidColorBrush LevelColor =>
            Level switch
            {
                2 => new SolidColorBrush(Colors.LightGreen),
                3 => new SolidColorBrush(Colors.LightBlue),
                4 => new SolidColorBrush(Colors.Yellow),
                5 => new SolidColorBrush(Colors.Orange),
                6 => new SolidColorBrush(Colors.Red),
                7 => new SolidColorBrush(Colors.HotPink),
                8 => new SolidColorBrush(Colors.Purple),
                _ => new SolidColorBrush(Colors.Gray)
            };

        [DoNotNotify]
        public int JoinTime { get; set; }

        [DoNotNotify]
        public int Moral { get; set; }

        [DoNotNotify]
        public int Silence { get; set; }

        [DoNotNotify]
        public string Birthday { get; set; }

        [DoNotNotify]
        public double Coins { get; set; }

        [DoNotNotify]
        public bool FansBadge { get; set; }

        [DoNotNotify]
        public UserCenterInfoOfficialModel Official { get; set; }

        [DoNotNotify]
        public UserCenterInfoVipModel Vip { get; set; }

        [DoNotNotify]
        public UserCenterInfoPendantModel Pendant { get; set; }

        [DoNotNotify]
        public UserCenterInfoNameplateModel NamePlate { get; set; }

        public bool IsFollowed { get; set; }

        [DoNotNotify]
        public string TopPhoto { get; set; }

        [DoNotNotify]
        public UserCenterInfoLiveRoomModel LiveRoom { get; set; }

        [DoNotNotify]
        public UserCenterSpaceStatModel Stat { get; set; }

        [DoNotNotify]
        public string PendantStr
        {
            get
            {
                if (Pendant != null)
                {
                    return Pendant.Image == "" ? Constants.App.TRANSPARENT_IMAGE : Pendant.Image;
                }
                else
                {
                    return Constants.App.TRANSPARENT_IMAGE;
                }
            }
        }

        [DoNotNotify]
        public string Verify
        {
            get
            {
                if (Official == null)
                {
                    return "";
                }
                switch (Official.Type)
                {
                    case 0:
                        return Constants.App.VERIFY_PERSONAL_IMAGE;
                    case 1:
                        return Constants.App.VERIFY_OGANIZATION_IMAGE;
                    default:
                        return Constants.App.TRANSPARENT_IMAGE;
                }
            }
        }
    }
}