using BiliLite.Models;
using BiliLite.Models.Requests.Api;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BiliLite.Extensions;
using BiliLite.Models.Common.User;
using BiliLite.Models.Exceptions;
using BiliLite.Services;
using BiliLite.ViewModels.Common;

namespace BiliLite.Modules.User
{
    public class HistoryVM : BaseViewModel
    {
        AccountApi accountApi;
        private HistoryCursor m_historyCursor;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        public HistoryVM()
        {
            accountApi = new AccountApi();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
        }

        public bool Loading { get; set; }

        public ICommand LoadMoreCommand { get; private set; }
        public ICommand RefreshCommand { get; private set; }

        public bool Nothing { get; set; }
        
        public bool ShowLoadMore { get; set; }
        
        public ObservableCollection<UserHistoryItem> Videos { get; set; }

        public async Task LoadHistory()
        {
            try
            {
                ShowLoadMore = false;
                Loading = true;
                Nothing = false;
                var results = await accountApi.HistoryWbi(m_historyCursor).Request();
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
            catch (CustomizedErrorException ex)
            {
                Notify.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message);
            }
            catch (Exception ex)
            {
                var handel = HandelError<HistoryVM>(ex);
                Notify.ShowMessageToast(handel.message);
                _logger.Error("加载历史记录失败", ex);
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
            await LoadHistory();
        }
        public async void Del(UserHistoryItem item)
        {
            try
            {
                var results = await accountApi.DelHistory(item.History.Business + "_" + item.Kid).Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        Videos.Remove(item);
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
                var handel = HandelError<HistoryVM>(ex);
                Notify.ShowMessageToast(handel.message);
            }
        }
    }
}
