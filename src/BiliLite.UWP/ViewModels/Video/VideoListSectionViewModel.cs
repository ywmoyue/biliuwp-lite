using System;
using System.Collections.ObjectModel;
using System.Linq;
using BiliLite.Converters;
using BiliLite.Models.Common.Video;
using BiliLite.ViewModels.Common;
using PropertyChanged;

namespace BiliLite.ViewModels.Video
{
    public class VideoListSectionViewModel : BaseViewModel
    {
        public long Id { get; set; }

        public string Title { get; set; }

        [DoNotNotify]
        public string Info { get; set; }

        [DoNotNotify]
        public string InfoStr
        {
            get
            {
                if (IsLazyOnlineList && !string.IsNullOrEmpty(Info)) return Info;
                var duration = Items.Where(item => item.Duration != null)
                    .Aggregate(TimeSpan.Zero, (current, item) => current + item.Duration.Value);
                var result = $"共{Items.Count}集";
                if (duration != TimeSpan.Zero)
                {
                    result += $" - {TimeSpanStrFormatConverter.Convert(duration)}";
                }

                if (!string.IsNullOrEmpty(Info))
                {
                    result += $" - {Info}";
                }

                return result;
            }
        }

        [DoNotNotify]
        public bool ShowInfo { get; set; } = true;

        [DoNotNotify]
        public string Description { get; set; }

        [DoNotNotify]
        public bool ShowDescription => !string.IsNullOrEmpty(Description);

        public bool Selected { get; set; }

        public ObservableCollection<VideoListItem> Items { get; set; }

        public VideoListItem SelectedItem { get; set; }

        public bool IsLazyOnlineList { get; set; }

        [DoNotNotify]
        public string OnlineListId { get; set; }

        [DoNotNotify]
        public int OnlineListType { get; set; } = 1;
    }
}