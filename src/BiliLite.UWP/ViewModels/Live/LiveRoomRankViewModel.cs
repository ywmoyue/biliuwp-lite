using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Live;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api.Live;
using BiliLite.Modules;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BiliLite.ViewModels.Live
{
    public class LiveRoomRankViewModel : BaseViewModel
    {
        #region Fields

        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly LiveRoomAPI m_liveRoomApi;

        #endregion

        #region Constructors

        public LiveRoomRankViewModel(int roomId, long uid, string title, string type)
        {
            m_liveRoomApi = new LiveRoomAPI();
            RankType = type;
            Title = title;
            RoomID = roomId;
            Uid = uid;
            LoadMoreCommand = new RelayCommand(LoadMore);
            Items = new ObservableCollection<LiveRoomRankItemModel>();
        }

        #endregion

        #region Properties

        public ICommand LoadMoreCommand { get; private set; }

        public int RoomID { get; set; }

        public long Uid { get; set; }

        public string Title { get; set; }

        public string RankType { get; set; }

        public bool Loading { get; set; }

        public ObservableCollection<LiveRoomRankItemModel> Items { get; set; }

        public int Page { get; set; } = 1;

        public bool CanLoadMore { get; set; }

        public int Next { get; set; } = 0;

        #endregion

        #region Private Methods

        private void LoadDataCore(JObject data)
        {
            var list = JsonConvert.DeserializeObject<List<LiveRoomRankItemModel>>(data["data"]["list"]
                .ToString());
            if (list != null)
            {
                foreach (var item in list)
                {
                    Items.Add(item);
                }
            }

            if (RankType != "fans")
            {
                Next = data["data"]["next_offset"].ToInt32();
                CanLoadMore = Next != 0;
            }
            else
            {
                var total = data["data"]["total_page"].ToInt32();
                if (Page < total)
                {
                    CanLoadMore = true;
                    Page++;
                }
            }
        }

        #endregion

        #region Public Methods

        public async void LoadMore()
        {
            if (Loading)
            {
                return;
            }
            await LoadData();
        }

        public async Task LoadData()
        {
            try
            {
                Loading = true;
                CanLoadMore = false;
                var api = m_liveRoomApi.FansList(Uid, RoomID, Page);
                if (RankType != "fans")
                {
                    api = m_liveRoomApi.RoomRankList(Uid, RoomID, RankType, Next);
                }

                var result = await api.Request();
                if (!result.status)
                {
                    throw new CustomizedErrorException(result.message);
                }

                var data = result.GetJObject();
                if (data["code"].ToInt32() != 0)
                {
                    throw new CustomizedErrorException(data["message"].ToString());
                }

                LoadDataCore(data);
            }
            catch (CustomizedErrorException ex)
            {
                Notify.ShowMessageToast(ex.Message);
                _logger.Error("读取直播排行榜失败" + RankType, ex);
            }
            catch (Exception ex)
            {
                Notify.ShowMessageToast("读取直播排行榜失败：" + RankType);
                _logger.Log("读取直播排行榜失败" + RankType, LogType.Error, ex);
            }
            finally
            {
                Loading = false;
            }
        }

        #endregion
    }
}