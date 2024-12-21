using System.Collections.ObjectModel;
using BiliLite.Models.Common.Home;
using BiliLite.ViewModels.Common;
using PropertyChanged;

namespace BiliLite.ViewModels.Home
{
    public class CinemaHomeFallViewModel : BaseViewModel
    {
        [DoNotNotify]
        public int Wid { get; set; }

        [DoNotNotify]
        public string Title { get; set; }

        public bool ShowMore { get; set; } = true;

        [DoNotNotify]
        public ObservableCollection<CinemaHomeFallItemModel> Items { get; set; }
    }
}