using AutoMapper;
using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models;
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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BiliLite.ViewModels.Season
{
    [RegisterTransientViewModel]
    public class SeasonReviewViewModel : BaseViewModel
    {
        #region Fields

        readonly SeasonApi m_seasonApi;
        private readonly IMapper m_mapper;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        #endregion

        #region Constructors

        public SeasonReviewViewModel(IMapper mapper)
        {
            m_mapper = mapper;
            Items = new ObservableCollection<SeasonShortReviewItemViewModel>();
            m_seasonApi = new SeasonApi();
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
        public ObservableCollection<SeasonShortReviewItemViewModel> Items { get; set; }

        [DoNotNotify]
        public int MediaID { get; set; }

        public bool Loading { get; set; }

        public bool CanLoadMore { get; set; }

        [DoNotNotify]
        public string Next { get; set; } = "";

        #endregion

        #region Private Methods

        private void GetItemsCore(JObject data)
        {
            var items = JsonConvert.DeserializeObject<List<SeasonShortReviewItemModel>>(data["data"]["list"]
                .ToString());
            if (items == null) return;
            var dataItems = m_mapper.Map<List<SeasonShortReviewItemViewModel>>(items);
            Items.AddRange(dataItems);
            //Items = new IncrementalLoadingCollection<LiveRecommendItemSource, LiveRecommendItemModel>(new LiveRecommendItemSource(items, SortType), 30);
            if (Items.Count >= data["data"]["total"].ToInt32()) return;
            Next = data["data"]["next"].ToString();
            CanLoadMore = true;
        }

        private void LikeCore(SeasonShortReviewItemViewModel item, ApiDataModel<JObject> data)
        {
            item.Stat.Liked = data.data["status"].ToInt32();
            if (item.Stat.Liked == 1)
            {
                item.Stat.Likes += 1;
            }
            else
            {
                item.Stat.Likes -= 1;
            }
        }

        #endregion

        #region Public Methods

        public async Task GetItems()
        {
            try
            {
                if (MediaID == 0) { return; }
                Loading = true;
                CanLoadMore = false;
                var results = await m_seasonApi.ShortReview(MediaID, Next).Request();
                if (!results.status) throw new CustomizedErrorException(results.message);
                var data = results.GetJObject();
                if (data["code"].ToInt32() != 0) throw new CustomizedErrorException(data["message"].ToString());
                GetItemsCore(data);
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                var handel = HandelError<SeasonReviewViewModel>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
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
            Items.Clear();
            Next = "";
            await GetItems();
        }
        public async void LoadMore()
        {
            if (Loading)
            {
                return;
            }
            await GetItems();
        }

        public async void Like(SeasonShortReviewItemViewModel item)
        {
            if (!SettingService.Account.Logined && !await NotificationShowExtensions.ShowLoginDialog())
            {
                NotificationShowExtensions.ShowMessageToast("请先登录后再操作");
                return;
            }
            try
            {
                var api = m_seasonApi.LikeReview(MediaID, item.ReviewId, ReviewType.Short);
                var results = await api.Request();
                if (!results.status) throw new CustomizedErrorException(results.message);
                var data = await results.GetJson<ApiDataModel<JObject>>();
                if (!data.success) throw new CustomizedErrorException(data.message);
                LikeCore(item, data);
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

        public async void Dislike(SeasonShortReviewItemViewModel item)
        {
            if (!SettingService.Account.Logined && !await NotificationShowExtensions.ShowLoginDialog())
            {
                NotificationShowExtensions.ShowMessageToast("请先登录后再操作");
                return;
            }
            try
            {
                var api = m_seasonApi.DislikeReview(MediaID, item.ReviewId, ReviewType.Short);
                var results = await api.Request();
                if (!results.status) throw new CustomizedErrorException(results.message);
                var data = await results.GetJson<ApiDataModel<JObject>>();
                if (!data.success) throw new CustomizedErrorException(data.message);
                item.Stat.Disliked = data.data["status"].ToInt32();
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

        public async Task<bool> SendShortReview(string content, bool share, int score)
        {
            if (!SettingService.Account.Logined && !await NotificationShowExtensions.ShowLoginDialog())
            {
                NotificationShowExtensions.ShowMessageToast("请先登录后再操作");
                return false;
            }
            try
            {
                var api = m_seasonApi.SendShortReview(MediaID, content, share, score);
                var results = await api.Request();
                if (!results.status) throw new CustomizedErrorException(results.message);
                var data = await results.GetJson<ApiResultModel<JObject>>();
                if (!data.success) throw new CustomizedErrorException(data.message);
                NotificationShowExtensions.ShowMessageToast("发表成功");
                return true;
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
                return false;
            }
            catch (Exception ex)
            {
                var handel = HandelError<object>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
                return false;
            }
        }

        #endregion
    }
}
