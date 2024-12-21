using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using BiliLite.Extensions;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common.Home;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api.Home;
using BiliLite.Modules;
using BiliLite.Pages;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BiliLite.ViewModels.Home
{
    [RegisterTransientViewModel]
    public class HotViewModel : BaseViewModel
    {
        #region Fields

        private readonly HotAPI m_hotApi;
        private readonly IMainPage m_mainPage;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        #endregion

        #region Constructors

        public HotViewModel(IMainPage mainPage)
        {
            m_mainPage = mainPage;
            m_hotApi = new HotAPI();
            RefreshCommand = new RelayCommand(Refresh);
            LoadMoreCommand = new RelayCommand(LoadMore);
        }

        #endregion

        #region Properties

        public bool Loading { get; set; } = true;

        public ObservableCollection<HotDataItemModel> HotItems { get; set; }

        public List<HotTopItemModel> TopItems { get; set; }

        public ICommand RefreshCommand { get; private set; }

        public ICommand LoadMoreCommand { get; private set; }

        #endregion

        #region Private Methods

        private void GetPopularCore(JObject data)
        {
            TopItems ??= JsonConvert.DeserializeObject<List<HotTopItemModel>>(data["config"]["top_items"]
                .ToString());

            var items =
                JsonConvert.DeserializeObject<ObservableCollection<HotDataItemModel>>(data["data"]
                    .ToString());
            for (var i = items.Count - 1; i >= 0; i--)
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

        #endregion

        #region Public Methods

        public async Task GetPopular(string idx = "0", string lastParam = "")
        {
            try
            {
                Loading = true;

                var results = await m_hotApi.Popular(idx, lastParam).Request();
                if (!results.status) throw new CustomizedErrorException(results.message);
                var data = results.GetJObject();
                if (data["code"].ToInt32() != 0) throw new CustomizedErrorException(data["message"].ToString());
                GetPopularCore(data);
            }
            catch (CustomizedErrorException ex)
            {
                Notify.ShowMessageToast(ex.Message);
                _logger.Error("获取热门数据失败", ex);
            }
            catch (Exception ex)
            {
                var handel = HandelError<HotViewModel>(ex);
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
            // 当前不在首页，不应继续加载
            if (!(m_mainPage.CurrentPage is HomePage)) return;
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

        #endregion

    }
}
