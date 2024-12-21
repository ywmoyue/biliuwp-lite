﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BiliLite.Extensions;
using BiliLite.Models;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common.Home;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api.Home;
using BiliLite.Modules.Live.LiveCenter;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json.Linq;

namespace BiliLite.ViewModels.Home
{
    [RegisterTransientViewModel]
    public class LiveViewModel : BaseViewModel
    {
        #region Fields

        private readonly LiveAPI m_liveApi;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        #endregion

        #region Constructors

        public LiveViewModel()
        {
            m_liveApi = new LiveAPI();
            LiveAttentionVm = new LiveAttentionVM();
        }

        #endregion

        #region Properties

        public LiveAttentionVM LiveAttentionVm { get; private set; }

        public bool ShowFollows { get; set; }

        public bool Loading { get; set; }

        public bool LoadingFollow { get; set; } = true;

        public ObservableCollection<LiveHomeBannerModel> Banners { get; set; }

        public ObservableCollection<LiveHomeAreaModel> Areas { get; set; }

        //public ObservableCollection<LiveFollowAnchorModel> Follow { get; set; }

        public List<LiveHomeItemsModel> Items { get; set; }

        #endregion

        #region Public Methods

        public async Task GetLiveHome()
        {
            try
            {
                Loading = true;
                var api = m_liveApi.LiveHome();

                var results = await api.Request();
                if (!results.status) throw new CustomizedErrorException(results.message);
                var data = await results.GetJson<ApiDataModel<JObject>>();
                if (!data.success) throw new CustomizedErrorException(data.message);
                if (data.data["banner"].Any())
                {
                    Banners = await data.data["banner"][0]["list"].ToString()
                        .DeserializeJson<ObservableCollection<LiveHomeBannerModel>>();
                }

                if (data.data["area_entrance_v2"].Any())
                {
                    Areas = await data.data["area_entrance_v2"][0]["list"].ToString()
                        .DeserializeJson<ObservableCollection<LiveHomeAreaModel>>();
                }

                await GetLiveHomeItems();
            }
            catch (CustomizedErrorException ex)
            {
                Notify.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                var handel = HandelError<LiveViewModel>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

        public async Task GetLiveHomeItems()
        {
            try
            {
                Loading = true;
                var api = m_liveApi.LiveHomeItems();
                var results = await api.Request();
                if (!results.status) throw new CustomizedErrorException(results.message);
                var data = await results.GetJson<ApiDataModel<JObject>>();
                if (!data.success) throw new CustomizedErrorException(data.message);
                var items = await data.data["room_list"].ToString().DeserializeJson<List<LiveHomeItemsModel>>();

                Items = items.Where(x => x.List != null && x.List.Count > 0).ToList();
            }
            catch (CustomizedErrorException ex)
            {
                Notify.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                var handel = HandelError<LiveViewModel>(ex);
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
