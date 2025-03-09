using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Home;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api.Home;
using BiliLite.Modules;
using BiliLite.Pages;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

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

        [DoNotNotify]
        public double ScrollViewLoadMoreBottomOffset { get; } =
            SettingService.GetValue(SettingConstants.UI.SCROLL_VIEW_LOAD_MORE_BOTTOM_OFFSET, SettingConstants.UI.DEFAULT_SCROLL_VIEW_LOAD_MORE_BOTTOM_OFFSET);

        #endregion

        #region Private Methods

        private void GetPopularCore(List<HotDataItemModel> hotDataItems)
        {
            for (var i = hotDataItems.Count - 1; i >= 0; i--)
            {
                if (hotDataItems[i].CardGoto != "av")
                    hotDataItems.Remove(hotDataItems[i]);
            }

            if (HotItems == null)
            {
                HotItems = new ObservableCollection<HotDataItemModel>(hotDataItems);
            }
            else
            {
                foreach (var item in hotDataItems)
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
                var requestPage = 3;
                var items = new List<HotDataItemModel>();

                for (int i = 0; i < requestPage; i++)
                {
                    if (i > 0)
                    {
                        idx = items.LastOrDefault()?.Idx;
                        lastParam = items.LastOrDefault()?.Param;
                    }
                    var results = await m_hotApi.Popular(idx, lastParam).Request();
                    if (!results.status) throw new CustomizedErrorException(results.message);
                    var data = results.GetJObject();
                    if (data["code"].ToInt32() != 0) throw new CustomizedErrorException(data["message"].ToString());

                    TopItems ??= JsonConvert.DeserializeObject<List<HotTopItemModel>>(data["config"]["top_items"]
                        .ToString());

                    items.AddRange(
                        JsonConvert.DeserializeObject<List<HotDataItemModel>>(data["data"]
                            .ToString()));
                }
                GetPopularCore(items);
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error("获取热门数据失败", ex);
            }
            catch (Exception ex)
            {
                var handel = HandelError<HotViewModel>(ex);
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
