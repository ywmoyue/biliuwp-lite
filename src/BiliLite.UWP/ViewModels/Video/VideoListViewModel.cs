using System.Collections.ObjectModel;
using BiliLite.ViewModels.Common;

namespace BiliLite.ViewModels.Video
{
    public class VideoListViewModel : BaseViewModel
    {
        public ObservableCollection<VideoListSectionViewModel> Sections { get; set; }
    }
}
