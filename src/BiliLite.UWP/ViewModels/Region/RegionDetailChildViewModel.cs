using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using BiliLite.Extensions;
using BiliLite.Models;
using BiliLite.Models.Common.Home;
using BiliLite.Models.Common.Region;
using BiliLite.Models.Requests.Api;
using BiliLite.Modules;
using BiliLite.ViewModels.Common;
using BiliLite.ViewModels.Rank;
using Newtonsoft.Json;
using PropertyChanged;

namespace BiliLite.ViewModels.Region
{
    public class RegionDetailChildViewModel : BaseViewModel, IRegionViewModel
    {
        #region Fields

        private RegionChildrenItem m_region;
        private readonly RegionAPI m_regionApi;
        private RegionChildOrderModel m_selectOrder;
        private RegionTagItemModel m_selectTag;
        private string m_nextId = "";
        private int m_page = 1;

        #endregion

        #region Constructors

        public RegionDetailChildViewModel(RegionChildrenItem regionItem)
        {
            m_regionApi = new RegionAPI();
            Orders = new List<RegionChildOrderModel>() {
                //new RegionChildOrderModel("默认排序",""),
                new RegionChildOrderModel("最新视频","senddate"),
                new RegionChildOrderModel("最多播放","view"),
                new RegionChildOrderModel("评论最多","reply"),
                new RegionChildOrderModel("弹幕最多","danmaku"),
                new RegionChildOrderModel("最多收藏","favorite")
            };
            SelectOrder = Orders[0];
            m_region = regionItem;
            ID = regionItem.Tid;
            RegionName = regionItem.Name;
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
        public string RegionName { get; set; }

        [DoNotNotify]
        public int ID { get; set; }

        public bool Loading { get; set; }

        public List<RegionChildOrderModel> Orders { get; set; }

        [DoNotNotify]
        public RegionChildOrderModel SelectOrder
        {
            get => m_selectOrder;
            set
            {
                if (value != null)
                {
                    m_selectOrder = value;
                }
            }
        }

        [DoNotNotify]
        public RegionTagItemModel SelectTag
        {
            get => m_selectTag;
            set
            {
                if (value != null)
                {
                    m_selectTag = value;
                }

            }
        }

        public List<RegionTagItemModel> Tasgs { get; set; }

        public ObservableCollection<RegionVideoItemModel> Videos { get; set; }

        #endregion

        #region Public Methods

        public async Task LoadHome()
        {
            try
            {
                Loading = true;
                var api = m_regionApi.RegionChildDynamic(ID, (SelectTag == null) ? 0 : SelectTag.Tid);
                if (m_nextId != "")
                {
                    api = m_regionApi.RegionChildDynamic(ID, m_nextId, (SelectTag == null) ? 0 : SelectTag.Tid);
                }

                var results = await api.Request();
                if (results.status)
                {
                    var data = results.GetJObject();
                    if (data["code"].ToInt32() == 0)
                    {
                        var ls = JsonConvert.DeserializeObject<ObservableCollection<RegionVideoItemModel>>(data["data"]["new"].ToString());
                        if (m_nextId == "")
                        {
                            var tags = JsonConvert.DeserializeObject<List<RegionTagItemModel>>(data["data"]["top_tag"]?.ToString() ?? "[]");
                            tags.Insert(0, new RegionTagItemModel()
                            {
                                Tid = 0,
                                Tname = "全部标签"
                            });
                            if (Tasgs == null || Tasgs.Count == 0)
                            {
                                Tasgs = tags;
                                SelectTag = Tasgs[0];
                            }

                            Videos = ls;
                        }
                        else
                        {
                            foreach (var item in ls)
                            {
                                Videos.Add(item);
                            }
                        }
                        m_nextId = data["data"]["cbottom"]?.ToString() ?? "";
                    }
                    else
                    {
                        Notify.ShowMessageToast(data["message"].ToString());
                    }
                }
                else
                {
                    Notify.ShowMessageToast(results.message);

                }
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

        public async Task LoadList()
        {
            try
            {
                Loading = true;
                var api = m_regionApi.RegionChildList(ID, SelectOrder.Order, m_page, SelectTag.Tid);
                var results = await api.Request();
                if (results.status)
                {
                    var data = results.GetJObject();
                    if (data["code"].ToInt32() == 0)
                    {
                        var ls = JsonConvert.DeserializeObject<ObservableCollection<RegionVideoItemModel>>(data["data"].ToString());
                        if (m_page == 1)
                        {
                            Videos = ls;
                        }
                        else
                        {
                            foreach (var item in ls)
                            {
                                Videos.Add(item);
                            }
                        }
                        m_page++;
                    }
                    else
                    {
                        Notify.ShowMessageToast(data["message"].ToString());
                    }
                }
                else
                {
                    Notify.ShowMessageToast(results.message);

                }
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
            if (Loading)
            {
                return;
            }
            if (SelectOrder == null || SelectOrder.Order == "")
            {
                m_nextId = "";
                await LoadHome();
            }
            else
            {
                m_page = 1;
                await LoadList();
            }

        }
        public async void LoadMore()
        {
            if (Loading)
            {
                return;
            }
            if (SelectOrder == null || SelectOrder.Order == "")
            {
                await LoadHome();
            }
            else
            {
                await LoadList();
            }
        }

        #endregion
    }
}