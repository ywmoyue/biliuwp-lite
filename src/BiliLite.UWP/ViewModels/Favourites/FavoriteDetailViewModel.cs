using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common.Favorites;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api.User;
using BiliLite.Modules;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace BiliLite.ViewModels.Favourites
{
    [RegisterTransientViewModel]
    public class FavoriteDetailViewModel : BaseViewModel
    {
        #region Fields

        private readonly FavoriteApi m_favoriteApi;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        #endregion

        #region Constructors

        public FavoriteDetailViewModel()
        {
            m_favoriteApi = new FavoriteApi();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
            CollectCommand = new RelayCommand(DoCollect);
            CancelCollectCommand = new RelayCommand(DoCancelCollect);
            SelectCommand = new RelayCommand<object>(SetSelectMode);
        }

        #endregion

        #region Properties

        [DoNotNotify]
        public ICommand CollectCommand { get; private set; }

        [DoNotNotify]
        public ICommand CancelCollectCommand { get; private set; }

        [DoNotNotify]
        public ICommand RefreshCommand { get; private set; }

        [DoNotNotify]
        public ICommand LoadMoreCommand { get; private set; }

        [DoNotNotify]
        public ICommand SelectCommand { get; private set; }

        [DoNotNotify]
        public int Page { get; set; } = 1;

        [DoNotNotify]
        public string Keyword { get; set; } = "";

        [DoNotNotify]
        public string Id { get; set; }

        [DoNotNotify]
        public int Type { get; set; }

        public bool Loading { get; set; }

        public FavoriteInfoModel FavoriteInfo { get; set; }

        public ObservableCollection<FavoriteInfoVideoItemModel> Videos { get; set; }

        public ListViewSelectionMode SelectionMode { get; set; } = ListViewSelectionMode.None;

        public bool IsItemClickEnabled { get; set; } = true;

        public bool Nothing { get; set; }

        public bool ShowLoadMore { get; set; }

        public bool IsSelf { get; set; }

        public bool ShowCollect { get; set; }

        public bool ShowCancelCollect { get; set; }

        #endregion

        #region Private Methods

        private void SetSelectMode(object data)
        {
            if (data == null)
            {
                IsItemClickEnabled = true;
                SelectionMode = ListViewSelectionMode.None;
            }
            else
            {
                IsItemClickEnabled = false;
                SelectionMode = ListViewSelectionMode.Multiple;
            }
        }

        private void LoadFavoriteInfoCore(ApiDataModel<FavoriteDetailModel> data)
        {
            if (Page == 1)
            {
                FavoriteInfo = data.data.Info;
                IsSelf = FavoriteInfo.Mid == SettingService.Account.UserID.ToString();
                if (!IsSelf)
                {
                    ShowCollect = FavoriteInfo.FavState != 1;
                    ShowCancelCollect = !ShowCollect;
                }

                if (data.data.Medias == null || data.data.Medias.Count == 0)
                {
                    Nothing = true;
                    return;
                }

                Videos = new ObservableCollection<FavoriteInfoVideoItemModel>(data.data.Medias);
            }
            else
            {
                if (data.data.Medias != null)
                {
                    foreach (var item in data.data.Medias)
                    {
                        Videos.Add(item);
                    }
                }
            }

            if (Videos.Count != FavoriteInfo.MediaCount)
            {
                ShowLoadMore = true;
                Page++;
            }
        }

        #endregion

        #region Public Methods

        public async Task LoadFavoriteInfo()
        {
            try
            {
                ShowLoadMore = false;
                Loading = true;
                Nothing = false;
                var api = m_favoriteApi.FavoriteInfo(Id, Keyword, Page);
                if (Type == 21)
                {
                    api = m_favoriteApi.FavoriteSeasonInfo(Id, Keyword, Page);
                }
                var results = await api.Request();
                if (!results.status) throw new CustomizedErrorException(results.message);
                var data = await results.GetJson<ApiDataModel<FavoriteDetailModel>>();
                if (!data.success) throw new CustomizedErrorException(data.message);
                LoadFavoriteInfoCore(data);
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                var handel = HandelError<FavoriteDetailViewModel>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

        public async Task Delete(List<FavoriteInfoVideoItemModel> items)
        {
            try
            {
                if (!SettingService.Account.Logined && !await NotificationShowExtensions.ShowLoginDialog())
                {
                    NotificationShowExtensions.ShowMessageToast("请先登录后再操作");
                    return;
                }
                var results = await m_favoriteApi.Delete(Id, items.Select(x => x.Id).ToList()).Request();
                if (!results.status) throw new CustomizedErrorException(results.message);
                var data = await results.GetData<object>();
                if (!data.success) throw new CustomizedErrorException(data.message);
                foreach (var item in items)
                {
                    Videos.Remove(item);
                }
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                var handel = HandelError<FavoriteDetailViewModel>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
        }

        public async Task Clean()
        {
            try
            {
                if (!SettingService.Account.Logined && !await NotificationShowExtensions.ShowLoginDialog())
                {
                    NotificationShowExtensions.ShowMessageToast("请先登录后再操作");
                    return;
                }
                var results = await m_favoriteApi.Clean(Id).Request();
                if (!results.status) throw new CustomizedErrorException(results.message);
                var data = await results.GetData<object>();
                if (!data.success) throw new CustomizedErrorException(data.message);
                Refresh();
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                var handel = HandelError<FavoriteDetailViewModel>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
        }

        public async void Refresh()
        {
            if (Loading)
            {
                return;
            }
            Page = 1;
            FavoriteInfo = null;
            Videos = null;
            await LoadFavoriteInfo();
        }

        public async Task Sort(string sourceId, string targetId)
        {
            try
            {
                var result = await m_favoriteApi.SortResource(Id, sourceId, targetId).Request();
                if (!result.status) throw new CustomizedErrorException("排序失败" + result.message);

                var data = await result.GetData<object>();
                if (!data.success) throw new CustomizedErrorException("排序失败" + data.message);
                NotificationShowExtensions.ShowMessageToast("排序成功");
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                var handel = HandelError<FavoriteDetailViewModel>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
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
            await LoadFavoriteInfo();
        }

        public async void Search(string keyword)
        {
            if (Loading)
            {
                return;
            }
            Keyword = keyword;
            Page = 1;
            FavoriteInfo = null;
            Videos = null;
            await LoadFavoriteInfo();
        }

        public async void DoCollect()
        {
            if (!SettingService.Account.Logined && !await NotificationShowExtensions.ShowLoginDialog())
            {
                NotificationShowExtensions.ShowMessageToast("请先登录后再操作");
                return;
            }
            try
            {
                var results = await m_favoriteApi.CollectFavorite(FavoriteInfo.Id).Request();
                if (!results.status) throw new CustomizedErrorException(results.message);
                var data = await results.GetJson<ApiDataModel<object>>();
                if (!data.success) throw new CustomizedErrorException(data.message);
                ShowCancelCollect = true;
                ShowCollect = false;
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                var handel = HandelError<object>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
        }

        public async void DoCancelCollect()
        {
            if (!SettingService.Account.Logined && !await NotificationShowExtensions.ShowLoginDialog())
            {
                NotificationShowExtensions.ShowMessageToast("请先登录后再操作");
                return;
            }
            try
            {
                var results = await m_favoriteApi.CacnelCollectFavorite(FavoriteInfo.Id).Request();
                if (!results.status) throw new CustomizedErrorException(results.message);
                var data = await results.GetJson<ApiDataModel<object>>();
                if (!data.success) throw new CustomizedErrorException(data.message);
                ShowCancelCollect = false;
                ShowCollect = true;
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                var handel = HandelError<object>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
        }

        #endregion
    }
}
