using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BiliLite.Extensions;
using BiliLite.Models;
using BiliLite.Models.Common.Home;
using BiliLite.Models.Common.Region;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api;
using BiliLite.Modules;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using BiliLite.ViewModels.Rank;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyChanged;

namespace BiliLite.ViewModels.Region
{
    public class RegionDetailHomeViewModel : BaseViewModel, IRegionViewModel
    {
        #region Fields

        private readonly RegionAPI m_regionApi;
        private RegionItem m_region;
        private string m_nextId = "";
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        #endregion

        #region Constructors

        public RegionDetailHomeViewModel(RegionItem regionItem)
        {
            m_regionApi = new RegionAPI();
            m_region = regionItem;
            ID = regionItem.Tid;
            //RegionName = regionItem.name;
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
        }

        #endregion

        #region Properties

        [DoNotNotify]
        public ICommand RefreshCommand { get; set; }

        [DoNotNotify]
        public ICommand LoadMoreCommand { get; set; }

        [DoNotNotify]
        public int ID { get; set; }

        [DoNotNotify]
        public string RegionName { get; set; } = "推荐";

        public bool Loading { get; set; }

        public List<RegionHomeBannerItemModel> Banners { get; set; }

        public ObservableCollection<RegionVideoItemModel> Videos { get; set; }

        #endregion

        #region Private Methods

        private void LoadHomeCore(JObject data)
        {
            if (data["code"].ToInt32() != 0) throw new CustomizedErrorException(data["message"].ToString());
            var ls = JsonConvert.DeserializeObject<ObservableCollection<RegionVideoItemModel>>(
                data["data"]["new"].ToString());
            if (m_nextId == "")
            {
                var recommend =
                    JsonConvert.DeserializeObject<ObservableCollection<RegionVideoItemModel>>(
                        data["data"]["recommend"]?.ToString() ?? "[]");
                foreach (var item in recommend)
                {
                    ls.Insert(0, item);
                }

                Banners = JsonConvert.DeserializeObject<List<RegionHomeBannerItemModel>>(
                    data["data"]["banner"]["top"].ToString());
                Videos = ls;
            }
            else
            {
                foreach (var item in ls)
                {
                    Videos.Add(item);
                }
            }

            m_nextId = data["data"]["cbottom"].ToString();
        }

        #endregion

        #region Public Methods

        public async Task LoadHome()
        {
            try
            {
                Loading = true;
                var api = m_regionApi.RegionDynamic(ID);
                if (m_nextId != "")
                {
                    api = m_regionApi.RegionDynamic(ID, m_nextId);
                }
                var results = await api.Request();

                if (!results.status) throw new CustomizedErrorException(results.message);
                var data = results.GetJObject();
                LoadHomeCore(data);
            }
            catch (CustomizedErrorException ex)
            {
                Notify.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                var handel = HandelError<ApiDataModel<List<RankRegionViewModel>>>(ex);
                Notify.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

        public async void Refresh()
        {
            m_nextId = "";
            await LoadHome();
        }
        public async void LoadMore()
        {
            await LoadHome();
        }

        #endregion
    }
}