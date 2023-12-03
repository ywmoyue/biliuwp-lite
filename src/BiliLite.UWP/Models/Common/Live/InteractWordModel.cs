using Windows.UI.Xaml;

namespace BiliLite.Models.Common.Live
{
    public class InteractWordModel
    {
        public string UserName { get; set; }

        public string UserID { get; set; }
        
        public int MsgType { get; set; }

        public string MedalName { get; set; }

        public string MedalLevel { get; set; }

        public string MedalColor { get; set; }

        public Visibility ShowMedal { get; set; } = Visibility.Collapsed;
    }
}