using Windows.UI;
using BiliLite.Models.Common.User.UserDetails;
using Newtonsoft.Json;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;

namespace BiliLite.Models.Common.User
{
    public class UserSpaceInfo
    {
        public string Name { get; set; }

        public string Mid { get; set; }

        public string Face { get; set; }

        public string Sign { get; set; }

        public UserSpaceInfoVipInfo Vip { get; set; }

        public string TopImageLight { get; set; }

        public string TopImageDark { get; set; }

        public string TopImage => TopImageLight;

        public int Level { get; set; }

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

        public UserCenterInfoPendantModel Pendant { get; set; }

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

        public UserCenterInfoOfficialModel Official { get; set; }

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

        public UserCenterInfoLiveRoomModel LiveRoom { get; set; }
    }

    public class UserSpaceInfoVipInfo
    {
        [JsonProperty("vipType")]
        public int Type { get; set; }

        [JsonProperty("vipStatus")]
        public int Status { get; set; }

        public UserCenterInfoVipLabelModel Label { get; set; }
    }
}
