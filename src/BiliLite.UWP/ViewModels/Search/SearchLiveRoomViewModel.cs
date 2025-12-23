using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models;
using BiliLite.Models.Common.Search;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace BiliLite.ViewModels.Search
{
    public class SearchLiveRoomViewModel : BaseSearchPivotViewModel
    {
        #region Properties

        public ObservableCollection<SearchLiveRoomItem> Rooms { get; set; }

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

                var api = await SearchApi.WebSearchLive(Keyword, Page, Area);
                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {

                        var result = JsonConvert.DeserializeObject<ObservableCollection<SearchLiveRoomItem>>(data.data["result"]["live_room"]?.ToString() ?? "[]");
                        if (Page == 1)
                        {
                            if (result == null || result.Count == 0)
                            {
                                Nothing = true;
                                Rooms?.Clear();
                                return;
                            }
                            Rooms = result;
                        }
                        else
                        {
                            if (data.data != null)
                            {
                                foreach (var item in result)
                                {
                                    Rooms.Add(item);
                                }
                            }
                        }
                        if (Page < data.data["pageinfo"]["live_room"]["numPages"].ToInt32())
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

        #endregion
    }
}
