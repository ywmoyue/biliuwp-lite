using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using BiliLite.Extensions;
using BiliLite.Models;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Live;
using BiliLite.Models.Common.Player;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api;
using BiliLite.Models.Requests.Api.Live;
using BiliLite.Modules;
using BiliLite.Modules.Live;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using DateTime = System.DateTime;

namespace BiliLite.ViewModels.Live
{
    public class LiveRoomViewModel : BaseViewModel
    {
        #region Fields

        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly LiveRoomAPI m_liveRoomApi;
        private readonly PlayerAPI m_playerApi;
        private System.Threading.CancellationTokenSource m_cancelSource;
        private Modules.Live.LiveMessage m_liveMessage;
        private readonly Timer m_timerBox;
        private readonly Timer m_timer;
        private readonly LiveMessageHandleActionsMap m_messageHandleActionsMap;

        #endregion

        #region Constructors

        public LiveRoomViewModel()
        {
            m_liveRoomApi = new LiveRoomAPI();
            m_playerApi = new PlayerAPI();
            //liveMessage = new Live.LiveMessage();
            LotteryViewModel = new LiveRoomLotteryViewModel();
            MessageCenter.LoginedEvent += MessageCenter_LoginedEvent;
            MessageCenter.LogoutedEvent += MessageCenter_LogoutedEvent;
            Logined = SettingService.Account.Logined;
            Messages = new ObservableCollection<DanmuMsgModel>();
            GiftMessage = new ObservableCollection<GiftMsgModel>();
            Guards = new ObservableCollection<LiveGuardRankItem>();
            BagGifts = new ObservableCollection<LiveGiftItem>();
            SuperChats = new ObservableCollection<SuperChatMsgViewModel>();
            m_timer = new Timer(1000);
            m_timerBox = new Timer(1000);
            TimerAutoHideGift = new Timer(1000);
            m_timer.Elapsed += Timer_Elapsed;
            m_timerBox.Elapsed += Timer_box_Elapsed;
            TimerAutoHideGift.Elapsed += Timer_auto_hide_gift_Elapsed;
            m_messageHandleActionsMap = InitLiveMessageHandleActionMap();

            LoadMoreGuardCommand = new RelayCommand(LoadMoreGuardList);
            ShowBagCommand = new RelayCommand(SetShowBag);
            RefreshBagCommand = new RelayCommand(RefreshBag);
        }

        #endregion

        #region Properties

        public ICommand LoadMoreGuardCommand { get; private set; }

        public ICommand ShowBagCommand { get; private set; }

        public ICommand RefreshBagCommand { get; private set; }

        [DoNotNotify]
        public int HideGiftFlag { get; set; } = 1;

        [DoNotNotify]
        public List<LiveGiftItem> AllGifts { get; set; } = new List<LiveGiftItem>();

        [DoNotNotify]
        public Timer TimerAutoHideGift { get; private set; }

        public LiveRoomLotteryViewModel LotteryViewModel { get; set; }

        [DoNotNotify]
        public static List<LiveTitleModel> Titles { get; set; }

        public bool Logined { get; set; }

        /// <summary>
        /// 直播ID
        /// </summary>
        [DoNotNotify]
        public int RoomID { get; set; }

        /// <summary>
        /// 房间标题
        /// </summary>
        public string RoomTitle { get; set; }

        [DoNotNotify]
        public ObservableCollection<DanmuMsgModel> Messages { get; set; }

        [DoNotNotify]
        public ObservableCollection<GiftMsgModel> GiftMessage { get; set; }

        [DoNotNotify]
        public ObservableCollection<LiveGiftItem> BagGifts { get; set; }

        [DoNotNotify]
        public bool ReceiveWelcomeMsg { get; set; } = true;

        [DoNotNotify]
        public bool ReceiveLotteryMsg { get; set; } = true;

        [DoNotNotify]
        public bool ReceiveGiftMsg { get; set; } = true;

        public bool ShowGiftMessage { get; set; }

        /// <summary>
        /// 看过的人数(替代人气值)
        /// </summary>
        public string WatchedNum { get; set; }

        public bool Loading { get; set; } = true;

        public bool Attention { get; set; }

        public bool ShowBag { get; set; }

        public List<LiveRoomRankViewModel> Ranks { get; set; }
        
        public LiveRoomRankViewModel SelectRank { get; set; }

        [DoNotNotify]
        public ObservableCollection<LiveGuardRankItem> Guards { get; set; }

        [DoNotNotify]
        public ObservableCollection<SuperChatMsgViewModel> SuperChats { get; set; }

        [DoNotNotify]
        public LiveRoomWebUrlQualityDescriptionItemModel CurrentQn { get; set; }

        public List<LiveRoomWebUrlQualityDescriptionItemModel> Qualites { get; set; }

        public List<LiveGiftItem> Gifts { get; set; }

        public List<LiveBagGiftItem> Bag { get; set; }

        [DoNotNotify]
        public List<BasePlayUrlInfo> HlsUrls { get; set; }

        [DoNotNotify]
        public List<BasePlayUrlInfo> FlvUrls { get; set; }

        public LiveWalletInfo WalletInfo { get; set; }

        public LiveInfoModel LiveInfo { get; set; }

        public LiveAnchorProfile Profile { get; set; }

        public bool Liveing { get; set; }

        public string LiveTime { get; set; }

        [DoNotNotify]
        public int CleanCount { get; set; } = 200;

        [DoNotNotify]
        public int GuardPage { get; set; } = 1;

        public bool LoadingGuard { get; set; } = true;

        public bool LoadMoreGuard { get; set; }

        public bool ShowBox { get; set; }

        public bool OpenBox { get; set; }

        public string BoxTime { get; set; } = "--:--";

        [DoNotNotify]
        public DateTime freeSilverTime { get; set; }

        [DoNotNotify]
        public bool AutoReceiveFreeSilver { get; set; }

        public bool ShowRedPocketLotteryWinnerList { get; set; } = false;

        public bool ShowAnchorLotteryWinnerList { get; set; } = false;

        public string RedPocketSendDanmuBtnText { get; set; } = "一键关注并发送弹幕";

        #endregion

        #region Events

        public event EventHandler ChangedPlayUrl;

        public event EventHandler<LiveRoomEndAnchorLotteryInfoModel> AnchorLotteryEnd;

        public event EventHandler<DanmuMsgModel> AddNewDanmu;

        public event EventHandler ChatScrollToEnd;

        public event EventHandler<LiveRoomEndRedPocketLotteryInfoModel> RedPocketLotteryEnd;

        public event EventHandler<LiveAnchorInfoLiveInfoModel> AnchorLotteryStart;

        #endregion

        #region Private Methods

        private LiveMessageHandleActionsMap InitLiveMessageHandleActionMap()
        {
            var actionMap = new LiveMessageHandleActionsMap();
            actionMap.AddNewDanmu += (_, e) =>
            {
                AddNewDanmu?.Invoke(this, e);
            }; 
            actionMap.AnchorLotteryEnd += (_, e) =>
            {
                AnchorLotteryEnd?.Invoke(this, e);
            };
            actionMap.RedPocketLotteryEnd += (_, e) =>
            {
                RedPocketLotteryEnd?.Invoke(this, e);
            };
            return actionMap;
        }

        private void LiveMessage_NewMessage(MessageType type, object message)
        {
            if (Messages == null) return;

            var success = m_messageHandleActionsMap.Map.TryGetValue(type, out var handler);
            if (!success) return;

            handler(this, message);
        }

        private async void Timer_auto_hide_gift_Elapsed(object sender, ElapsedEventArgs e)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (GiftMessage == null || GiftMessage.Count == 0) return;
                if (HideGiftFlag >= 5)
                {
                    ShowGiftMessage = false;
                    GiftMessage.Clear();
                }
                else
                {
                    HideGiftFlag++;
                    ChatScrollToEnd?.Invoke(null, null);
                }
            });
        }

        private void MessageCenter_LogoutedEvent(object sender, EventArgs e)
        {
            Logined = false;
            m_timerBox.Stop();
            ShowBox = false;
            OpenBox = false;
        }

        private async void MessageCenter_LoginedEvent(object sender, object e)
        {
            Logined = true;
            await LoadWalletInfo();
            await LoadBag();
            //await GetFreeSilverTime();
        }

        private async void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (LiveInfo == null && !Liveing)
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        LiveTime = "";
                    });
                    return;
                }
                var startTime = TimeExtensions.TimestampToDatetime(LiveInfo.RoomInfo.LiveStartTime);
                var ts = DateTime.Now - startTime;

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    for (var i = 0; i < SuperChats.Count; i++)
                    {
                        var item = SuperChats[i];
                        if (item.Time <= 0)
                        {
                            SuperChats.Remove(item);
                        }
                        else
                        {
                            item.Time -= 1;
                        }
                    }

                    LiveTime = ts.ToString(@"hh\:mm\:ss");
                });
            }
            catch (Exception ex)
            {
                _logger.Warn(ex.Message, ex);
            }
        }

        private async void ReceiveMessage(int roomId)
        {
            try
            {
                var uid = 0;
                if (SettingService.Account.Logined)
                {
                    uid = SettingService.Account.UserID;
                }
                m_liveMessage ??= new LiveMessage();

                var buvidResults = await m_liveRoomApi.GetBuvid().Request();
                var buvidData = await buvidResults.GetJson<ApiDataModel<LiveBuvidModel>>();
                var buvid = buvidData.data.B3;

                var danmuResults = await m_liveRoomApi.GetDanmuInfo(roomId).Request();
                var danmuData = await danmuResults.GetJson<ApiDataModel<LiveDanmukuInfoModel>>();
                var token = danmuData.data.Token;
                var host = danmuData.data.HostList[0].Host;

                await m_liveMessage.Connect(roomId, uid, token, buvid, host, m_cancelSource.Token);
            }
            catch (TaskCanceledException)
            {
                Messages.Add(new DanmuMsgModel()
                {
                    UserName = "取消连接"
                });
            }
            catch (Exception ex)
            {
                Messages?.Add(new DanmuMsgModel()
                {
                    UserName = "连接失败:" + ex.Message
                });
            }

        }

        private async void Timer_box_Elapsed(object sender, ElapsedEventArgs e)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                if (DateTime.Now >= freeSilverTime)
                {
                    ShowBox = false;
                    OpenBox = true;
                    m_timerBox.Stop();
                    if (AutoReceiveFreeSilver)
                    {
                        await GetFreeSilver();
                    }
                }
                else
                {
                    OpenBox = false;
                    BoxTime = (freeSilverTime - DateTime.Now).ToString(@"mm\:ss");
                }
            });
        }

        private void SetShowBag()
        {
            if (!ShowBag && !SettingService.Account.Logined)
            {
                Notify.ShowMessageToast("请先登录");
                return;
            }
            ShowBag = !ShowBag;
        }

        private async void RefreshBag()
        {
            if (!SettingService.Account.Logined)
            {
                Notify.ShowMessageToast("请先登录");
                return;
            }
            await LoadBag();
        }

        private List<BasePlayUrlInfo> GetSpecialPlayUrls(LiveRoomPlayUrlModel liveRoomPlayUrlModel, string protocolName)
        {
            LiveRoomWebUrlStreamItemModel stream = null;
            if (liveRoomPlayUrlModel.PlayUrlInfo.PlayUrl.Stream.Any(item => item.ProtocolName == protocolName))
            {
                stream = liveRoomPlayUrlModel.PlayUrlInfo.PlayUrl.Stream.FirstOrDefault(item => item.ProtocolName == protocolName);
            }
            else
            {
                return null;
            }

            var codecList = stream.Format[0].Codec;

            var routeIndex = 1;
            foreach (var item in codecList.SelectMany(codecItem => codecItem.UrlInfo))
            {
                item.Name = "线路" + routeIndex;
                routeIndex++;
            }

            // 暂时不使用hevc流
            var codec = codecList.FirstOrDefault(item => item.CodecName == "avc");

            var acceptQnList = codec.AcceptQn;
            Qualites ??= liveRoomPlayUrlModel.PlayUrlInfo.PlayUrl.GQnDesc.Where(item => acceptQnList.Contains(item.Qn)).ToList();
            CurrentQn = liveRoomPlayUrlModel.PlayUrlInfo.PlayUrl.GQnDesc.FirstOrDefault(x => x.Qn == codec.CurrentQn);

            var urlList = codec.UrlInfo.Select(urlInfo => new BasePlayUrlInfo
                { Url = urlInfo.Host + codec.BaseUrl + urlInfo.Extra, Name = urlInfo.Name }).ToList();

            return urlList;
        }

        private List<BasePlayUrlInfo> GetHlsPlayUrls(LiveRoomPlayUrlModel liveRoomPlayUrlModel)
        {
            return GetSpecialPlayUrls(liveRoomPlayUrlModel, "http_hls");
        }

        private List<BasePlayUrlInfo> GetFlvPlayUrls(LiveRoomPlayUrlModel liveRoomPlayUrlModel)
        {
            return GetSpecialPlayUrls(liveRoomPlayUrlModel, "http_stream");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 读取直播头衔
        /// </summary>
        /// <returns></returns>
        public async Task GetTitles()
        {
            try
            {
                if (Titles != null)
                {
                    return;
                }
                var results = await m_liveRoomApi.LiveTitles().Request();
                if (!results.status)
                {
                    return;
                }

                var data = await results.GetData<List<LiveTitleModel>>();
                if (data.success)
                {
                    Titles = data.data;
                }
            }
            catch (Exception ex)
            {
                _logger.Log("读取直播头衔失败", LogType.Fatal, ex);
            }
        }

        public async Task GetPlayUrls(int roomId, int qn = 0)
        {
            try
            {
                Loading = true;
                var results = await m_playerApi.LivePlayUrl(roomId.ToString(), qn).Request();
                if (!results.status)
                {
                    throw new CustomizedErrorException(results.message);
                }
                var data = await results.GetJson<ApiDataModel<LiveRoomPlayUrlModel>>();

                if (!data.success)
                {
                    throw new CustomizedErrorException(data.message);
                }

                HlsUrls = GetHlsPlayUrls(data.data);
                FlvUrls = GetFlvPlayUrls(data.data);
                ChangedPlayUrl?.Invoke(this, null);
            }
            catch (Exception ex)
            {
                Notify.ShowMessageToast($"读取播放地址失败:{ex.Message}");
                _logger.Error($"读取播放地址失败:{ex.Message}", ex);
            }
            finally
            {
                Loading = false;
            }
        }

        /// <summary>
        /// 读取直播间详细信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task LoadLiveRoomDetail(string id)
        {
            try
            {
                if (m_cancelSource != null)
                {
                    m_cancelSource.Cancel();
                    m_cancelSource.Dispose();
                }
                if (m_liveMessage != null)
                {
                    m_liveMessage.NewMessage -= LiveMessage_NewMessage;
                    m_liveMessage = null;

                }
                m_liveMessage = new LiveMessage();
                m_liveMessage.NewMessage += LiveMessage_NewMessage;
                m_cancelSource = new System.Threading.CancellationTokenSource();

                Loading = true;
                var result = await m_liveRoomApi.LiveRoomInfo(id).Request();
                if (!result.status)
                {
                    throw new CustomizedErrorException(result.message);
                }

                var data = await result.GetData<LiveInfoModel>();
                if (!data.success)
                {
                    throw new CustomizedErrorException("加载直播间失败:" + data.message);
                }

                RoomID = data.data.RoomInfo.RoomId;
                RoomTitle = data.data.RoomInfo.Title;
                //WatchedNum = data.data.RoomInfo.Online;
                Liveing = data.data.RoomInfo.LiveStatus == 1;
                LiveInfo = data.data;
                if (Ranks == null)
                {
                    Ranks = new List<LiveRoomRankViewModel>()
                    {
                        //new LiveRoomRankViewModel(RoomID, data.data.RoomInfo.Uid, "金瓜子榜", "gold-rank"),
                        //new LiveRoomRankViewModel(RoomID, data.data.RoomInfo.Uid, "今日礼物榜", "today-rank"),
                        //new LiveRoomRankViewModel(RoomID, data.data.RoomInfo.Uid, "七日礼物榜", "seven-rank"),
                        new LiveRoomRankViewModel(RoomID, data.data.RoomInfo.Uid, "高能用户贡献榜", "contribution-rank"),
                        new LiveRoomRankViewModel(RoomID, data.data.RoomInfo.Uid, "粉丝榜", "fans"),
                    };
                    SelectRank = Ranks[0];
                }


                await LoadAnchorProfile();
                if (Liveing)
                {
                    m_timer.Start();
                    await GetPlayUrls(RoomID,
                        SettingService.GetValue(SettingConstants.Live.DEFAULT_QUALITY, 10000));
                    //GetFreeSilverTime();  
                    await LoadSuperChat();
                    if (ReceiveLotteryMsg)
                    {
                        // 抽奖
                        LotteryViewModel.LoadLotteryInfo(RoomID).RunWithoutAwait();
                        RedPocketSendDanmuBtnText = Attention ? "一键发送弹幕" : "一键关注并发送弹幕";
                    }
                }

                await GetRoomGiftList();
                await LoadBag();
                await LoadWalletInfo();
                if (Titles == null)
                {
                    await GetTitles();
                }

                EntryRoom();
                ReceiveMessage(data.data.RoomInfo.RoomId);
            }
            catch (CustomizedErrorException ex)
            {
                Notify.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                var result = HandelError<object>(ex);
                Notify.ShowMessageToast(ex.Message);
            }
            finally
            {
                Loading = false;
            }
        }

        /// <summary>
        /// 读取醒目留言
        /// </summary>
        /// <returns></returns>
        public async Task LoadSuperChat()
        {
            try
            {
                var result = await m_liveRoomApi.RoomSuperChat(RoomID).Request();
                if (!result.status)
                {
                    throw new CustomizedErrorException(result.message);
                }

                var data = await result.GetData<JObject>();
                if (!data.success)
                {
                    throw new CustomizedErrorException("读取醒目留言失败:" + data.message);
                }

                SuperChats.Clear();
                var ls = JsonConvert.DeserializeObject<List<LiveRoomSuperChatModel>>(
                    data.data["list"]?.ToString() ?? "[]");
                foreach (var item in ls)
                {
                    SuperChats.Add(new SuperChatMsgViewModel()
                    {
                        BackgroundBottomColor = item.BackgroundBottomColor,
                        BackgroundColor = item.BackgroundColor,
                        BackgroundImage = item.BackgroundImage,
                        EndTime = item.EndTime,
                        Face = item.UserInfo.Face,
                        FaceFrame = item.UserInfo.FaceFrame,
                        FontColor = string.IsNullOrEmpty(item.FontColor) ? "#FFFFFF" : item.FontColor,
                        MaxTime = item.EndTime - item.StartTime,
                        Message = item.Message,
                        Price = item.Price,
                        StartTime = item.StartTime,
                        Time = item.Time,
                        Username = item.UserInfo.Uname
                    });
                }
            }
            catch (CustomizedErrorException ex)
            {
                Notify.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                var result = HandelError<object>(ex);
                Notify.ShowMessageToast(ex.Message);
            }
        }

        /// <summary>
        /// 进入房间
        /// </summary>
        public async void EntryRoom()
        {
            try
            {
                if (SettingService.Account.Logined)
                {
                    await m_liveRoomApi.RoomEntryAction(RoomID).Request();
                }
            }
            catch (Exception ex)
            {
                _logger.Warn(ex.Message, ex);
            }

        }

        /// <summary>
        /// 读取钱包信息
        /// </summary>
        /// <returns></returns>
        public async Task LoadWalletInfo()
        {
            try
            {
                if (!Logined)
                {
                    return;
                }
                var result = await m_liveRoomApi.MyWallet().Request();
                if (!result.status)
                {
                    throw new CustomizedErrorException(result.message);
                }

                var data = await result.GetData<LiveWalletInfo>();
                if (!data.success)
                {
                    throw new CustomizedErrorException("读取钱包失败:" + data.message);
                }

                WalletInfo = data.data;
            }
            catch (CustomizedErrorException ex)
            {
                Notify.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                var result = HandelError<object>(ex);
                Notify.ShowMessageToast(ex.Message);
            }
        }

        /// <summary>
        /// 读取背包礼物
        /// </summary>
        /// <returns></returns>
        public async Task LoadBag()
        {
            try
            {
                if (!Logined)
                {
                    return;
                }
                var result = await m_liveRoomApi.GiftList(LiveInfo.RoomInfo.AreaId, LiveInfo.RoomInfo.ParentAreaId, RoomID).Request();
                if (!result.status) return;

                var data = await result.GetData<JObject>();
                if (!data.success) return;
                var list = JsonConvert.DeserializeObject<List<LiveGiftItem>>(data.data["list"].ToString());

                var bagResult = await m_liveRoomApi.BagList(RoomID).Request();
                if (!bagResult.status)
                {
                    throw new CustomizedErrorException(bagResult.message);
                }

                var bagData = await bagResult.GetData<JObject>();
                if (!bagData.success)
                {
                    throw new CustomizedErrorException("读取背包失败:" + bagData.message);
                }

                BagGifts.Clear();
                var ls = JsonConvert.DeserializeObject<List<LiveBagGiftItem>>(
                    bagData.data["list"]?.ToString() ?? "[]");
                if (ls != null)
                    foreach (var item in ls)
                    {
                        var _gift = list.FirstOrDefault(x => x.Id == item.GiftId);
                        var gift = _gift.ObjectClone();
                        gift.GiftNum = item.GiftNum;
                        gift.CornerMark = item.CornerMark;
                        gift.BagId = item.BagId;
                        BagGifts.Add(gift);
                    }
                //WalletInfo = data.data;
            }
            catch (CustomizedErrorException ex)
            {
                Notify.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                var result = HandelError<object>(ex);
                Notify.ShowMessageToast(ex.Message);
            }
        }

        /// <summary>
        /// 读取主播资料
        /// </summary>
        /// <returns></returns>
        public async Task LoadAnchorProfile()
        {
            try
            {
                var result = await m_liveRoomApi.AnchorProfile(LiveInfo.RoomInfo.Uid).Request();
                if (!result.status)
                {
                    throw new CustomizedErrorException(result.message);
                }

                var data = await result.GetData<LiveAnchorProfile>();
                if (!data.success)
                {
                    throw new CustomizedErrorException("读取主播信息失败:" + data.message);
                }

                Profile = data.data;
                Attention = Profile.RelationStatus > 1;
            }
            catch (CustomizedErrorException ex)
            {
                Notify.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                var result = HandelError<object>(ex);
                Notify.ShowMessageToast(ex.Message);
            }
        }

        /// <summary>
        /// 读取直播间可用礼物列表
        /// </summary>
        /// <returns></returns>
        public async Task GetRoomGiftList()
        {
            try
            {
                var result = await m_liveRoomApi.GiftList(LiveInfo.RoomInfo.AreaId, LiveInfo.RoomInfo.ParentAreaId, RoomID).Request();
                if (!result.status) return;
                var data = await result.GetData<JObject>();
                if (!data.success) return;
                var list = JsonConvert.DeserializeObject<List<LiveGiftItem>>(data.data["list"].ToString());
                if (AllGifts == null || AllGifts.Count == 0)
                {
                    AllGifts = list;
                }

                var resultRoom = await m_liveRoomApi
                    .RoomGifts(LiveInfo.RoomInfo.AreaId, LiveInfo.RoomInfo.ParentAreaId, RoomID).Request();
                if (!resultRoom.status) return;
                var dataRoom = await resultRoom.GetData<JObject>();
                var listRoom =
                    JsonConvert.DeserializeObject<List<LiveRoomGiftItem>>(dataRoom.data["list"]
                        .ToString());
                var liveGiftItems = new List<LiveGiftItem>()
                {
                    list.FirstOrDefault(x => x.Id == 1)
                };
                liveGiftItems.AddRange(listRoom.Select(item => list.FirstOrDefault(x => x.Id == item.GiftId)));

                Gifts = liveGiftItems;
            }
            catch (Exception ex)
            {
                Notify.ShowMessageToast("读取礼物信息失败");
                _logger.Log("读取礼物信息失败", LogType.Error, ex);
            }
        }

        /// <summary>
        /// 读取舰队信息
        /// </summary>
        /// <returns></returns>
        public async Task GetGuardList()
        {
            try
            {
                LoadingGuard = true;
                LoadMoreGuard = false;
                var result = await m_liveRoomApi.GuardList(LiveInfo.RoomInfo.Uid, RoomID, GuardPage).Request();
                if (!result.status) return;
                var data = await result.GetData<JObject>();
                if (!data.success) return;
                var guardNum = data.data["info"]["num"].ToInt32();
                LiveInfo.GuardInfo.Count = guardNum; // 更新显示数字(似乎不生效...)

                var top3 = JsonConvert.DeserializeObject<List<LiveGuardRankItem>>(data.data["top3"].ToString());
                if (Guards.Count == 0 && top3 != null && top3.Count != 0 && Guards.Count < guardNum)
                {
                    foreach (var item in top3)
                    {
                        Guards.Add(item);
                    }
                }

                var list = JsonConvert.DeserializeObject<List<LiveGuardRankItem>>(data.data["list"].ToString());
                if (list != null && list.Count != 0 && Guards.Count < guardNum)
                {
                    foreach (var item in list)
                    {
                        Guards.Add(item);
                    }
                }

                if (GuardPage >= data.data["info"]["page"].ToInt32())
                {
                    LoadMoreGuard = false;
                }
                else
                {
                    LoadMoreGuard = true;
                    GuardPage++;
                }
            }
            catch (Exception ex)
            {
                Notify.ShowMessageToast("读取舰队失败");
                _logger.Log("读取舰队失败", LogType.Error, ex);
            }
            finally
            {
                LoadingGuard = false;
            }
        }

        /// <summary>
        /// 加载更多舰队信息
        /// </summary>
        public async void LoadMoreGuardList()
        {
            if (LoadingGuard)
            {
                return;
            }
            await GetGuardList();
        }

        public async Task GetFreeSilverTime()
        {
            try
            {
                if (!SettingService.Account.Logined)
                {
                    ShowBox = false;
                    OpenBox = false;
                    return;
                }
                OpenBox = false;
                var result = await m_liveRoomApi.FreeSilverTime().Request();
                if (!result.status) return;
                var data = await result.GetData<JObject>();
                if (!data.success) return;
                ShowBox = true;
                freeSilverTime =
                    TimeExtensions.TimestampToDatetime(Convert.ToInt64(data.data["time_end"].ToString()));
                m_timerBox.Start();
            }
            catch (Exception ex)
            {
                Notify.ShowMessageToast("读取直播免费瓜子时间失败");
                _logger.Log("读取直播免费瓜子时间失败", LogType.Error, ex);
            }
        }

        public async Task GetFreeSilver()
        {
            try
            {
                if (!SettingService.Account.Logined)
                {
                    return;
                }
                var result = await m_liveRoomApi.GetFreeSilver().Request();
                if (!result.status) return;
                var data = await result.GetData<JObject>();
                if (data.success)
                {
                    Notify.ShowMessageToast("宝箱领取成功,瓜子+" + data.data["awardSilver"]);
                    //GetFreeSilverTime();
                    await LoadWalletInfo();
                }
                else
                {
                    await GetFreeSilverTime();
                }
            }
            catch (Exception ex)
            {
                Notify.ShowMessageToast("读取直播免费瓜子时间失败");
                _logger.Log("读取直播免费瓜子时间失败", LogType.Error, ex);
            }
        }

        public async Task SendGift(LiveGiftItem liveGiftItem)
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录");
                return;
            }
            try
            {
                var result = await m_liveRoomApi.SendGift(LiveInfo.RoomInfo.Uid, liveGiftItem.Id, liveGiftItem.CoinType, liveGiftItem.Num, RoomID, liveGiftItem.Price).Request();
                if (!result.status)
                {
                    throw new CustomizedErrorException(result.message);
                }

                var data = await result.GetData<JObject>();
                if (!data.success)
                {
                    throw new CustomizedErrorException(data.message);
                }

                Notify.ShowMessageToast(data.data?["send_tips"].ToString().Length > 0 ? data.data?["send_tips"].ToString() : "赠送成功"); // 鬼知道怎么有时候有返回提示有时候没有
                await LoadWalletInfo();
            }
            catch (CustomizedErrorException ex)
            {
                Notify.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.Log("赠送礼物出现错误", LogType.Error, ex);
                Notify.ShowMessageToast("赠送礼物出现错误");
            }

        }

        public async Task SendBagGift(LiveGiftItem liveGiftItem)
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录");
                return;
            }
            try
            {
                var result = await m_liveRoomApi.SendBagGift(LiveInfo.RoomInfo.Uid, liveGiftItem.Id, liveGiftItem.Num, liveGiftItem.BagId, RoomID).Request();
                if (!result.status)
                {
                    throw new CustomizedErrorException(result.message);
                }

                var data = await result.GetData<JObject>();
                if (!data.success)
                {
                    throw new CustomizedErrorException(data.message);
                }
                Notify.ShowMessageToast(data.data?["send_tips"].ToString().Length > 0 ? data.data?["send_tips"].ToString() : "赠送成功");
                await LoadBag();
            }
            catch (CustomizedErrorException ex)
            {
                Notify.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.Log("赠送礼物出现错误", LogType.Error, ex);
                Notify.ShowMessageToast("赠送礼物出现错误");
            }

        }

        public async Task<bool> SendDanmu(string text)
        {
            if (!SettingService.Account.Logined && !await Notify.ShowLoginDialog())
            {
                Notify.ShowMessageToast("请先登录");
                return false;
            }
            try
            {
                var result = await m_liveRoomApi.SendDanmu(text, RoomID).Request();
                if (!result.status)
                {
                    throw new CustomizedErrorException(result.message);
                }

                var data = await result.GetData<object>();
                if (!data.success)
                {
                    throw new CustomizedErrorException(data.message);
                }

                return true;
            }
            catch (CustomizedErrorException ex)
            {
                Notify.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
                return false;
            }
            catch (Exception ex)
            {
                _logger.Log("发送弹幕出现错误", LogType.Error, ex);
                Notify.ShowMessageToast("发送弹幕出现错误");
                return false;
            }
        }

        public void Dispose()
        {
            m_cancelSource?.Cancel();
            m_liveMessage?.Dispose();

            m_timer?.Stop();
            m_timerBox?.Stop();
            TimerAutoHideGift?.Stop();
            if (LotteryViewModel != null)
            {
                LotteryViewModel.AnchorLotteryTimer.Stop();
                LotteryViewModel.RedPocketLotteryTimer.Stop();
                LotteryViewModel = null;
            }

            Messages?.Clear();
            Messages = null;
            GiftMessage?.Clear();
            GiftMessage = null;
            Guards?.Clear();
            Guards = null;
        }

        public void EmitSelectRankUpdate()
        {
            Set(nameof(SelectRank));
        }

        //public void SetDelay(int ms)
        //{
        //    if (liveDanmaku != null)
        //    {
        //        liveDanmaku.delay = ms;
        //    }
        //}

        #endregion
    }
}
