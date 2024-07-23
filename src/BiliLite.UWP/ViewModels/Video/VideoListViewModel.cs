using System.Collections.ObjectModel;
using BiliLite.Models.Common.Video;
using BiliLite.ViewModels.Common;
using PropertyChanged;

namespace BiliLite.ViewModels.Video
{
    public class VideoListViewModel : BaseViewModel
    {
        public ObservableCollection<VideoListSectionViewModel> Sections { get; set; }

        [DoNotNotify]
        public VideoListItem LastSelectedItem { get; set; }
    }
}
