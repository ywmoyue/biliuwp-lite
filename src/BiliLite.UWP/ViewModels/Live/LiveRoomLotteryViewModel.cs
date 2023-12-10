using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Live;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api.Live;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyChanged;

namespace BiliLite.ViewModels.Live
{
    public class LiveRoomLotteryViewModel : BaseViewModel
    {
        #region Fields

        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly LiveRoomAPI m_liveRoomApi;

        #endregion

        #region Constructors

        public LiveRoomLotteryViewModel()
        {
            m_liveRoomApi = new LiveRoomAPI();
            AnchorLotteryTimer = new Timer(1000);
            AnchorLotteryTimer.Elapsed += AnchorLottery_Timer_Elapsed;
            RedPocketLotteryTimer = new Timer(1000);
            RedPocketLotteryTimer.Elapsed += RedPocket_Timer_Elapsed;
        }

        #endregion

        #region Properties

        // 天选时刻相关
        [DoNotNotify]
        public Timer AnchorLotteryTimer { get; set; }

        public LiveRoomAnchorLotteryInfoModel AnchorLotteryInfo { get; set; }

        public string AnchorLotteryDownTime { get; set; } = "--:--";

        public bool AnchorLotteryShow { get; set; }

        // 人气红包相关
        [DoNotNotify]
        public Timer RedPocketLotteryTimer { get; set; }

        public LiveRoomRedPocketLotteryInfoModel RedPocketLotteryInfo { get; set; }

        public string RedPocketLotteryDownTime { get; set; } = "--:--";

        public bool RedPocketLotteryShow { get; set; }

        public event EventHandler<LiveRoomAnchorLotteryInfoModel> AnchorLotteryStart;

        #endregion

        #region Private Methods

        private async void AnchorLottery_Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (AnchorLotteryInfo == null) return;

                if (AnchorLotteryInfo.Time <= 0)
                {          
                    AnchorLotteryDownTime = "已开奖";
                    AnchorLotteryInfo.GoawayTime--;
                }

                if (AnchorLotteryInfo.GoawayTime <= 0)
                {
                    AnchorLotteryTimer.Stop();
                    AnchorLotteryShow = false;
                    AnchorLotteryInfo = null;
                    return;
                }
                
                if (AnchorLotteryInfo.Time > 0)
                {
                    AnchorLotteryDownTime = TimeSpan.FromSeconds(AnchorLotteryInfo.Time).ToString(@"mm\:ss");
                }
                
                AnchorLotteryInfo.Time--;
            });
        }

        private async void RedPocket_Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (RedPocketLotteryInfo == null) return;

                var nowTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
                var runningTime = RedPocketLotteryInfo.CurrentTime - RedPocketLotteryInfo.StartTime;

                if (RedPocketLotteryInfo.LotteryStatus == 2 ||
                    runningTime >= RedPocketLotteryInfo.LastTime ||
                    nowTime >= RedPocketLotteryInfo.EndTime)
                {
                    RedPocketLotteryDownTime = "已开奖";
                }

                if (nowTime >= RedPocketLotteryInfo.RemoveTime)
                {
                    RedPocketLotteryInfo = null;
                    RedPocketLotteryTimer.Stop();
                    RedPocketLotteryShow = false;
                    return;
                }

                if (runningTime < RedPocketLotteryInfo.LastTime &&
                    nowTime < RedPocketLotteryInfo.EndTime)
                {
                    RedPocketLotteryDownTime = TimeSpan.FromSeconds(RedPocketLotteryInfo.EndTime - nowTime).ToString(@"mm\:ss");
                }
            });
        }

        #endregion

        #region Public Methods

        public async Task LoadLotteryInfo(int roomId)
        {
            try
            {
                var result = await m_liveRoomApi.RoomLotteryInfo(roomId).Request();
                if (!result.status)
                {
                    throw new CustomizedErrorException(result.message);
                }

                var obj = await result.GetData<JObject>();
                if (!obj.success)
                {
                    throw new CustomizedErrorException(obj.message);
                }

                if (obj.data["anchor"].ToArray().Length > 0)
                {
                    var data = JsonConvert.DeserializeObject<LiveRoomAnchorLotteryInfoModel>(obj.data["anchor"].ToString());
                    AnchorLotteryInfo = data ?? throw new CustomizedErrorException("Anchor lottery data is null");
                    AnchorLotteryStart?.Invoke(this, AnchorLotteryInfo);
                    AnchorLotteryShow = true;
                    AnchorLotteryTimer.Start();
                }

                if (obj.data["popularity_red_pocket"].ToArray().Length > 0)
                {
                    var data = JsonConvert.DeserializeObject<ObservableCollection<LiveRoomRedPocketLotteryInfoModel>>(obj.data["popularity_red_pocket"].ToString());
                    RedPocketLotteryInfo = data?[0] ?? throw new CustomizedErrorException("RedPocket lottery data is null");

                    RedPocketLotteryShow = true;
                    RedPocketLotteryTimer.Start();
                }
            }
            catch (Exception ex)
            {
                Notify.ShowMessageToast("加载主播抽奖信息失败");
                _logger.Log("加载主播抽奖信息失败", LogType.Error, ex);
            }
        }

        public void SetAnchorLotteryInfo(LiveRoomAnchorLotteryInfoModel info)
        {
            AnchorLotteryInfo = info;
            AnchorLotteryShow = true;
            AnchorLotteryTimer.Start();
        }

        public void SetRedPocketLotteryInfo(LiveRoomRedPocketLotteryInfoModel info)
        {
            RedPocketLotteryInfo = info;
            RedPocketLotteryShow = true;
            RedPocketLotteryTimer.Start();
        }

        #endregion
    }
}