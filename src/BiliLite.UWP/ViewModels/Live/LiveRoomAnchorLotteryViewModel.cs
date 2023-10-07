using System;
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
    public class LiveRoomAnchorLotteryViewModel : BaseViewModel
    {
        #region Fields

        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly LiveRoomAPI m_liveRoomApi;

        #endregion

        #region Constructors

        public LiveRoomAnchorLotteryViewModel()
        {
            m_liveRoomApi = new LiveRoomAPI();
            Timer = new Timer(1000);
            Timer.Elapsed += Timer_Elapsed;
        }

        #endregion

        #region Properties

        [DoNotNotify]
        public Timer Timer { get; set; }

        public LiveRoomAnchorLotteryInfoModel LotteryInfo { get; set; }

        public string DownTime { get; set; } = "--:--";

        public bool End { get; set; }

        public bool Show { get; set; }

        #endregion

        #region Private Methods

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (LotteryInfo == null) return;
                if (LotteryInfo.Time <= 0)
                {
                    End = false;
                    Timer.Stop();
                    DownTime = "已开奖";
                    Show = false;
                    LotteryInfo = null;
                    return;
                }
                var time = TimeSpan.FromSeconds(LotteryInfo.Time);
                DownTime = time.ToString(@"mm\:ss");
                LotteryInfo.Time--;
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

                var data = JsonConvert.DeserializeObject<LiveRoomAnchorLotteryInfoModel>(obj.data["anchor"]
                    .ToString());

                LotteryInfo = data ?? throw new CustomizedErrorException("result data is null");
                Show = true;
                Timer.Start();
            }
            catch (Exception ex)
            {
                _logger.Log("加载主播抽奖信息失败", LogType.Error, ex);
            }
        }

        public void SetLotteryInfo(LiveRoomAnchorLotteryInfoModel info)
        {
            LotteryInfo = info;
            Show = true;
            Timer.Start();
        }

        #endregion
    }
}