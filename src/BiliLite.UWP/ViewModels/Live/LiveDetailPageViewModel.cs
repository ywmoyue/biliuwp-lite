using Windows.UI.Xaml;
using BiliLite.ViewModels.Common;
using PropertyChanged;

namespace BiliLite.ViewModels.Live
{
    public class LiveDetailPageViewModel : BaseViewModel
    {
        public bool IsPaused { get; set; } = true;

        [DependsOn(nameof(IsPaused))]
        public Visibility ShowResumeBtn => IsPaused ? Visibility.Visible : Visibility.Collapsed;

        [DependsOn(nameof(IsPaused))]
        public Visibility ShowPauseBtn => !IsPaused ? Visibility.Visible : Visibility.Collapsed;
    }
}
