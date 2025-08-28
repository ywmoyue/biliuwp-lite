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
                    new RankRegionViewModel(1005,"动画"),
                    new RankRegionViewModel(1008,"游戏"),
                    new RankRegionViewModel(1007,"鬼畜"),
                    new RankRegionViewModel(1003,"音乐"),
                    new RankRegionViewModel(1004,"舞蹈"),
                    new RankRegionViewModel(1001,"影视"),
                    new RankRegionViewModel(1002,"娱乐"),
                    new RankRegionViewModel(1010,"知识"),
                    new RankRegionViewModel(1012,"数码"),
                    new RankRegionViewModel(1020,"美食"),
                    new RankRegionViewModel(1013,"汽车"),
                    new RankRegionViewModel(1014,"时尚"),
                    new RankRegionViewModel(1018,"体育"),
                    new RankRegionViewModel(1024,"动物"),
                    new RankRegionViewModel(168,"国创相关"),
                    new RankRegionViewModel(160,"生活"),

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
