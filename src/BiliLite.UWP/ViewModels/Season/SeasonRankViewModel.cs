using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BiliLite.Extensions;
using BiliLite.Models;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common.Season;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json.Linq;

namespace BiliLite.ViewModels.Season
{
    [RegisterTransientViewModel]
    public class SeasonRankViewModel : BaseViewModel
    {
        #region Fields

        private readonly RankAPI m_rankApi;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        #endregion

        #region Constructors

        public SeasonRankViewModel()
        {
            m_rankApi = new RankAPI();
        }

        #endregion

        #region Properties

        public bool Loading { get; set; } = true;

        public SeasonRankDataViewModel Current { get; set; }

        public List<SeasonRankDataViewModel> RegionItems { get; set; }

        #endregion

        #region Public Methods

        public void LoadRankRegion(int type = 1)
        {
            RegionItems = new List<SeasonRankDataViewModel>()
            {
                new SeasonRankDataViewModel()
                {
                    Name="热门番剧",
                    Type=1
                },
                new SeasonRankDataViewModel()
                {
                    Name="热门国创",
                    Type=4
                },
                new SeasonRankDataViewModel()
                {
                    Name="热门电影",
                    Type=2
                },
                new SeasonRankDataViewModel()
                {
                    Name="热门纪录片",
                    Type=3
                },
                new SeasonRankDataViewModel()
                {
                    Name="热门电视剧",
                    Type=5
                },
                new SeasonRankDataViewModel()
                {
                    Name="热门综艺",
                    Type=7
                },
            };
            Current = RegionItems.FirstOrDefault(x => x.Type.Equals(type));
        }

        public async Task LoadRankDetail(SeasonRankDataViewModel region)
        {
            try
            {
                Loading = true;
                var results = await m_rankApi.SeasonRank(region.Type).Request();
                if (!results.status) throw new CustomizedErrorException(results.message);
                var data = await results.GetJson<ApiDataModel<JObject>>();
                if (!data.success) throw new CustomizedErrorException(data.message);
                var result = await data.data["list"].ToString().DeserializeJson<List<SeasonRankItemModel>>();
                region.Items = result;
            }
            catch (CustomizedErrorException ex)
            {
                Notify.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                var handel = HandelError<SeasonRankViewModel>(ex);
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
