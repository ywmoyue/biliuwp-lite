using BiliLite.ViewModels.Common;

namespace BiliLite.ViewModels
{
    public class PlayerToastViewModel : BaseViewModel
    {
        public string Text { get; set; }

        public bool ShowSkipButton { get; set; } = false;
    }
}
