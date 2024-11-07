using System.Collections.Generic;
using System.Collections.ObjectModel;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Search;
using BiliLite.ViewModels.Common;
using PropertyChanged;

namespace BiliLite.ViewModels.Search
{
    public class SearchPageViewModel : BaseViewModel
    {
        #region Constructors

        public SearchPageViewModel()
        {
        }

        #endregion

        #region Properties

        public ObservableCollection<ISearchPivotViewModel> SearchItems { get; set; }

        [DoNotNotify]
        public List<SearchArea> Areas { get; set; } = new List<SearchArea>()
        {
            new SearchArea("默认地区",""),
            new SearchArea("大陆地区","cn"),
            new SearchArea("香港地区","hk"),
            new SearchArea("台湾地区","tw"),
        };
        
        [DoNotNotify]
        public SearchArea Area { get; set; }

        [DoNotNotify]
        public ISearchPivotViewModel SelectItem { get; set; }

        public ObservableCollection<string> SuggestSearchContents { get; set; }

        public int PivotIndex { get; set; }

        public int ComboIndex { get; set; }

        #endregion

        #region Public Methods

        public void Init(int pivotIndex, int comboIndex)
        {
            Area = Areas[comboIndex];
            SearchItems = new ObservableCollection<ISearchPivotViewModel>() {
                new SearchVideoViewModel()
                {
                    Title="视频",
                    SearchType= SearchType.Video,
                    Area= Area.area
                },
                new SearchAnimeViewModel()
                {
                    Title="番剧",
                    SearchType= SearchType.Anime,
                    Area= Area.area
                },
                new SearchLiveRoomViewModel()
                {
                    Title="直播",
                    SearchType= SearchType.Live,
                    Area= Area.area
                },
                //new SearchLiveRoomVM()
                //{
                //    Title="主播",
                //    SearchType= SearchType.Anchor 
                //},
                new SearchUserViewModel()
                {
                    Title="用户",
                    SearchType= SearchType.User,
                    Area= Area.area
                },
                new SearchAnimeViewModel()
                {
                    Title="影视",
                    SearchType= SearchType.Movie,
                    Area= Area.area
                },
                new SearchArticleViewModel()
                {
                    Title="专栏",
                    SearchType= SearchType.Article,
                    Area= Area.area
                },
                new SearchTopicViewModel()
                {
                    Title="话题",
                    SearchType= SearchType.Topic,
                    Area= Area.area
                }
            };
            SelectItem = SearchItems[pivotIndex];
        }

        #endregion
    }
}
