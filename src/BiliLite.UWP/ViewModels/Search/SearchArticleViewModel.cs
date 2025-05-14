using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models;
using BiliLite.Models.Common.Search;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace BiliLite.ViewModels.Search
{
    public class SearchArticleViewModel : BaseSearchPivotViewModel
    {
        public SearchArticleViewModel()
        {
            OrderFilters = new List<SearchFilterItem>() {
                new SearchFilterItem("默认排序","totalrank"),
                new SearchFilterItem("最多阅读","click"),
                new SearchFilterItem("最新发布","pubdate"),
                new SearchFilterItem("最多喜欢","attention"),
                new SearchFilterItem("最多评论","scores")
            };
            SelectOrder = OrderFilters[0];

            RegionFilters = new List<SearchFilterItem>() {
                new SearchFilterItem("全部分区","0"),
                new SearchFilterItem("动画","2"),
                new SearchFilterItem("游戏","1"),
                new SearchFilterItem("影视","28"),
                new SearchFilterItem("生活","3"),
                new SearchFilterItem("兴趣","29"),
                new SearchFilterItem("轻小说","16"),
                new SearchFilterItem("科技","17"),
            };

            SelectRegion = RegionFilters[0];
        }

        [DoNotNotify]
        public List<SearchFilterItem> OrderFilters { get; set; }

        [DoNotNotify]
        public SearchFilterItem SelectOrder { get; set; }

        [DoNotNotify]
        public List<SearchFilterItem> DurationFilters { get; set; }

        [DoNotNotify]
        public List<SearchFilterItem> RegionFilters { get; set; }

        [DoNotNotify]
        public SearchFilterItem SelectRegion { get; set; }

        public ObservableCollection<SearchArticleItem> Articles { get; set; }

        public override async Task LoadData()
        {
            try
            {
                if (Loading)
                {
                    return;
                }
                ShowLoadMore = false;
                Loading = true;
                Nothing = false;
                var results = await SearchApi.WebSearchArticle(Keyword, Page, SelectOrder.value, SelectRegion.value, Area).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        var result = JsonConvert.DeserializeObject<ObservableCollection<SearchArticleItem>>(data.data["result"]?.ToString() ?? "[]");
                        if (Page == 1)
                        {
                            if (result == null || result.Count == 0)
                            {
                                Nothing = true;
                                Articles?.Clear();
                                return;
                            }
                            Articles = result;
                        }
                        else
                        {
                            if (data.data != null)
                            {
                                foreach (var item in result)
                                {
                                    Articles.Add(item);
                                }
                            }
                        }
                        if (Page < data.data["numPages"].ToInt32())
                        {
                            ShowLoadMore = true;
                            Page++;
                        }
                        HasData = true;
                    }
                    else
                    {
                        NotificationShowExtensions.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    NotificationShowExtensions.ShowMessageToast(results.message);
                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<SearchPageViewModel>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }
    }
}
