using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AutoMapper;
using BiliLite.Extensions;
using BiliLite.Models;
using BiliLite.Models.Common.User;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api.User;
using BiliLite.Modules;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json.Linq;
using PropertyChanged;

namespace BiliLite.ViewModels.User
{
    public class MyFollowVideoViewModel : BaseViewModel
    {
        #region Fields

        private readonly FavoriteApi m_favoriteApi;
        private readonly IMapper m_mapper;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private int m_page = 1;

        #endregion

        #region Constructors

        public MyFollowVideoViewModel(IMapper mapper)
        {
            m_mapper = mapper;
            m_favoriteApi = new FavoriteApi();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
        }

        #endregion

        #region Properties

        [DoNotNotify]
        public ICommand RefreshCommand { get; private set; }

        [DoNotNotify]
        public ICommand LoadMoreCommand { get; private set; }

        public bool Loading { get; set; }

        public ObservableCollection<FavoriteItemViewModel> MyFavorite { get; set; }

        public ObservableCollection<FavoriteItemViewModel> CollectFavorite { get; set; }

        public bool HasMore { get; set; }

        #endregion

        #region Private Methods

        private async Task LoadFavoriteCore()
        {
            var results = await m_favoriteApi.MyFavorite().Request();
            if (!results.status)
                throw new CustomizedErrorException(results.message);

            var data = await results.GetJson<ApiDataModel<JObject>>();
            if (!data.success)
                throw new CustomizedErrorException(data.message);

            if (data.data["space_infos"][0]["mediaListResponse"] != null)
            {
                var favorite =
                    await data.data["space_infos"][0]["mediaListResponse"]["list"]
                        .ToString()
                        .DeserializeJson<ObservableCollection<FavoriteItemModel>>() ??
                    new ObservableCollection<FavoriteItemModel>();
                favorite.Insert(0,
                    await data.data["default_folder"]["folder_detail"].ToString()
                        .DeserializeJson<FavoriteItemModel>());
                HasMore = (bool)data.data["space_infos"][0]["mediaListResponse"]["has_more"];

                var favoriteViewModels = m_mapper.Map<List<FavoriteItemViewModel>>(favorite);

                MyFavorite = new ObservableCollection<FavoriteItemViewModel>(favoriteViewModels);
                m_page++;
            }

            if (data.data["space_infos"][1]["mediaListResponse"] != null)
            {
                CollectFavorite = await data.data["space_infos"][1]["mediaListResponse"]["list"].ToString()
                    .DeserializeJson<ObservableCollection<FavoriteItemViewModel>>();
            }
        }

        #endregion

        #region Public Methods

        public async Task LoadFavorite()
        {
            try
            {
                Loading = true;
                HasMore = false;
                m_page = 1;
                await LoadFavoriteCore();
            }
            catch (CustomizedErrorException ex)
            {
                _logger.Error(ex.Message, ex);
                Notify.ShowMessageToast(ex.Message);
            }
            catch (Exception ex)
            {
                var handel = HandelError<MyFollowVideoViewModel>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

        public async void LoadMore()
        {
            await LoadCreateList();
        }

        public async Task LoadCreateList()
        {
            try
            {
                HasMore = false;
                var results = await m_favoriteApi.MyCreatedFavoriteList(m_page).Request();
                if (!results.status)
                    throw new CustomizedErrorException(results.message);

                var data = await results.GetJson<ApiDataModel<JObject>>();
                if (!data.success)
                    throw new CustomizedErrorException(data.message);

                var ls = await data.data["list"].ToString().DeserializeJson<List<FavoriteItemViewModel>>();
                foreach (var item in ls)
                {
                    MyFavorite.Add(item);
                }

                HasMore = (bool)data.data["has_more"];
                m_page++;
            }
            catch (CustomizedErrorException ex)
            {
                _logger.Error(ex.Message, ex);
                Notify.ShowMessageToast(ex.Message);
            }
            catch (Exception ex)
            {
                var handel = HandelError<MyFollowVideoViewModel>(ex);
                Notify.ShowMessageToast(handel.message);
            }
        }

        public async Task<bool> DelFavorite(string id)
        {
            try
            {
                var results = await m_favoriteApi.DelFavorite(id).Request();
                if (!results.status)
                    throw new CustomizedErrorException(results.message);

                var data = await results.GetJson<ApiDataModel<object>>();
                return data.success ? true : throw new CustomizedErrorException(data.message);
            }
            catch (CustomizedErrorException ex)
            {
                _logger.Error(ex.Message, ex);
                Notify.ShowMessageToast(ex.Message);
            }
            catch (Exception ex)
            {
                var handel = HandelError<MyFollowVideoViewModel>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            return false;
        }

        public async Task<bool> CancelCollected(FavoriteItemViewModel item)
        {
            try
            {
                var results = await m_favoriteApi.UnFavCollected(item.Id).Request();
                if (!results.status)
                    throw new CustomizedErrorException(results.message);

                var data = await results.GetJson<ApiDataModel<object>>();
                if (data.success)
                {
                    CollectFavorite.Remove(item);
                    return true;
                }
                else
                    throw new CustomizedErrorException(data.message);
            }
            catch (CustomizedErrorException ex)
            {
                _logger.Error(ex.Message, ex);
                Notify.ShowMessageToast(ex.Message);
            }
            catch (Exception ex)
            {
                var handel = HandelError<MyFollowVideoViewModel>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            return false;
        }

        public async void Refresh()
        {
            if (Loading)
            {
                return;
            }
            MyFavorite = null;
            CollectFavorite = null;
            await LoadFavorite();
        }

        public async Task SortMyFavorite()
        {
            try
            {
                var favIds = MyFavorite.Select(x => x.Id).ToList();
                var result = await m_favoriteApi.Sort(favIds).Request();
                if (!result.status)
                {
                    throw new CustomizedErrorException(result.code + ":" + result.message);
                }

                var data = await result.GetData<object>();
                if (!data.success) throw new CustomizedErrorException(data.code + ":" + data.message);
                Notify.ShowMessageToast("排序成功");
            }
            catch (Exception ex)
            {
                _logger.Error("排序失败" + ex.Message);
                Notify.ShowMessageToast("排序失败" + ex.Message);
            }
        }

        #endregion
    }
}
