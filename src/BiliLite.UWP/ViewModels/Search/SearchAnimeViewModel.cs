using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using BiliLite.Extensions;
using BiliLite.Models;
using BiliLite.Models.Common.Search;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BiliLite.ViewModels.Search
{
    public class SearchAnimeViewModel : BaseSearchPivotViewModel
    {
        #region Properties

        public ObservableCollection<SearchAnimeItem> Animes { get; set; }

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
                var api = SearchApi.WebSearchAnime(Keyword, Page, Area);
                if (this.SearchType == Models.Common.SearchType.Movie)
                {
                    api = SearchApi.WebSearchMovie(Keyword, Page, Area);
                }
                var results = await api.Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {

                        var result = JsonConvert.DeserializeObject<ObservableCollection<SearchAnimeItem>>(data.data["result"]?.ToString() ?? "[]");
                        if (Page == 1)
                        {
                            if (result == null || result.Count == 0)
                            {
                                Nothing = true;
                                Animes?.Clear();
                                return;
                            }
                            Animes = result;
                        }
                        else
                        {
                            if (data.data != null)
                            {
                                foreach (var item in result)
                                {
                                    Animes.Add(item);
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
