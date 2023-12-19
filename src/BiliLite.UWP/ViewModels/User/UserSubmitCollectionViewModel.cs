using AutoMapper;
using BiliLite.Extensions;
using BiliLite.Models.Common.User;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api.User;
using BiliLite.Modules;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BiliLite.ViewModels.User
{
    /// <summary>
    /// 合集投稿
    /// </summary>
    public class UserSubmitCollectionViewModel : BaseViewModel
    {
        #region Fields

        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly UserDetailAPI m_userDetailApi;

        #endregion

        #region Constructors

        public UserSubmitCollectionViewModel()
        {
            RefreshSubmitCollectionCommand = new RelayCommand(Refresh);
            LoadMoreSubmitCollectionCommand = new RelayCommand(LoadMore);
            m_userDetailApi = new UserDetailAPI();
        }

        #endregion

        #region Properties
        public ICommand RefreshSubmitCollectionCommand { get; private set; }

        public ICommand LoadMoreSubmitCollectionCommand { get; private set; }

        public string Mid { get; set; }

        public bool LoadingSubmitCollection { get; set; } = true;

        public bool SubmitCollectionCanLoadMore { get; set; }

        public ObservableCollection<SubmitCollectionItemModel> SubmitCollectionItems { get; set; }

        public bool Nothing { get; set; }

        public int SubmitCollectionPage { get; set; } = 1;

        #endregion

        #region Private Methods

        private void GetSubmitCollectionCore(JObject data)
        {
            var items = JsonConvert.DeserializeObject<List<SubmitCollectionItemModel>>(data["data"]["items"].ToString());
            SubmitCollectionPage = data["data"]["page"]["page_num"].ToInt32();
            var count = data["data"]["page"]["total"].ToInt32();
            AttachSubmitCollectionItems(items, count);
            
        }

        private void AttachSubmitCollectionItems(List<SubmitCollectionItemModel> submitCollectionItems, int count)
        {
            if (SubmitCollectionItems == null)
            {
                var observableSubmitCollectionItems = new ObservableCollection<SubmitCollectionItemModel>(submitCollectionItems);
                SubmitCollectionItems = observableSubmitCollectionItems;
            }
            else
            {
                foreach (var item in submitCollectionItems)
                {
                    SubmitCollectionItems.Add(item);
                }
            }

            if (SubmitCollectionPage == 1 && SubmitCollectionItems == null)
            {
                Nothing = true;
            }
            if (SubmitCollectionItems.Count >= count)
            {
                SubmitCollectionCanLoadMore = false;
            }
            else
            {
                SubmitCollectionCanLoadMore = true;
                SubmitCollectionPage++;
            }
        }

        #endregion

        #region Public Methods

        public async Task GetSubmitCollection()
        {
            try
            {
                Nothing = false;
                SubmitCollectionCanLoadMore = false;
                LoadingSubmitCollection = true;

                var api = m_userDetailApi.SubmitCollections(Mid, SubmitCollectionPage);
                var results = await api.Request();
                if (!results.status)
                {
                    throw new CustomizedErrorException(results.message);
                }
                var data = results.GetJObject();
                if (data["code"].ToInt32() != 0)
                {
                    throw new CustomizedErrorException(data["message"].ToString());
                }

                GetSubmitCollectionCore(data);
            }
            catch (Exception ex)
            {
                if (ex is CustomizedErrorException)
                {
                    Notify.ShowMessageToast(ex.Message);
                }
                else
                {
                    var handel = HandelError<UserSubmitCollectionViewModel>(ex);
                    Notify.ShowMessageToast(handel.message);
                }

                _logger.Error("获取用户合集失败", ex);
            }
            finally
            {
                LoadingSubmitCollection = false;
            }
        }

        public async void Refresh()
        {
            if (LoadingSubmitCollection)
            {
                return;
            }
            SubmitCollectionItems = null;
            SubmitCollectionPage = 1;
            await GetSubmitCollection();
        }

        public async void LoadMore()
        {
            if (LoadingSubmitCollection)
            {
                return;
            }
            if (SubmitCollectionItems == null)
            {
                return;
            }
            await GetSubmitCollection();
        }

        #endregion
    }
}
