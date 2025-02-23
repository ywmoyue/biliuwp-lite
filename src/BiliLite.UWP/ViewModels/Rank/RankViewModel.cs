using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Rank;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BiliLite.ViewModels.Rank
{
    public class RankViewModel : BaseViewModel
    {
        private readonly RankAPI m_rankApi;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        public RankViewModel()
        {
            m_rankApi = new RankAPI();
        }

        public bool Loading { get; set; } = true;

        public RankRegionViewModel Current { get; set; }

        public List<RankRegionViewModel> RegionItems { get; set; }

        public void LoadRankRegion(int rid = 0)
        {
            try
            {
                Loading = true;
                RegionItems = new List<RankRegionViewModel>() {
                    new RankRegionViewModel(0,"全站"),
                    new RankRegionViewModel(0,"原创", RankRegionType.Origin),
                    new RankRegionViewModel(0,"新人", RankRegionType.Rookie),
                    new RankRegionViewModel(1,"动画"),
                    new RankRegionViewModel(168,"国创相关"),
                    new RankRegionViewModel(3,"音乐"),
                    new RankRegionViewModel(129,"舞蹈"),
                    new RankRegionViewModel(4,"游戏"),
                    new RankRegionViewModel(36,"知识"),
                    new RankRegionViewModel(188,"数码"),
                    new RankRegionViewModel(160,"生活"),
                    new RankRegionViewModel(211,"美食"),
                    new RankRegionViewModel(119,"鬼畜"),
                    new RankRegionViewModel(155,"时尚"),
                    new RankRegionViewModel(5,"娱乐"),
                    new RankRegionViewModel(181,"影视"),

                };
                Current = RegionItems.FirstOrDefault(x => x.Rid == rid);
            }
            catch (Exception ex)
            {
                var handel = HandelError<ApiDataModel<List<RankRegionViewModel>>>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }

        public async Task LoadRankDetail(RankRegionViewModel regionView)
        {
            try
            {
                Loading = true;
                var api = await m_rankApi.Rank(regionView.Rid, regionView.Type.ToString().ToLower());
                var results = await api.Request();
                if (!results.status)
                    throw new CustomizedErrorException(results.message);
                var data = await results.GetJson<ApiDataModel<JObject>>();
                if (!data.success)
                    throw new CustomizedErrorException(data.message);
                regionView.ToolTip = data.data["note"].ToString();
                var result = await data.data["list"].ToString().DeserializeJson<List<RankItemModel>>();
                int i = 1;
                result = result.ToList();
                foreach (var item in result)
                {
                    item.Rank = i;
                    i++;
                }

                regionView.Items = result;
            }
            catch (CustomizedErrorException ex)
            {
                _logger.Error(ex.Message, ex);
                NotificationShowExtensions.ShowMessageToast(ex.Message);
            }
            catch (Exception ex)
            {
                var handel = HandelError<ApiDataModel<List<RankRegionViewModel>>>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
            finally
            {
                Loading = false;
            }
        }
    }
}
