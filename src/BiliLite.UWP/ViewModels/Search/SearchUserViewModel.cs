using BiliLite.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BiliLite.Extensions;
using BiliLite.Models.Common.Search;
using PropertyChanged;

namespace BiliLite.ViewModels.Search
{
    public class SearchUserViewModel : BaseSearchPivotViewModel
    {
        #region Constructors

        public SearchUserViewModel()
        {
            OrderFilters = new List<SearchFilterItem>() {
                new SearchFilterItem("默认排序","&order=&order_sort="),
                new SearchFilterItem("粉丝数由高到低","&order=fans&order_sort=0"),
                new SearchFilterItem("粉丝数由低到高","&order=fans&order_sort=1"),
                new SearchFilterItem("LV等级由高到低","&order=level&order_sort=0"),
                new SearchFilterItem("LV等级由低到高","&order=level&order_sort=1"),
            };
            SelectOrder = OrderFilters[0];
            TypeFilters = new List<SearchFilterItem>() {
                new SearchFilterItem("全部用户","&user_type=0"),
                new SearchFilterItem("UP主","&user_type=1"),
                new SearchFilterItem("普通用户","&user_type=2"),
                new SearchFilterItem("认证用户","&user_type=3")
            };
            SelectType = TypeFilters[0];

        }

        #endregion

        #region Properties

        [DoNotNotify]
        public List<SearchFilterItem> OrderFilters { get; set; }

        [DoNotNotify]
        public SearchFilterItem SelectOrder { get; set; }

        [DoNotNotify]
        public List<SearchFilterItem> TypeFilters { get; set; }

        [DoNotNotify]
        public SearchFilterItem SelectType { get; set; }

        public ObservableCollection<SearchUserItem> Users { get; set; }

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
                var results = await SearchApi.WebSearchUser(Keyword, Page, SelectOrder.value, SelectType.value, Area).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        var result = JsonConvert.DeserializeObject<ObservableCollection<SearchUserItem>>(data.data["result"]?.ToString() ?? "[]");
                        if (Page == 1)
                        {
                            if (result == null || result.Count == 0)
                            {
                                Nothing = true;
                                Users?.Clear();
                                return;
                            }
                            Users = result;
                        }
                        else
                        {
                            if (data.data != null)
                            {
                                foreach (var item in result)
                                {
                                    Users.Add(item);
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
                        Notify.ShowMessageToast(data.message);
                    }
                }
                else
                {
                    Notify.ShowMessageToast(results.message);
                }
            }
            catch (Exception ex)
            {
                var handel = HandelError<SearchPageViewModel>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

        #endregion

    }
}
