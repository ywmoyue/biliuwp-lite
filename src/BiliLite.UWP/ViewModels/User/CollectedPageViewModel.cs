using MapsterMapper;
using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models;
using BiliLite.Models.Common.User;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api.User;
using BiliLite.Modules;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BiliLite.ViewModels.User
{
    public class CollectedPageViewModel : BaseViewModel
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly FavoriteApi m_favoriteApi;
        private readonly IMapper m_mapper;
        private int m_page;

        public CollectedPageViewModel(IMapper mapper)
        {
            m_favoriteApi = new FavoriteApi();
            m_mapper = mapper;
            LoadMoreCommand = new RelayCommand(LoadMore);
        }

        [DoNotNotify]
        public ICommand LoadMoreCommand { get; private set; }

        public ObservableCollection<FavoriteItemViewModel> CollectedItems { get; set; }

        public bool Loading { get; set; }

        public bool HasMore { get; set; }

        private async Task LoadCollectedCore()
        {
            var results = await m_favoriteApi.GetCollected(m_page).Request();
            if (!results.status)
                throw new CustomizedErrorException(results.message);

            var data = await results.GetJson<ApiDataModel<JObject>>();
            if (!data.success)
                throw new CustomizedErrorException(data.message);

            if (data.data["list"] != null)
            {
                var favorite =
                    await data.data["list"]
                        .ToString()
                        .DeserializeJson<ObservableCollection<FavoriteItemModel>>() ??
                    new ObservableCollection<FavoriteItemModel>();
                HasMore = (bool)data.data["has_more"];

                var favoriteViewModels = m_mapper.Map<List<FavoriteItemViewModel>>(favorite);

                if (m_page == 1)
                {
                    CollectedItems = new ObservableCollection<FavoriteItemViewModel>(favoriteViewModels);
                }
                else
                {
                    CollectedItems.AddRange(favoriteViewModels);
                }
            }
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
                    CollectedItems.Remove(item);
                    return true;
                }
                else
                    throw new CustomizedErrorException(data.message);
            }
            catch (CustomizedErrorException ex)
            {
                _logger.Error(ex.Message, ex);
                NotificationShowExtensions.ShowMessageToast(ex.Message);
            }
            catch (Exception ex)
            {
                var handel = HandelError<MyFollowVideoViewModel>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
            return false;
        }

        public async Task LoadCollected(int page = 1)
        {
            try
            {
                Loading = true;
                HasMore = false;
                m_page = page;
                await LoadCollectedCore();
            }
            catch (CustomizedErrorException ex)
            {
                _logger.Error(ex.Message, ex);
                NotificationShowExtensions.ShowMessageToast(ex.Message);
            }
            catch (Exception ex)
            {
                var handel = HandelError<MyFollowVideoViewModel>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

        public async void LoadMore()
        {
            await LoadCollected(m_page + 1);
        }
    }
}
