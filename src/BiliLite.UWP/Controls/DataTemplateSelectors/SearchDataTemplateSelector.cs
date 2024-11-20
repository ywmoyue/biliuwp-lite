using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BiliLite.Models.Common;
using BiliLite.ViewModels.Search;

namespace BiliLite.Controls.DataTemplateSelectors
{
    public class SearchDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate VideoTemplate { get; set; }
        public DataTemplate AnimeTemplate { get; set; }
        public DataTemplate TestTemplate { get; set; }
        public DataTemplate LiveRoomTemplate { get; set; }
        public DataTemplate UserTemplate { get; set; }
        public DataTemplate ArticTemplate { get; set; }
        public DataTemplate TopicTemplate { get; set; }
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var data = item as ISearchPivotViewModel;
            switch (data.SearchType)
            {
                case SearchType.Video:
                    return VideoTemplate;
                case SearchType.Anime:
                case SearchType.Movie:
                    return AnimeTemplate;
                case SearchType.User:
                    return UserTemplate;
                case SearchType.Live:
                    return LiveRoomTemplate;
                case SearchType.Article:
                    return ArticTemplate;
                case SearchType.Topic:
                    return TopicTemplate;
                case SearchType.Anchor:
                    return TestTemplate;
                default:
                    return TestTemplate;
            }


        }
    }
}