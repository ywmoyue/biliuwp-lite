using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Windows.Input;
using BiliLite.Extensions;
using BiliLite.Models.Common.Recommend;
using BiliLite.Models.Requests.Api.Home;

namespace BiliLite.Modules
{
    public class HotVM : IModules
    {
        readonly HotAPI hotAPI;
        public HotVM()
        {
            hotAPI = new HotAPI();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
        }
        public ICommand RefreshCommand { get; private set; }
        public ICommand LoadMoreCommand { get; private set; }

        private bool _loading = true;
        public bool Loading
        {
            get { return _loading; }
            set { _loading = value; DoPropertyChanged("Loading"); }
        }

        private ObservableCollection<HotDataItemModel> _hotItems;

        public ObservableCollection<HotDataItemModel> HotItems
        {
            get { return _hotItems; }
            set { _hotItems = value; DoPropertyChanged("HotItems"); }
        }
        private List<HotTopItemModel> _topItems;

        public List<HotTopItemModel> TopItems
        {
            get { return _topItems; }
            set { _topItems = value; DoPropertyChanged("TopItems"); }
        }

        public async Task GetPopular(string idx = "0", string last_param = "")
        {
            try
            {
                Loading = true;

                var results = await hotAPI.Popular(idx, last_param).Request();
                if (results.status)
                {
                    var data = results.GetJObject();
                    if (data["code"].ToInt32() == 0)
                    {
                        if (TopItems == null)
                        {
                            TopItems = JsonConvert.DeserializeObject<List<HotTopItemModel>>(data["config"]["top_items"].ToString());
                        }
                        var items = JsonConvert.DeserializeObject<ObservableCollection<HotDataItemModel>>(data["data"].ToString());
                        for (int i = items.Count - 1; i >= 0; i--)
                        {
                            if (items[i].CardGoto != "av")
                                items.Remove(items[i]);
                        }
                        if (HotItems == null)
                        {
                            HotItems = items;
                        }
                        else
                        {
                            foreach (var item in items)
                            {
                                HotItems.Add(item);
                            }
                        }
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
                var handel = HandelError<HotVM>(ex);
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
            TopItems = null;
            HotItems = null;
            await GetPopular();
        }
        public async void LoadMore()
        {
            if (Loading)
            {
                return;
            }
            if (HotItems == null || HotItems.Count == 0)
            {
                return;
            }
            var last = HotItems.LastOrDefault();
            await GetPopular(last.Idx, last.Param);
        }
    }
    public class HotTopItemModel
    {
        [JsonProperty("entrance_id")]
        public int EntranceId { get; set; }

        public string Icon { get; set; }

        [JsonProperty("module_id")]
        public string ModuleId { get; set; }

        public string Uri { get; set; }

        public string Title { get; set; }
    }

    public class HotDataItemModel
    {
        [JsonProperty("card_type")]
        public string CardType { get; set; }

        [JsonProperty("card_goto")]
        public string CardGoto { get; set; }

        public string Param { get; set; }

        public string Cover { get; set; }

        public string Title { get; set; }

        public string Idx { get; set; }

        public string Uri { get; set; }

        [JsonProperty("cover_right_text_1")]
        public string CoverRightText1 { get; set; }

        [JsonProperty("right_desc_1")]
        public string RightDesc1 { get; set; }

        [JsonProperty("right_desc_2")]
        public string RightDesc2 { get; set; }

        [JsonProperty("cover_left_text_1")]
        public string CoverLeftText1 { get; set; }

        [JsonProperty("cover_left_text_2")]
        public string CoverLeftText2 { get; set; }

        [JsonProperty("cover_left_text_3")]
        public string CoverLeftText3 { get; set; }

        public string TextInfo1 => string.IsNullOrEmpty(CoverRightText1) ? CoverLeftText1 : CoverRightText1;

        public string TextInfo2 => string.IsNullOrEmpty(RightDesc1) ? CoverLeftText2 : RightDesc1;

        public string TextInfo3 => string.IsNullOrEmpty(RightDesc2) ? CoverLeftText3 : RightDesc2;

        [JsonProperty("rcmd_reason_style")]
        public RecommendRcmdReasonStyleModel RcmdReasonStyle { get; set; }

        [JsonProperty("top_rcmd_reason_style")]
        public RecommendRcmdReasonStyleModel TopRcmdReasonStyle { get; set; }

        public RecommendRcmdReasonStyleModel RcmdReason => RcmdReasonStyle ?? TopRcmdReasonStyle;
    }
}
