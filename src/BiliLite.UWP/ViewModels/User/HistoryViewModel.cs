using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models;
using BiliLite.Models.Common.User;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api;
using BiliLite.Modules;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BiliLite.ViewModels.User
{
    public class HistoryViewModel : BaseViewModel
    {
        #region Fields

        private readonly AccountApi m_accountApi;
        private HistoryCursor m_historyCursor;
        private int m_searchPage = 1;
        private string m_searchKeyword;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private string m_mode = nameof(LoadHistory);

        #endregion

        #region Constructors

        public HistoryViewModel()
        {
            m_accountApi = new AccountApi();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
        }

        #endregion

        #region Properties

        [DoNotNotify]
        public ICommand LoadMoreCommand { get; private set; }

        [DoNotNotify]
        public ICommand RefreshCommand { get; private set; }

        public bool Loading { get; set; }

        public bool Nothing { get; set; }

        public bool ShowLoadMore { get; set; }

        public ObservableCollection<UserHistoryItem> Videos { get; set; }

        #endregion

        #region Private Methods

        private async Task LoadHistoryCore()
        {
            var results = await m_accountApi.HistoryWbi(m_historyCursor).Request();
            if (!results.status)
            {
                throw new CustomizedErrorException(results.message);
            }

            var data = await results.GetJson<ApiDataModel<UserHistory>>();
            if (!data.success)
            {
                throw new CustomizedErrorException(data.message);
            }

            if (m_historyCursor == null)
            {
                if (data.data == null || data.data.List.Count == 0)
                {
                    Nothing = true;
                    return;
                }

                Videos = new ObservableCollection<UserHistoryItem>(data.data.List);

                // 默认封面如果缺失, 用其他封面替换
                foreach (var item in Videos)
                {
                    if (item.Cover.Length == 0 && item.Covers != null)
                    {
                        item.Cover = (string)((JArray)item.Covers)[0];
                    }
                }
            }
            else
            {
                if (data.data != null)
                {
                    foreach (var item in data.data.List)
                    {
                        Videos.Add(item);
                    }
                }
            }

            if (data.data != null && data.data.List.Count != 0)
            {
                ShowLoadMore = true;
                m_historyCursor = data.data.Cursor;
            }
        }

        private async Task SearchHistoryCore(string keyword, int page)
        {
            var results = await m_accountApi.SearchHistory(keyword, page).Request();
            if (!results.status)
            {
                throw new CustomizedErrorException(results.message);
            }

            var data = await results.GetData<SearchHistoryData>();
            if (!data.success)
            {
                throw new CustomizedErrorException(data.message);
            }

            if (page > 1)
            {
                Videos.AddRange(data.data.List);
            }
            else
                Videos = new ObservableCollection<UserHistoryItem>(data.data.List);
            if (data.data.HasMore)
            {
                ShowLoadMore = true;
            }
        }

        #endregion

        #region Public Methods

        public async Task LoadHistory()
        {
            try
            {
                ShowLoadMore = false;
                Loading = true;
                Nothing = false;
                m_mode = nameof(LoadHistory);
                await LoadHistoryCore();
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message);
            }
            catch (Exception ex)
            {
                var handel = HandelError<HistoryViewModel>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
                _logger.Error("加载历史记录失败", ex);
            }
            finally
            {
                Loading = false;
            }
        }

        public async Task SearchHistory(string keyword, int page = 1)
        {
            ShowLoadMore = false;
            Loading = true;
            Nothing = false;
            m_searchPage = page;
            m_searchKeyword = keyword;
            m_mode = nameof(SearchHistory);
            try
            {
                await SearchHistoryCore(keyword, m_searchPage);
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message);
            }
            catch (Exception ex)
            {
                var handel = HandelError<HistoryViewModel>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
                _logger.Error("搜索历史记录失败", ex);
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

            m_historyCursor = null;
            Videos = null;
            await LoadHistory();
        }

        public async void LoadMore()
        {
            if (Loading)
            {
                return;
            }
            if (Videos == null || Videos.Count == 0)
            {
                return;
            }

            if (m_mode == nameof(SearchHistory))
                await SearchHistory(m_searchKeyword, m_searchPage + 1);
            else
                await LoadHistory();
        }

        public async void Del(UserHistoryItem item)
        {
            try
            {
                var results = await m_accountApi.DelHistory(item.History.Business + "_" + item.Kid).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        Videos.Remove(item);
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
                var handel = HandelError<HistoryViewModel>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
        }

        #endregion
    }
}
