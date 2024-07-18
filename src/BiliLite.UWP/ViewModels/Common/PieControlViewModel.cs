namespace BiliLite.ViewModels.Common
{

    public class PieControlViewModel : BaseViewModel
    {
        public double LittleBtnWidth => 50;

        public double LittleBtnHeight => 50;

        public double PieExtendGridLittleWidth = 25;

        public double PieExtendGridLittleHeight = 50;

        public double PieExtendGridExtendWidth = 80;

        public double PieExtendGridExtendHeight = 160;

        public bool ExtendVisibility { get; set; } = false;
    }
}
