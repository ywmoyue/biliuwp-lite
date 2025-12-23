using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Live;
using BiliLite.Models.Common.Player;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api;
using BiliLite.Models.Requests.Api.Live;
using BiliLite.Models.Responses;
using BiliLite.Modules;
using BiliLite.Modules.Live;
using BiliLite.Services;
using BiliLite.ViewModels.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
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
        private readonly Timer m_timerLiveTime;
        private readonly Timer m_timerSuperChat;
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
            m_timerLiveTime = new Timer(1000);
            m_timerSuperChat = new Timer(1000);
            m_timerBox = new Timer(1000);
            TimerAutoHideGift = new Timer(1000);
            m_timerLiveTime.Elapsed += TimerLiveTimeElapsed;
            m_timerSuperChat.Elapsed += TimerSuperChatElapsed;
            m_timerBox.Elapsed += Timer_box_Elapsed;
            TimerAutoHideGift.Elapsed += Timer_auto_hide_gift_Elapsed;
            LotteryViewModel.AddLotteryShieldWord += (_, e) => AddLotteryShieldWord?.Invoke(_, e);
            LotteryViewModel.AnchorLotteryStart += (_, e) => AnchorLotteryStart?.Invoke(_, e);
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
        /// 主播uid
        /// </summary>
        [DoNotNotify]
        public long AnchorUid { get; set; }

        /// <summary>
        /// 房间标题
        /// </summary>
        public string RoomTitle { get; set; }

        /// <summary>
        /// buvid3 用于防止风控
        /// </summary>
        [DoNotNotify]
        public string Buvid3 { get; set; }

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

        [DoNotNotify]
        public bool ShowLotteryDanmu { get; set; } = true;

        public bool ShowGiftMessage { get; set; }

        /// <summary>
        /// 房间在线观众数量与看过的人数(替代人气值)
        /// </summary>
        public string ViewerNumCount { get; set; }

        /// <summary>
        /// 舰长数
        /// </summary>
        public int GuardNum { get; set; }

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

        public ObservableCollection<LiveRoomEmoticonPackage> EmoticonsPackages { get; set; }

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

        /// <summary>
        /// 远程的直播状态, 不受本地播放情况影响.
        /// </summary>
        public bool Live { get; set; }

        public string LiveTime { get; set; } = "未开播";

        [DoNotNotify]
        public int CleanCount { get; set; } = 200;

        [DoNotNotify]
        public int GuardPage { get; set; } = 1;

        public bool LoadingGuard { get; set; }

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

        public string ManualPlayUrl { get; set; } = "";

        [DoNotNotify]
        public Dictionary<string, string> LotteryDanmu = new Dictionary<string, string> { { "AnchorLottery", "" }, { "RedPocketLottery", "" } };

        /// <summary>
        /// 有的特殊直播间没有一些娱乐内容. 例如央视新闻直播间.
        /// </summary>
        public bool IsSpecialLiveRoom;

        #endregion

        #region Events

        public event EventHandler ChangedPlayUrl;

        public event EventHandler<LiveRoomEndAnchorLotteryInfoModel> AnchorLotteryEnd;

        public event EventHandler<DanmuMsgModel> AddNewDanmu;

        public event EventHandler<LiveRoomEndRedPocketLotteryInfoModel> RedPocketLotteryEnd;

        public event EventHandler<LiveRoomAnchorLotteryInfoModel> AnchorLotteryStart;

        public event EventHandler<string> SetManualPlayUrl;

        public event EventHandler<string> AddLotteryShieldWord;

        public event EventHandler<string> DelShieldWord;

        public event EventHandler SpecialLiveRoomHideElements;

        public event EventHandler RefreshGuardNum;

        public event EventHandler StartLive;

        public event EventHandler StopLive;

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
            actionMap.AddShieldWord += (_, e) =>
            {
                AddLotteryShieldWord?.Invoke(this, e);
            };
            actionMap.DelShieldWord += (_, e) =>
            {
                DelShieldWord?.Invoke(this, e);
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

        private async void TimerLiveTimeElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                var startTime = TimeExtensions.TimestampToDatetime(LiveInfo.RoomInfo.LiveStartTime);
                var ts = DateTime.Now - startTime;

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    LiveTime = ts.ToString(@"hh\:mm\:ss");
                });
            }
            catch (Exception ex)
            {
                _logger.Warn(ex.Message, ex);
            }
        }

        private async void TimerSuperChatElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    for (var i = 0; i < SuperChats.Count; i++)
                    {
                        if (SuperChats.ElementAt(i).Time <= 0) SuperChats.RemoveAt(i);
                        else SuperChats.ElementAt(i).Time -= 1;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.Warn(ex.Message, ex);
            }
        }

        private async Task GetHistoryDanmu(int roomId, string buvid)
        {
            try
            {
                var result = await(await m_liveRoomApi.GetHistoryDanmu(roomId, buvid)).Request();
                if (!result.status) throw new CustomizedErrorException(result.message);
                var data = await result.GetJson<ApiDataModel<LiveRoomHistoryDanmu>>();
                if (!data.success) throw new CustomizedErrorException(result.message);

                foreach (var danmu in data.data.GetHistoryDanmuList())
                {
                    LiveMessage_NewMessage(MessageType.Danmu, danmu);
                }
            }
            catch (CustomizedErrorException ex)
            {
                Messages?.Add(new DanmuMsgModel()
                {
                    UserName = "获取历史弹幕失败:" + ex.Message
                });
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                Messages?.Add(new DanmuMsgModel()
                {
                    UserName = "获取历史弹幕失败:" + ex.Message
                });
                _logger.Error(ex.Message, ex);
            }
        }

        private async Task ReceiveMessage(int roomId, bool webMethod = true)
        {
            try
            {
                long uid = 0;
                if (SettingService.Account.Logined)
                {
                    uid = SettingService.Account.UserID;
                }
                m_liveMessage ??= new LiveMessage();

                HttpResults danmuResults;
                if (webMethod) danmuResults = await (await m_liveRoomApi.GetDanmuInfo(roomId, Buvid3)).Request();
                else danmuResults = await (await m_liveRoomApi.GetDanmuInfoApp(roomId)).Request();

                if (!danmuResults.status)
                {
                    if (!webMethod) throw new CustomizedErrorException( "所有API信息均获取失败: " + danmuResults.message);
                    _logger.Error("Web弹幕API信息获取失败, 切换到App方式, 错误信息: " + danmuResults.message);
                    await ReceiveMessage(roomId, false);
                    return;
                }
                var danmuData = await danmuResults.GetJson<ApiDataModel<LiveDanmukuInfoModel>>();

                if (!danmuData.success)
                {
                    if (!webMethod) throw new CustomizedErrorException("所有API信息均解析失败: " + danmuData.message);
                    _logger.Error("Web弹幕API信息解析失败, 切换到App方式, 错误信息: " + danmuData.message);
                    await ReceiveMessage(roomId, false);
                    return;
                }
                var token = danmuData.data.Token;
                var host = danmuData.data.HostList[0].Host;

                await m_liveMessage.Connect(roomId, uid, token, Buvid3, host, m_cancelSource.Token);
            }
            catch (TaskCanceledException)
            {
                Messages?.Add(new DanmuMsgModel()
                {
                    UserName = "取消连接"
                });
            }
            catch (CustomizedErrorException ex)
            {
                Messages?.Add(new DanmuMsgModel()
                {
                    UserName = "连接失败:" + ex.Message
                });
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                Messages?.Add(new DanmuMsgModel()
                {
                    UserName = "连接失败:" + ex.Message
                });
                _logger.Error(ex.Message, ex);
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
                NotificationShowExtensions.ShowMessageToast("请先登录");
                return;
            }
            ShowBag = !ShowBag;
        }

        private async void RefreshBag()
        {
            if (!SettingService.Account.Logined)
            {
                NotificationShowExtensions.ShowMessageToast("请先登录");
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

            // 暂时不使用hevc流, 但无法保证avc流一定存在
            var codec = codecList.Count > 1 ? codecList.FirstOrDefault(item => item.CodecName == "avc") : codecList[0];

            var acceptQnList = codec.AcceptQn;
            Qualites ??= liveRoomPlayUrlModel.PlayUrlInfo.PlayUrl.GQnDesc.Where(item => acceptQnList.Contains(item.Qn)).ToList();
            Qualites = [.. Qualites.OrderBy(x => x.Qn)];
            CurrentQn = liveRoomPlayUrlModel.PlayUrlInfo.PlayUrl.GQnDesc.FirstOrDefault(x => x.Qn == codec.CurrentQn);

            var urlList = codec.UrlInfo.Select(urlInfo => new BasePlayUrlInfo
            { Url = urlInfo.Host + codec.BaseUrl + urlInfo.Extra, Name = urlInfo.Name }).ToList();

            var regex = new Regex(@"live_\d+_\d+\.flv");
            foreach (var item in urlList)
            {
                if (regex.IsMatch(item.Url))
                {
                    SetManualPlayUrl?.Invoke(this, item.Url);
                    break;
                }
            }

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

        private async Task<bool> JoinRedPocketLotteryRequest(long uid, int room_id, long ruid, int lot_id)
        {
            if (!Logined && !await NotificationShowExtensions.ShowLoginDialog())
            {
                NotificationShowExtensions.ShowMessageToast("请先登录");
                return false;
            }
            try
            {
                var result = await m_liveRoomApi.JoinRedPocketLottery(uid, room_id, ruid, lot_id).Request();
                if (!result.status)
                {
                    throw new CustomizedErrorException(result.message);
                }

                var data = await result.GetData<JObject>();
                if (!data.success)
                {
                    throw new CustomizedErrorException(data.message);
                }

                if (data.data?["join_status"].ToString().Length > 0 && data.data?["join_status"].ToInt32() == 1) return true;
                else throw new CustomizedErrorException("未能成功加入红包抽奖");
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
                return false;
            }
            catch (Exception ex)
            {
                _logger.Log("参加红包抽奖出现错误", LogType.Error, ex);
                NotificationShowExtensions.ShowMessageToast("参加红包抽奖出现错误");
                return false;
            }
        }

        private async Task<bool> JoinAnchorLotteryRequest(int lottery_id, int gift_id, int gift_num)
        {
            if (!Logined && !await NotificationShowExtensions.ShowLoginDialog())
            {
                NotificationShowExtensions.ShowMessageToast("请先登录");
                return false;
            }
            try
            {
                var result = await m_liveRoomApi.JoinAnchorLottery(RoomID, lottery_id, Buvid3, gift_id, gift_num).Request();
                if (!result.status)
                {
                    throw new CustomizedErrorException(result.message);
                }

                var data = await result.GetData<JObject>();
                if (!data.success)
                {
                    throw new CustomizedErrorException(data.message);
                }

                return true;
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
                return false;
            }
            catch (Exception ex)
            {
                _logger.Log("参加天选抽奖出现错误", LogType.Error, ex);
                NotificationShowExtensions.ShowMessageToast("参加天选抽奖出现错误");
                return false;
            }
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
                NotificationShowExtensions.ShowMessageToast($"读取播放地址失败:{ex.Message}");
                _logger.Error($"读取播放地址失败:{ex.Message}", ex);
            }
            finally
            {
                Loading = false;
            }
        }

        /// <summary>
        /// 加载直播间详细信息
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

                LiveInfo = data.data;
                RoomID = LiveInfo.RoomInfo.RoomId;
                RoomTitle = LiveInfo.RoomInfo.Title;
                AnchorUid = LiveInfo.RoomInfo.Uid;
                Live = LiveInfo.RoomInfo.LiveStatus == 1;

                var buvidResults = await m_liveRoomApi.GetBuvid().Request();
                var buvidData = await buvidResults.GetJson<ApiDataModel<LiveBuvidModel>>();
                Buvid3 = buvidData.data.B3;

                await GetHistoryDanmu(RoomID, Buvid3);
                ReceiveMessage(LiveInfo.RoomInfo.RoomId).RunWithoutAwait(); // 连接弹幕优先级提高

                if(Live) await LiveStart();

                await GetEmoticons();

                m_timerSuperChat.Start();
                await LoadSuperChat();

                if (LiveInfo.GuardInfo == null)
                {
                    IsSpecialLiveRoom = true;
                    SpecialLiveRoomHideElements?.Invoke(this, null);
                }
                else
                {
                    GuardNum = !IsSpecialLiveRoom ? LiveInfo.GuardInfo.Count : 0;
                    await GetGuardList();
                }

                if (Ranks == null)
                {
                    Ranks =
                    [
                        new LiveRoomRankViewModel(RoomID, AnchorUid, "高能用户贡献榜", "contribution-rank"),
                        new LiveRoomRankViewModel(RoomID, AnchorUid, "粉丝榜", "fans"),
                    ];
                    SelectRank = Ranks[0];
                }

                await LoadAnchorProfile();

                //GetFreeSilverTime();  
                if (ReceiveLotteryMsg)
                {
                    // 天选抽奖和红包抽奖
                    LotteryViewModel.LoadLotteryInfo(RoomID, Buvid3).RunWithoutAwait();
                    RedPocketSendDanmuBtnText = Attention ? "一键发送弹幕" : "一键关注并发送弹幕";
                }

                await GetRoomGiftList();
                await LoadBag();
                await LoadWalletInfo();
                if (Titles == null)
                {
                    await GetTitles();
                }

                await EntryRoom();
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                var result = HandelError<object>(ex);
                NotificationShowExtensions.ShowMessageToast(ex.Message);
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
                if (ls == null) return;
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
                        Username = item.UserInfo.Uname,
                        GuardLevel = item.UserInfo.GuardLevel,
                        Uid = item.Uid
                    });
                }
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                var result = HandelError<object>(ex);
                NotificationShowExtensions.ShowMessageToast(ex.Message);
            }
        }

        /// <summary>
        /// 进入房间
        /// </summary>
        public async Task EntryRoom()
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
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                var result = HandelError<object>(ex);
                NotificationShowExtensions.ShowMessageToast(ex.Message);
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
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                var result = HandelError<object>(ex);
                //NotificationShowExtensions.ShowMessageToast(ex.Message);
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
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                var result = HandelError<object>(ex);
                NotificationShowExtensions.ShowMessageToast(ex.Message);
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
                NotificationShowExtensions.ShowMessageToast("读取礼物信息失败");
                _logger.Log("读取礼物信息失败", LogType.Error, ex);
            }
        }

        /// <summary>
        /// 读取舰队信息
        /// </summary>
        /// <returns></returns>
        public async Task GetGuardList()
        {
            if (IsSpecialLiveRoom) return;
            try
            {
                LoadingGuard = true;
                LoadMoreGuard = false;
                var result = await m_liveRoomApi.GuardList(LiveInfo.RoomInfo.Uid, RoomID, GuardPage).Request();
                if (!result.status) return;
                var data = await result.GetData<JObject>();
                if (!data.success) return;
                GuardNum = data.data["info"]["num"].ToInt32(); //更新舰长数

                var top3 = JsonConvert.DeserializeObject<List<LiveGuardRankItem>>(data.data["top3"].ToString());
                if (Guards.Count == 0 && top3 != null && top3.Count != 0 && Guards.Count < GuardNum)
                {
                    foreach (var item in top3)
                    {
                        Guards.Add(item);
                    }
                }

                var list = JsonConvert.DeserializeObject<List<LiveGuardRankItem>>(data.data["list"].ToString());
                if (list != null && list.Count != 0 && Guards.Count < GuardNum)
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
                NotificationShowExtensions.ShowMessageToast("读取舰队失败");
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

        /// <summary>
        /// 重新加载舰队信息
        /// </summary>
        public async Task ReloadGuardList()
        {
            if (LoadingGuard) { return; }
            Guards?.Clear();
            GuardPage = 1;
            await GetGuardList();
            RefreshGuardNum?.Invoke(null, null);
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
                NotificationShowExtensions.ShowMessageToast("读取直播免费瓜子时间失败");
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
                    NotificationShowExtensions.ShowMessageToast("宝箱领取成功,瓜子+" + data.data["awardSilver"]);
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
                NotificationShowExtensions.ShowMessageToast("读取直播免费瓜子时间失败");
                _logger.Log("读取直播免费瓜子时间失败", LogType.Error, ex);
            }
        }

        public async Task SendGift(LiveGiftItem liveGiftItem)
        {
            if (!SettingService.Account.Logined && !await NotificationShowExtensions.ShowLoginDialog())
            {
                NotificationShowExtensions.ShowMessageToast("请先登录");
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

                NotificationShowExtensions.ShowMessageToast(data.data?["send_tips"].ToString().Length > 0 ? data.data?["send_tips"].ToString() : "赠送成功"); // 鬼知道怎么有时候有返回提示有时候没有
                await LoadWalletInfo();
                await GetEmoticons(); // 礼物有可能刷新表情包
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.Log("赠送礼物出现错误", LogType.Error, ex);
                NotificationShowExtensions.ShowMessageToast("赠送礼物出现错误");
            }

        }

        public async Task SendBagGift(LiveGiftItem liveGiftItem)
        {
            if (!SettingService.Account.Logined && !await NotificationShowExtensions.ShowLoginDialog())
            {
                NotificationShowExtensions.ShowMessageToast("请先登录");
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
                NotificationShowExtensions.ShowMessageToast(data.data?["send_tips"].ToString().Length > 0 ? data.data?["send_tips"].ToString() : "赠送成功");
                await LoadBag();
                await GetEmoticons(); // 礼物有可能刷新表情包
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.Log("赠送礼物出现错误", LogType.Error, ex);
                NotificationShowExtensions.ShowMessageToast("赠送礼物出现错误");
            }

        }

        public async Task<bool> SendDanmu(string text)
        {
            if (!Logined && !await NotificationShowExtensions.ShowLoginDialog())
            {
                NotificationShowExtensions.ShowMessageToast("请先登录");
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
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
                return false;
            }
            catch (Exception ex)
            {
                _logger.Log("发送弹幕出现错误", LogType.Error, ex);
                NotificationShowExtensions.ShowMessageToast("发送弹幕出现错误");
                return false;
            }
        }

        public async Task SendDanmu(LiveRoomEmoticon emoji)
        {
            if (!Logined && !await NotificationShowExtensions.ShowLoginDialog())
            {
                NotificationShowExtensions.ShowMessageToast("请先登录");
                return;
            }
            if (emoji.Perm != 1)
            {
                NotificationShowExtensions.ShowMessageToast("权限不足哦~\n" + $"需要: {emoji.UnlockShowText}");
                return;
            }
            try
            {
                var result = await m_liveRoomApi.SendDanmu(RoomID, emoji).Request();
                if (!result.status)
                {
                    throw new CustomizedErrorException(result.message);
                }

                var data = await result.GetData<object>();
                if (!data.success)
                {
                    throw new CustomizedErrorException(data.message);
                }
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.Log("发送表情弹幕出现错误", LogType.Error, ex);
                NotificationShowExtensions.ShowMessageToast("发送表情弹幕出现错误");
            }
        }

        public async Task<bool> JoinAnchorLottery()
        {
            try
            {
                if (!Logined)
                {
                    throw new CustomizedErrorException("未登录");
                }

                if (LotteryViewModel?.AnchorLotteryInfo == null)
                {
                    throw new CustomizedErrorException("未获取到天选抽奖信息");
                }

                return await JoinAnchorLotteryRequest(LotteryViewModel.AnchorLotteryInfo.Id, LotteryViewModel.AnchorLotteryInfo.GiftId, LotteryViewModel.AnchorLotteryInfo.GiftNum);
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
                return false;
            }
            catch (Exception ex)
            {
                _logger.Log("参与天选抽奖出现错误", LogType.Error, ex);
                NotificationShowExtensions.ShowMessageToast("参与天选抽奖出现错误");
                return false;
            }

        }

        public async Task<bool> JoinRedPocketLottery()
        {
            try
            {
                if (!Logined)
                {
                    throw new CustomizedErrorException("未登录");
                }

                if (LotteryViewModel == null ||
                    LotteryViewModel.RedPocketLotteryInfo == null ||
                    string.IsNullOrEmpty(LotteryViewModel.RedPocketLotteryInfo.Danmu))
                {
                    return false;
                }
                // 参与红包抽奖会自动发送弹幕, 不用自己发
                return await JoinRedPocketLotteryRequest(SettingService.Account.UserID,
                                                                   RoomID,
                                                                   AnchorUid,
                                                                   LotteryViewModel.RedPocketLotteryInfo.LotteryId.ToInt32());
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
                return false;
            }
            catch (Exception ex)
            {
                _logger.Log("参与红包抽奖出现错误", LogType.Error, ex);
                NotificationShowExtensions.ShowMessageToast("参与红包抽奖出现错误");
                return false;
            }

        }

        public async Task GetEmoticons()
        {
            if (!Logined && !await NotificationShowExtensions.ShowLoginDialog())
            {
                NotificationShowExtensions.ShowMessageToast("请先登录");
                return;
            }
            try
            {
                var result = await m_liveRoomApi.GetLiveRoomEmoticon(RoomID).Request();
                if (!result.status) throw new CustomizedErrorException(result.message);
                var data = await result.GetData<JObject>();
                if (!data.success) throw new CustomizedErrorException(data.message);

                if (EmoticonsPackages?.Count > 0) EmoticonsPackages?.Clear();
                EmoticonsPackages = JsonConvert.DeserializeObject<ObservableCollection<LiveRoomEmoticonPackage>>(data.data["data"].ToString());
            }
            catch (CustomizedErrorException ex)
            {
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                _logger.Error(ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.Log("获取表情包出现错误", LogType.Error, ex);
                NotificationShowExtensions.ShowMessageToast("获取表情包出现错误");
            }
        }

        public void CheckClearMessages()
        {
            if (Messages.Count >= CleanCount) Messages.RemoveAt(0);
        }

        public async Task LiveStart()
        {
            Live = true;
            m_timerLiveTime.Start();
            await GetPlayUrls(RoomID, SettingService.GetValue(SettingConstants.Live.DEFAULT_QUALITY, 10000));
            StartLive?.Invoke(this, null);
        }

        public async Task LiveStop()
        {
            Live = false;
            m_timerLiveTime.Stop();
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                LiveTime = "未开播";
            });
            StopLive?.Invoke(this, null);
        }

        public void Dispose()
        {
            foreach (var item in LotteryDanmu)
            {
                DelShieldWord?.Invoke(this, item.Value);
            }

            m_cancelSource?.Cancel();
            m_liveMessage?.Dispose();

            m_timerSuperChat?.Stop();
            m_timerLiveTime?.Stop();
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

        #endregion
    }
}
