using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models;
using BiliLite.Models.Common.Search;
using BiliLite.Models.Exceptions;
using BiliLite.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace BiliLite.ViewModels.Search
{
    public class SearchVideoViewModel : BaseSearchPivotViewModel
    {
        #region Fields

        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly ContentFilterService m_contentFilterService;

        #endregion

        #region Constructors

        public SearchVideoViewModel()
        {
            m_contentFilterService = App.ServiceProvider.GetRequiredService<ContentFilterService>();
            OrderFilters = new List<SearchFilterItem>() {
                new SearchFilterItem("综合排序",""),
                new SearchFilterItem("最多点击","click"),
                new SearchFilterItem("最新发布","pubdate"),
                new SearchFilterItem("最多弹幕","dm"),
                new SearchFilterItem("最多收藏","stow")
            };
            SelectOrder = OrderFilters[0];
            DurationFilters = new List<SearchFilterItem>() {
                new SearchFilterItem("全部时长",""),
                new SearchFilterItem("10分钟以下","1"),
                new SearchFilterItem("10-30分钟","2"),
                new SearchFilterItem("30-60分钟","3"),
                new SearchFilterItem("60分钟以上","4")
            };
            SelectDuration = DurationFilters[0];
            RegionFilters = new List<SearchFilterItem>() {
                new SearchFilterItem("全部分区","0"),
            };
            foreach (var item in AppHelper.Regions.Where(x => x.Children != null && x.Children.Count != 0))
            {
                RegionFilters.Add(new SearchFilterItem(item.Name, item.Tid.ToString()));
            }
            SelectRegion = RegionFilters[0];
        }

        #endregion

        #region Properties


        [DoNotNotify]
        public List<SearchFilterItem> OrderFilters { get; set; }

        [DoNotNotify]
        public SearchFilterItem SelectOrder { get; set; }

        [DoNotNotify]
        public List<SearchFilterItem> DurationFilters { get; set; }

        [DoNotNotify]
        public SearchFilterItem SelectDuration { get; set; }

        [DoNotNotify]
        public List<SearchFilterItem> RegionFilters { get; set; }

        [DoNotNotify]
        public SearchFilterItem SelectRegion { get; set; }

        public ObservableCollection<SearchVideoItem> Videos { get; set; }

        #endregion

        #region Public Methods

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
                var api = await SearchApi.WebSearchVideo(Keyword, Page, SelectOrder.value, SelectDuration.value, SelectRegion.value, Area);
                var results = await api.Request();
                if (!results.status)
                {
                    throw new CustomizedErrorException(results.message);
                }
                var data = await results.GetJson<ApiDataModel<JObject>>();
                if (!data.success)
                {
                    throw new CustomizedErrorException(data.message);
                }
                var searchVideoItems = JsonConvert.DeserializeObject<List<SearchVideoItem>>(data.data["result"]?.ToString() ?? "[]");
                searchVideoItems = m_contentFilterService.FilterSearchItems(searchVideoItems);
                var result = new ObservableCollection<SearchVideoItem>(searchVideoItems);

                if (Page == 1)
                {
                    if (result == null || result.Count == 0)
                    {
                        Nothing = true;
                        Videos?.Clear();
                        return;
                    }
                    Videos = result;
                }
                else if (data.data != null)
                {
                    foreach (var item in result)
                    {
                        Videos.Add(item);
                    }
                }
                if (Page < data.data["numPages"].ToInt32())
                {
                    ShowLoadMore = true;
                    Page++;
                }
                HasData = true;

            }
            catch (Exception ex)
            {
                if (ex is CustomizedErrorException customizedErrorException)
                {
                    NotificationShowExtensions.ShowMessageToast(ex.Message);
                    _logger.Error("搜索失败", ex);
                }

                var handel = HandelError<SearchPageViewModel>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

        #endregion
    }
}
