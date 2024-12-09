using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BiliLite.Extensions;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common.Season;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api;
using BiliLite.Modules;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyChanged;

namespace BiliLite.ViewModels.Season
{
    [RegisterTransientViewModel]
    public class SeasonIndexViewModel : BaseViewModel
    {
        #region Fields

        private readonly SeasonIndexAPI m_seasonIndexApi;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private bool m_canLoadMore = true;

        #endregion

        #region Constructors

        public SeasonIndexViewModel()
        {
            m_seasonIndexApi = new SeasonIndexAPI();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
        }

        #endregion

        #region Properties

        [DoNotNotify]
        public ICommand RefreshCommand { get; private set; }

        [DoNotNotify]
        public ICommand LoadMoreCommand { get; private set; }

        [DoNotNotify]
        public SeasonIndexParameter Parameter { get; set; }

        public bool Loading { get; set; } = true;

        public bool ConditionsLoading { get; set; } = true;

        public ObservableCollection<SeasonIndexConditionFilterModel> Conditions { get; set; }

        public ObservableCollection<SeasonIndexResultItemModel> Result { get; set; }

        public int Page { get; set; }

        #endregion

        #region Private Methods

        private void LoadConditionsCore(JObject data)
        {
            var items =
                JsonConvert.DeserializeObject<ObservableCollection<SeasonIndexConditionFilterModel>>(
                    data["data"]["filter"].ToString());
            foreach (var item in items)
            {
                item.Current = item.Field switch
                {
                    "style_id" => item.Values.FirstOrDefault(x => x.Keyword == Parameter.Style),
                    "area" => item.Values.FirstOrDefault(x => x.Keyword == Parameter.Area),
                    "pub_date" => item.Values.FirstOrDefault(x => x.Keyword == Parameter.Year),
                    "season_month" => item.Values.FirstOrDefault(x => x.Keyword == Parameter.Month),
                    _ => item.Values.FirstOrDefault()
                };
            }

            var orders = new List<SeasonIndexConditionFilterItemModel>();

            foreach (var item in data["data"]["order"])
            {
                orders.Add(new SeasonIndexConditionFilterItemModel()
                {
                    Keyword = item["field"].ToString(),
                    Name = item["name"].ToString()
                });
            }

            items.Insert(0, new SeasonIndexConditionFilterModel()
            {
                Name = "排序",
                Values = orders,
                Field = "order",
                Current = orders.FirstOrDefault(x => x.Name == Parameter.Order) ?? orders[0],
            });
            Conditions = items;
        }

        private void LoadResultCore(List<SeasonIndexResultItemModel> items)
        {
            if (items != null && items.Count != 0)
            {
                if (Page == 1)
                {
                    Result = new ObservableCollection<SeasonIndexResultItemModel>(items);
                }
                else
                {
                    foreach (var item in items)
                    {
                        Result.Add(item);
                    }
                }

                Page++;
            }
            else
            {
                m_canLoadMore = false;
                Notify.ShowMessageToast("加载完了");
            }
        }

        #endregion

        #region Public Methods

        public async Task LoadConditions()
        {
            try
            {
                ConditionsLoading = true;
                var results = await m_seasonIndexApi.Condition((int)Parameter.Type).Request();
                if (!results.status) throw new CustomizedErrorException(results.message);
                var data = results.GetJObject();
                if (data["code"].ToInt32() != 0) throw new CustomizedErrorException(data["message"].ToString());
                LoadConditionsCore(data);
            }
            catch (CustomizedErrorException ex)
            {
                Notify.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                var handel = HandelError<SeasonIndexConditionFilterModel>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                ConditionsLoading = false;
            }
        }

        public async Task LoadResult()
        {
            try
            {
                if (Loading) return;

                if (Page == 1)
                {
                    m_canLoadMore = true;
                    Result = null;
                }
                else
                {
                    if (!m_canLoadMore)
                    {
                        Loading = false;
                        return;
                    }
                }
                Loading = true;
                var con = "";
                foreach (var item in Conditions)
                {
                    con += $"&{item.Field}={Uri.EscapeDataString(item.Current.Keyword)}";
                }
                con += $"&sort=0";
                var results = await m_seasonIndexApi.Result(Page, (int)Parameter.Type, con).Request();
                if (!results.status) throw new CustomizedErrorException(results.message);
                var data = results.GetJObject();
                if (data["code"].ToInt32() != 0) throw new CustomizedErrorException(data["message"].ToString());
                var items = JsonConvert.DeserializeObject<List<SeasonIndexResultItemModel>>(
                    data["data"]["list"]?.ToString() ?? "[]");
                LoadResultCore(items);
            }
            catch (CustomizedErrorException ex)
            {
                Notify.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                var handel = HandelError<SeasonIndexConditionFilterModel>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

        public async void Refresh()
        {
            if (Loading)
            {
                return;
            }
            Page = 1;
            if (Conditions == null)
            {
                await LoadConditions();
            }
            if (Conditions != null)
            {
                await LoadResult();
            }
        }

        public async void LoadMore()
        {
            if (Loading)
            {
                return;
            }
            if (Conditions == null || Conditions.Count == 0 || Result == null || Result.Count == 0)
            {
                return;
            }
            await LoadResult();
        }

        #endregion
    }
}
