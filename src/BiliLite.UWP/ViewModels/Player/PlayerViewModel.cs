using BiliLite.Player.States.PlayStates;
using BiliLite.ViewModels.Common;

namespace BiliLite.ViewModels.Player
{
    public class PlayerViewModel : BaseViewModel
    {
        public IPlayState PlayState { get; set; }

        public double Position { get; set; }

        public double Duration { get; set; }
    }
}
