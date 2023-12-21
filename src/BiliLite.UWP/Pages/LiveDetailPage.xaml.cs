using BiliLite.Controls;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Modules;
using BiliLite.Services;
using Microsoft.UI.Xaml.Controls;
using NSDanmaku.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.System.Display;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using BiliLite.Models.Common.Live;
using BiliLite.Models.Common.Player;
using BiliLite.Models.Exceptions;
using BiliLite.Player;
using BiliLite.Player.Controllers;
using BiliLite.Player.States.ContentStates;
using BiliLite.Player.States.PauseStates;
using BiliLite.Player.States.PlayStates;
using BiliLite.Player.States.ScreenStates;
using BiliLite.ViewModels.Live;
using Windows.UI.Xaml.Documents;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace BiliLite.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class LiveDetailPage : BasePage
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();

        private readonly BasePlayerController m_playerController;
        private readonly LivePlayer m_player;
        private readonly RealPlayInfo m_realPlayInfo;
        private readonly PlayerConfig m_playerConfig;
        private readonly LiveDetailPageViewModel m_viewModel;

        DisplayRequest dispRequest;
        LiveRoomViewModel m_liveRoomViewModel;
        SettingVM settingVM;
        DispatcherTimer timer_focus;
        DispatcherTimer controlTimer;

        private string url = "";
        private bool changePlayUrlFlag = false;

        public LiveDetailPage()
        {
            m_viewModel = new LiveDetailPageViewModel();
            DataContext = m_viewModel;
            this.InitializeComponent();

            m_playerConfig = new PlayerConfig();
            PreLoadSetting();
            m_playerController = PlayerControllerFactory.Create(PlayerType.Live);
            m_player = new LivePlayer(m_playerConfig, playerElement, m_playerController);
            m_realPlayInfo = new RealPlayInfo();
            m_realPlayInfo.IsAutoPlay = true;
            m_playerController.SetPlayer(m_player);
            m_player.SetRealPlayInfo(m_realPlayInfo);
            InitPlayerEvent();

            Title = "直播间";
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
            dispRequest = new DisplayRequest();
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += DataTransferManager_DataRequested;
            //每过2秒就设置焦点
            timer_focus = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(2) };

            timer_focus.Tick += Timer_focus_Tick;
            controlTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
            controlTimer.Tick += ControlTimer_Tick;
            settingVM = new SettingVM();

            m_liveRoomViewModel = new LiveRoomViewModel();
            m_liveRoomViewModel.ChangedPlayUrl += LiveRoomViewModelChangedPlayUrl;
            m_liveRoomViewModel.AddNewDanmu += LiveRoomViewModelAddNewDanmu;
            m_liveRoomViewModel.AnchorLotteryEnd += LiveRoomViewModelAnchorLotteryEnd;
            m_liveRoomViewModel.RedPocketLotteryEnd += LiveRoomViewModelRedPocketLotteryEnd;
            m_liveRoomViewModel.ChatScrollToEnd += LiveRoomViewModelChatScrollToEnd;
            m_liveRoomViewModel.LotteryViewModel.AnchorLotteryStart += LiveRoomViewModelAnchorLotteryStart;
            this.Loaded += LiveDetailPage_Loaded;
            this.Unloaded += LiveDetailPage_Unloaded;
        }

        private void ControlTimer_Tick(object sender, object e)
        {
            // 显示播放器控件，5秒后隐藏，如果正在输入则不隐藏
            if (showControlsFlag == -1) return;
            if (showControlsFlag >= 5)
            {
                var element = FocusManager.GetFocusedElement();
                if (element is TextBox || element is AutoSuggestBox) return;
                ShowControl(false);
                showControlsFlag = -1;
            }
            else
            {
                showControlsFlag++;
            }
        }

        private void Timer_focus_Tick(object sender, object e)
        {
            var element = FocusManager.GetFocusedElement();
            if (element is Button || element is AppBarButton || element is HyperlinkButton || element is MenuFlyoutItem)
            {
                BtnFoucs.Focus(FocusState.Programmatic);
            }
        }

        private void LiveRoomViewModelAnchorLotteryStart(object sender, LiveRoomAnchorLotteryInfoModel e)
        {
            AnchorLotteryWinnerList.Content = e.WinnerList;
            m_liveRoomViewModel.ShowAnchorLotteryWinnerList = e.AwardUsers != null && e.AwardUsers.Count > 0;
        }

        private void LiveRoomViewModelAnchorLotteryEnd(object sender, LiveRoomEndAnchorLotteryInfoModel e)
        {
            var str = e.AwardUsers.Aggregate("", (current, item) => current + (item.Uname + "、"));
            str = str.TrimEnd('、');
            var msg = $"天选时刻 开奖信息:\r\n奖品: {e.AwardName} \r\n中奖用户:{str}";
            foreach(var awardUser in e.AwardUsers)
            {
                if (awardUser.Uid == SettingService.Account.UserID)
                {
                    msg += $"\r\n你已抽中奖品: {e.AwardName}, 恭喜欧皇~";
                }
            }
            Notify.ShowMessageToast(msg, new List<MyUICommand>(), 10);
            AnchorLotteryWinnerList.Content = e.WinnerList;
            m_liveRoomViewModel.ShowAnchorLotteryWinnerList = true;
            m_liveRoomViewModel.LoadBag().RunWithoutAwait();
        }

        private void LiveRoomViewModelRedPocketLotteryEnd(object sender, LiveRoomEndRedPocketLotteryInfoModel e)
        {
            var winners = e.Winners;
            var awards = e.Awards;
            redPocketWinnerList.Content = e.WinnersList;
            m_liveRoomViewModel.ShowRedPocketLotteryWinnerList = true;
            foreach (var winner in winners)
            {
                if (winner[0] == (SettingService.Account.UserID).ToString()) {
                    Notify.ShowMessageToast($"你已在人气红包抽中 {awards[winner[3]].AwardName} , 赶快查看吧~");
                }
            }
            m_liveRoomViewModel.LoadBag().RunWithoutAwait();
        }

        private void LiveRoomViewModelAddNewDanmu(object sender, DanmuMsgModel e)
        {
            if (DanmuControl.Visibility != Visibility.Visible) return;
            if (settingVM.LiveWords != null && settingVM.LiveWords.Count > 0)
            {
                if (settingVM.LiveWords.FirstOrDefault(x => e.Text.Contains(x)) != null) return;
            }
            try
            {
                DanmuControl.AddLiveDanmu(e.Text, false, e.DanmuColor.StrToColor());
            }
            catch (Exception ex)
            {
                //记录错误，不弹出通知
                logger.Log(ex.Message, LogType.Error, ex);
            }
        }

        private void LiveRoomViewModelChatScrollToEnd(object sender, EventArgs e)
        {
            list_chat.ScrollIntoView(list_chat.Items[list_chat.Items.Count - 1]);
        }

        #region 页面生命周期

        private void LiveDetailPage_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;
            timer_focus.Stop();
            controlTimer.Stop();
        }

        private void LiveDetailPage_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            BtnFoucs.Focus(FocusState.Programmatic);
            DanmuControl.ClearAll();
            if (this.Parent is MyFrame frame)
            {
                frame.ClosedPage -= LiveDetailPage_ClosedPage;
                frame.ClosedPage += LiveDetailPage_ClosedPage;
            }
            timer_focus.Start();
            controlTimer.Start();
        }

        private async void LiveDetailPage_ClosedPage(object sender, EventArgs e)
        {
            await StopPlay();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.NavigationMode == NavigationMode.New)
            {
                LoadSetting();
                roomid = e.Parameter.ToString();
                await m_liveRoomViewModel.LoadLiveRoomDetail(roomid);
                Title = m_liveRoomViewModel.LiveInfo.AnchorInfo.BaseInfo.Uname + "的直播间";
                ChangeTitle(m_liveRoomViewModel.LiveInfo.AnchorInfo.BaseInfo.Uname + "的直播间");
            }
            else
            {
                Title = (m_liveRoomViewModel.LiveInfo?.AnchorInfo?.BaseInfo?.Uname ?? "") + "直播间";
                MessageCenter.ChangeTitle(this, Title);
            }
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Back)
                await StopPlay();
            base.OnNavigatingFrom(e);
        }

        #endregion

        #region 播放器事件

        private void InitPlayerEvent()
        {
            m_playerController.PlayStateChanged += PlayerController_PlayStateChanged;
            m_playerController.PauseStateChanged += PlayerController_PauseStateChanged;
            m_playerController.ContentStateChanged += PlayerController_ContentStateChanged;
            m_playerController.ScreenStateChanged += PlayerController_ScreenStateChanged;
            m_player.ErrorOccurred += Player_ErrorOccurred;
            m_playerController.MediaInfosUpdated += PlayerController_MediaInfosUpdated; ;
        }

        private async void PlayerController_MediaInfosUpdated(object sender, Player.MediaInfos.MediaInfo e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () => { txtInfo.Text = e.ToString(); });
        }

        private async void PlayerController_ScreenStateChanged(object sender, ScreenStateChangedEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                m_viewModel.ScreenState = e.NewState;
                var view = ApplicationView.GetForCurrentView();
                if (e.NewState.IsFullscreen && !view.IsFullScreenMode)
                {
                    view.TryEnterFullScreenMode();
                }
                else if (view.IsFullScreenMode)
                {
                    view.ExitFullScreenMode();
                }
            });
        }

        private async void PlayerController_ContentStateChanged(object sender, ContentStateChangedEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                m_viewModel.ContentState = e.NewState;
            });
        }

        private async void PlayerController_PauseStateChanged(object sender, PauseStateChangedEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                m_viewModel.IsPaused = e.NewState.IsPaused;
            });
        }

        private async void Player_ErrorOccurred(object sender, PlayerException e)
        {
            await MediaFailed(e);
        }

        private async void PlayerController_PlayStateChanged(object sender, PlayStateChangedEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                m_viewModel.PlayState = e.NewState;
            });
            if (e.NewState.IsPlaying)
            {
                await MediaOpened();
            }
            if (e.NewState.IsStopped)
            {
                MediaStopped();
            }
        }

        private void MediaStopped()
        {
            url = "";
        }

        private async Task MediaFailed(PlayerException exception)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                logger.Log("直播加载失败", LogType.Error, new Exception(exception.Description));
                await new MessageDialog($"啊，直播加载失败了\r\n错误信息:{exception.Description}\r\n请尝试在直播设置中打开/关闭硬解试试", "播放失败")
                    .ShowAsync();
            });
        }

        private async Task MediaOpened()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                //保持屏幕常亮
                dispRequest.RequestActive();
                SetMediaInfo();
            });
        }

        private void SetMediaInfo()
        {
            try
            {
                //var str = $"Url: {url}\r\n";
                //str += $"Quality: {m_liveRoomViewModel.CurrentQn.Desc}({m_liveRoomViewModel.CurrentQn.Qn})\r\n";
                //str += $"Video Codec: {interopMSS.CurrentVideoStream.CodecName}\r\nAudio Codec:{interopMSS.AudioStreams[0].CodecName}\r\n";
                //str += $"Resolution: {interopMSS.CurrentVideoStream.PixelWidth} x {interopMSS.CurrentVideoStream.PixelHeight}\r\n";
                //str += $"Video Bitrate: {interopMSS.CurrentVideoStream.Bitrate / 1024} Kbps\r\n";
                //str += $"Audio Bitrate: {interopMSS.AudioStreams[0].Bitrate / 1024} Kbps\r\n";
                //str += $"Decoder Engine: {interopMSS.CurrentVideoStream.DecoderEngine.ToString()}";
                //txtInfo.Text = str;
            }
            catch (Exception)
            {
                txtInfo.Text = "Url";
            }
        }

        #endregion

        private void LiveRoomViewModelChangedPlayUrl(object sender, EventArgs e)
        {
            changePlayUrlFlag = true;

            m_realPlayInfo.PlayUrls.HlsUrls = m_liveRoomViewModel.HlsUrls;
            m_realPlayInfo.PlayUrls.FlvUrls = m_liveRoomViewModel.FlvUrls;
            var urls = m_liveRoomViewModel.HlsUrls ?? m_liveRoomViewModel.FlvUrls;
            BottomCBLine.ItemsSource = urls;

            var flag = false;
            for (var i = 0; i < urls.Count; i++)
            {
                var domain = new Uri(urls[i].Url).Host;

                if (domain.Contains(m_viewModel.LivePlayUrlSource) && !flag) {
                    BottomCBLine.SelectedIndex = i;
                    flag = true;
                }
            }
            if (!flag)
            {
                BottomCBLine.SelectedIndex = 0;
            }

            BottomCBQuality.SelectedItem = m_liveRoomViewModel.CurrentQn;
            changePlayUrlFlag = false;
        }

        private async void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            var elent = FocusManager.GetFocusedElement();
            if (elent is TextBox || elent is AutoSuggestBox)
            {
                args.Handled = false;
                return;
            }
            args.Handled = true;
            switch (args.VirtualKey)
            {
                case Windows.System.VirtualKey.Space:
                    if (m_playerController.PauseState.IsPaused)
                    {
                        await m_playerController.PauseState.Resume();
                    }
                    else
                    {
                        await m_playerController.PauseState.Pause();
                    }
                    break;

                case Windows.System.VirtualKey.Up:
                    if (SliderVolume.Value + 0.1 > 1)
                    {
                        SliderVolume.Value = 1;
                    }
                    else
                    {
                        SliderVolume.Value += 0.1;
                    }

                    TxtToolTip.Text = "音量:" + SliderVolume.Value.ToString("P");
                    ToolTip.Visibility = Visibility.Visible;
                    await Task.Delay(2000);
                    ToolTip.Visibility = Visibility.Collapsed;
                    break;

                case Windows.System.VirtualKey.Down:

                    if (SliderVolume.Value - 0.1 < 0)
                    {
                        SliderVolume.Value = 0;
                    }
                    else
                    {
                        SliderVolume.Value -= 0.1;
                    }
                    if (SliderVolume.Value == 0)
                    {
                        TxtToolTip.Text = "静音";
                    }
                    else
                    {
                        TxtToolTip.Text = "音量:" + SliderVolume.Value.ToString("P");
                    }
                    ToolTip.Visibility = Visibility.Visible;
                    await Task.Delay(2000);
                    ToolTip.Visibility = Visibility.Collapsed;
                    break;
                case Windows.System.VirtualKey.Escape:
                    SetFullScreen(false);

                    break;
                case Windows.System.VirtualKey.F8:
                case Windows.System.VirtualKey.T:
                    //小窗播放
                    MiniWidnows(BottomBtnMiniWindows.Visibility == Visibility.Visible);

                    break;
                case Windows.System.VirtualKey.F12:
                case Windows.System.VirtualKey.W:
                    SetFullWindow(!m_playerController.ContentState.IsFullWindow);
                    break;
                case Windows.System.VirtualKey.F11:
                case Windows.System.VirtualKey.F:
                case Windows.System.VirtualKey.Enter:
                    SetFullScreen(!m_playerController.ScreenState.IsFullscreen);
                    break;
                case Windows.System.VirtualKey.F10:
                    await CaptureVideo();
                    break;
                case Windows.System.VirtualKey.F9:
                case Windows.System.VirtualKey.D:
                    if (DanmuControl.Visibility == Visibility.Visible)
                    {
                        DanmuControl.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        DanmuControl.Visibility = Visibility.Visible;
                    }
                    break;
                default:
                    break;
            }
        }

        private async Task StopPlay()
        {
            await m_playerController.PlayState.Stop();
            m_liveRoomViewModel?.Dispose();
            //取消屏幕常亮
            if (dispRequest != null)
            {
                dispRequest = null;
            }
            m_liveRoomViewModel = null;
            SetFullScreen(false);
            MiniWidnows(false);
        }

        string roomid;

        private void PreLoadSetting()
        {
            //硬解视频
            LiveSettingHardwareDecode.IsOn =
                SettingService.GetValue<bool>(SettingConstants.Live.HARDWARE_DECODING, true);
            m_playerConfig.EnableHw = LiveSettingHardwareDecode.IsOn;
            LiveSettingHardwareDecode.Toggled += async (e, args) =>
            {
                SettingService.SetValue<bool>(SettingConstants.Live.HARDWARE_DECODING,
                    LiveSettingHardwareDecode.IsOn);
                m_playerConfig.EnableHw = LiveSettingHardwareDecode.IsOn;
                await LoadPlayer();
            };
            // 播放器优先模式
            m_viewModel.LivePlayerMode = (LivePlayerMode)SettingService.GetValue(
                        SettingConstants.Player.DEFAULT_LIVE_PLAYER_MODE,
                        (int)DefaultPlayerModeOptions.DEFAULT_LIVE_PLAYER_MODE);
            m_playerConfig.PlayMode = m_viewModel.LivePlayerMode;

            // 直播流默认源
            m_viewModel.LivePlayUrlSource = SettingService.GetValue(
                SettingConstants.Live.DEFAULT_LIVE_PLAY_URL_SOURCE,
                DefaultPlayUrlSourceOptions.DEFAULT_PLAY_URL_SOURCE);
        }

        private void LoadSetting()
        {
            //音量
            m_player.Volume = SettingService.GetValue(SettingConstants.Player.PLAYER_VOLUME, 1.0);
            SliderVolume.Value = m_player.Volume;
            SliderVolume.ValueChanged += (e, args) =>
            {
                m_player.Volume = SliderVolume.Value;
                SettingService.SetValue(SettingConstants.Player.PLAYER_VOLUME, SliderVolume.Value);
            };
            //亮度
            _brightness = SettingService.GetValue<double>(SettingConstants.Player.PLAYER_BRIGHTNESS, 0);
            BrightnessShield.Opacity = _brightness;

            //弹幕顶部距离
            DanmuControl.Margin = new Thickness(0, SettingService.GetValue<int>(SettingConstants.VideoDanmaku.TOP_MARGIN, 0), 0, 0);
            DanmuTopMargin.Value = DanmuControl.Margin.Top;
            DanmuTopMargin.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                SettingService.SetValue<double>(SettingConstants.VideoDanmaku.TOP_MARGIN, DanmuTopMargin.Value);
                DanmuControl.Margin = new Thickness(0, DanmuTopMargin.Value, 0, 0);
            });
            //弹幕大小
            DanmuControl.DanmakuSizeZoom = SettingService.GetValue<double>(SettingConstants.Live.FONT_ZOOM, 1);
            DanmuSettingFontZoom.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                if (isMini) return;
                SettingService.SetValue<double>(SettingConstants.Live.FONT_ZOOM, DanmuSettingFontZoom.Value);
            });
            //弹幕速度
            DanmuControl.DanmakuDuration = SettingService.GetValue<int>(SettingConstants.Live.SPEED, 10);
            DanmuSettingSpeed.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                if (isMini) return;
                SettingService.SetValue<double>(SettingConstants.Live.SPEED, DanmuSettingSpeed.Value);
            });
            //弹幕透明度
            DanmuControl.Opacity = SettingService.GetValue<double>(SettingConstants.Live.OPACITY, 1.0);
            DanmuSettingOpacity.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                SettingService.SetValue<double>(SettingConstants.Live.OPACITY, DanmuSettingOpacity.Value);
            });
            //弹幕加粗
            DanmuControl.DanmakuBold = SettingService.GetValue<bool>(SettingConstants.Live.BOLD, false);
            DanmuSettingBold.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingService.SetValue<bool>(SettingConstants.Live.BOLD, DanmuSettingBold.IsOn);
            });
            //弹幕样式
            DanmuControl.DanmakuStyle = (DanmakuBorderStyle)SettingService.GetValue<int>(SettingConstants.Live.BORDER_STYLE, 2);
            DanmuSettingStyle.SelectionChanged += new SelectionChangedEventHandler((e, args) =>
            {
                if (DanmuSettingStyle.SelectedIndex != -1)
                {
                    SettingService.SetValue<int>(SettingConstants.Live.BORDER_STYLE, DanmuSettingStyle.SelectedIndex);
                }
            });


            //弹幕显示区域
            DanmuControl.DanmakuArea = SettingService.GetValue<double>(SettingConstants.Live.AREA, 1);
            DanmuSettingArea.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                SettingService.SetValue<double>(SettingConstants.Live.AREA, DanmuSettingArea.Value);
            });

            //弹幕开关
            DanmuControl.Visibility = SettingService.GetValue<Visibility>(SettingConstants.Live.SHOW, Visibility.Visible);
            //弹幕延迟
            //LiveSettingDelay.Value = SettingService.GetValue<int>(SettingConstants.Live.DELAY, 20);
            //liveRoomVM.SetDelay(LiveSettingDelay.Value.ToInt32());
            //LiveSettingDelay.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            //{
            //    SettingService.SetValue(SettingConstants.Live.DELAY, LiveSettingDelay.Value);
            //    liveRoomVM.SetDelay(LiveSettingDelay.Value.ToInt32());
            //});

            //互动清理数量
            LiveSettingCount.Value = SettingService.GetValue<int>(SettingConstants.Live.DANMU_CLEAN_COUNT, 200);
            m_liveRoomViewModel.CleanCount = LiveSettingCount.Value.ToInt32();
            LiveSettingCount.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                SettingService.SetValue(SettingConstants.Live.DANMU_CLEAN_COUNT, LiveSettingCount.Value);
                m_liveRoomViewModel.CleanCount = LiveSettingCount.Value.ToInt32();
            });

            //自动打开宝箱
            LiveSettingAutoOpenBox.IsOn = SettingService.GetValue<bool>(SettingConstants.Live.AUTO_OPEN_BOX, true);
            m_liveRoomViewModel.AutoReceiveFreeSilver = LiveSettingAutoOpenBox.IsOn;
            LiveSettingAutoOpenBox.Toggled += new RoutedEventHandler((e, args) =>
            {
                m_liveRoomViewModel.AutoReceiveFreeSilver = LiveSettingAutoOpenBox.IsOn;
                SettingService.SetValue<bool>(SettingConstants.Live.AUTO_OPEN_BOX, LiveSettingAutoOpenBox.IsOn);
            });

            //屏蔽礼物信息
            LiveSettingDotReceiveGiftMsg.IsOn = SettingService.GetValue<bool>(SettingConstants.Live.HIDE_GIFT, false);
            m_liveRoomViewModel.ReceiveGiftMsg = !LiveSettingDotReceiveGiftMsg.IsOn;
            LiveSettingDotReceiveGiftMsg.Toggled += new RoutedEventHandler((e, args) =>
            {
                m_liveRoomViewModel.ReceiveGiftMsg = !LiveSettingDotReceiveGiftMsg.IsOn;
                if (LiveSettingAutoOpenBox.IsOn)
                {
                    m_liveRoomViewModel.ShowGiftMessage = false;
                }
                SettingService.SetValue<bool>(SettingConstants.Live.HIDE_GIFT, LiveSettingDotReceiveGiftMsg.IsOn);
            });

            //屏蔽进场信息
            LiveSettingDotReceiveWelcomeMsg.IsOn = SettingService.GetValue<bool>(SettingConstants.Live.HIDE_WELCOME, false);
            m_liveRoomViewModel.ReceiveWelcomeMsg = !LiveSettingDotReceiveWelcomeMsg.IsOn;
            LiveSettingDotReceiveWelcomeMsg.Toggled += new RoutedEventHandler((e, args) =>
            {
                m_liveRoomViewModel.ReceiveWelcomeMsg = !LiveSettingDotReceiveWelcomeMsg.IsOn;
                SettingService.SetValue<bool>(SettingConstants.Live.HIDE_WELCOME, LiveSettingDotReceiveWelcomeMsg.IsOn);
            });

            //屏蔽抽奖信息
            LiveSettingDotReceiveLotteryMsg.IsOn = SettingService.GetValue<bool>(SettingConstants.Live.HIDE_LOTTERY, false);
            m_liveRoomViewModel.ReceiveLotteryMsg = !LiveSettingDotReceiveLotteryMsg.IsOn;
            LiveSettingDotReceiveWelcomeMsg.Toggled += new RoutedEventHandler((e, args) =>
            {
                m_liveRoomViewModel.ReceiveLotteryMsg = !LiveSettingDotReceiveLotteryMsg.IsOn;
                SettingService.SetValue<bool>(SettingConstants.Live.HIDE_LOTTERY, LiveSettingDotReceiveLotteryMsg.IsOn);
            });
        }

        public void ChangeTitle(string title)
        {
            if (this.Parent is Frame frame)
            {
                if (frame.Parent is TabViewItem tabViewItem)
                {
                    tabViewItem.Header = title;
                }
            }
            else
            {
                MessageCenter.ChangeTitle(this, title);
            }
        }

        private async Task LoadPlayer()
        {
            try
            {
                await m_playerController.PlayState.Stop();
                await m_playerController.PlayState.Load();
            }
            catch (Exception ex)
            {
                Notify.ShowMessageToast("播放失败" + ex.Message);
            }
        }

        private async void BottomCBQuality_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BottomCBQuality.SelectedItem == null || changePlayUrlFlag)
            {
                return;
            }
            var item = BottomCBQuality.SelectedItem as LiveRoomWebUrlQualityDescriptionItemModel;
            SettingService.SetValue(SettingConstants.Live.DEFAULT_QUALITY, item.Qn);
            await m_liveRoomViewModel.GetPlayUrls(m_liveRoomViewModel.RoomID, item.Qn);
        }

        private async void BottomCBLine_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BottomCBLine.SelectedIndex == -1)
            {
                return;
            }

            m_playerConfig.SelectedRouteLine = BottomCBLine.SelectedIndex;

            await LoadPlayer();
        }

        private async void BottomBtnPause_Click(object sender, RoutedEventArgs e)
        {
            await m_playerController.PauseState.Pause();
        }

        private async void BottomBtnPlay_Click(object sender, RoutedEventArgs e)
        {
            await m_playerController.PauseState.Resume();
        }

        private void BottomBtnFullWindows_Click(object sender, RoutedEventArgs e)
        {
            SetFullWindow(true);
        }

        private void BottomBtnExitFullWindows_Click(object sender, RoutedEventArgs e)
        {
            SetFullWindow(false);
        }

        private void BottomBtnFull_Click(object sender, RoutedEventArgs e)
        {
            SetFullScreen(true);
        }

        private void BottomBtnExitFull_Click(object sender, RoutedEventArgs e)
        {
            SetFullScreen(false);
        }

        private async void SetFullWindow(bool e)
        {
            if (e)
            {
                await m_playerController.ContentState.FullWindow();
            }
            else
            {
                await m_playerController.ContentState.CancelFullWindow();
            }
        }

        private async void SetFullScreen(bool e)
        {
            if (e)
            {
                await m_playerController.ScreenState.Fullscreen();
            }
            else
            {
                await m_playerController.ScreenState.CancelFullscreen();
            }
        }

        private async void BottomBtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await m_liveRoomViewModel.LoadLiveRoomDetail(roomid);
        }

        private async void btnSendGift_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { DataContext: LiveGiftItem giftInfo })
            {
                await m_liveRoomViewModel.SendGift(giftInfo);
            }
        }

        private async void btnSendBagGift_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { DataContext: LiveGiftItem giftInfo })
            {
                await m_liveRoomViewModel.SendBagGift(giftInfo);
            }
        }

        private async void TopBtnScreenshot_Click(object sender, RoutedEventArgs e)
        {
            await CaptureVideo();
        }

        private async Task CaptureVideo()
        {
            try
            {
                string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg";
                StorageFolder applicationFolder = KnownFolders.PicturesLibrary;
                StorageFolder folder = await applicationFolder.CreateFolderAsync("哔哩哔哩截图", CreationCollisionOption.OpenIfExists);
                StorageFile saveFile = await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
                RenderTargetBitmap bitmap = new RenderTargetBitmap();
                await bitmap.RenderAsync(playerElement);
                var pixelBuffer = await bitmap.GetPixelsAsync();
                using (var fileStream = await saveFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, fileStream);
                    encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Ignore,
                         (uint)bitmap.PixelWidth,
                         (uint)bitmap.PixelHeight,
                         DisplayInformation.GetForCurrentView().LogicalDpi,
                         DisplayInformation.GetForCurrentView().LogicalDpi,
                         pixelBuffer.ToArray());
                    await encoder.FlushAsync();
                }
                Notify.ShowMessageToast("截图已经保存至图片库");
            }
            catch (Exception)
            {
                Notify.ShowMessageToast("截图失败");
            }
        }

        private void TopBtnCloseDanmaku_Click(object sender, RoutedEventArgs e)
        {
            DanmuControl.Visibility = Visibility.Collapsed;
            SettingService.SetValue(SettingConstants.Live.SHOW, Visibility.Collapsed);
            DanmuControl.ClearAll();
        }

        private void TopBtnOpenDanmaku_Click(object sender, RoutedEventArgs e)
        {
            DanmuControl.Visibility = Visibility.Visible;
            SettingService.SetValue(SettingConstants.Live.SHOW, Visibility.Visible);
        }

        private async void BtnOpenBox_Click(object sender, RoutedEventArgs e)
        {
            await m_liveRoomViewModel.GetFreeSilver();
        }

        private void BtnOpenUser_Click(object sender, RoutedEventArgs e)
        {
            if (m_liveRoomViewModel.LiveInfo == null)
            {
                return;
            }
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Account,
                title = "用户信息",
                page = typeof(UserInfoPage),
                parameters = m_liveRoomViewModel.LiveInfo.RoomInfo.Uid
            });
        }

        private async void DanmuText_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (string.IsNullOrEmpty(DanmuText.Text))
            {
                Notify.ShowMessageToast("弹幕内容不能为空");
                return;
            }
            var result = await m_liveRoomViewModel.SendDanmu(DanmuText.Text);
            if (result)
            {
                DanmuText.Text = "";
            }

        }

        private async void BtnAttention_Click(object sender, RoutedEventArgs e)
        {
            if (m_liveRoomViewModel.LiveInfo != null)
            {
                var result = await new VideoDetailPageViewModel().AttentionUP(m_liveRoomViewModel.LiveInfo.RoomInfo.Uid.ToString(), 1);
                if (result)
                {
                    m_liveRoomViewModel.Attention = true;
                }
            }

        }

        private async void BtnCacnelAttention_Click(object sender, RoutedEventArgs e)
        {
            if (m_liveRoomViewModel.LiveInfo != null)
            {
                var result = await new VideoDetailPageViewModel().AttentionUP(m_liveRoomViewModel.LiveInfo.RoomInfo.Uid.ToString(), 2);
                if (result)
                {
                    m_liveRoomViewModel.Attention = false;
                }
            }
        }

        private async void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pivot.SelectedIndex == 3 && m_liveRoomViewModel.Guards.Count == 0)
            {
                await m_liveRoomViewModel.GetGuardList();
            }
        }

        private void list_Guard_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as LiveGuardRankItem;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Account,
                title = item.Username,
                page = typeof(UserInfoPage),
                parameters = item.Uid
            });
        }

        private async void cb_Rank_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cb_Rank.SelectedItem == null)
            {
                return;
            }
            m_liveRoomViewModel.EmitSelectRankUpdate();
            var data = cb_Rank.SelectedItem as LiveRoomRankViewModel;
            if (!data.Loading && data.Items.Count == 0)
            {

                await data.LoadData();
            }
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as LiveRoomRankItemModel;
            MessageCenter.NavigateToPage(this, new NavigationInfo()
            {
                icon = Symbol.Account,
                title = item.Uname,
                page = typeof(UserInfoPage),
                parameters = item.Uid
            });
        }

        private async void BtnSendLotteryDanmu_Click(object sender, RoutedEventArgs e)
        {
            if (m_liveRoomViewModel.LotteryViewModel != null &&
                m_liveRoomViewModel.LotteryViewModel.AnchorLotteryInfo != null &&
                !string.IsNullOrEmpty(m_liveRoomViewModel.LotteryViewModel.AnchorLotteryInfo.Danmu))
            {
                var result = await m_liveRoomViewModel.SendDanmu(m_liveRoomViewModel.LotteryViewModel.AnchorLotteryInfo.Danmu);
                if (result)
                {
                    Notify.ShowMessageToast("弹幕发送成功");
                    FlyoutLottery.Hide();
                }
            }
        }

        private async void BtnSendRedPocketLotteryDanmu_Click(object sender, RoutedEventArgs e)
        {
            if (m_liveRoomViewModel.LotteryViewModel == null ||
                m_liveRoomViewModel.LotteryViewModel.RedPocketLotteryInfo == null ||
                string.IsNullOrEmpty(m_liveRoomViewModel.LotteryViewModel.RedPocketLotteryInfo.Danmu)) return;
            var msg = "";
            var result = await m_liveRoomViewModel.SendDanmu(m_liveRoomViewModel.LotteryViewModel.RedPocketLotteryInfo.Danmu);
            if (result)
            {
                FlyoutRedPocketLottery.Hide();
                msg += "弹幕发送成功";
            }

            if (!m_liveRoomViewModel.Attention)
            {
                BtnAttention_Click(sender, e);
                msg += ", 关注主播成功";
            }

            Notify.ShowMessageToast(msg, 4);
        }

        private void BottomBtnMiniWindows_Click(object sender, RoutedEventArgs e)
        {
            MiniWidnows(true);

        }

        private void BottomBtnExitMiniWindows_Click(object sender, RoutedEventArgs e)
        {
            MiniWidnows(false);
        }
        bool isMini = false;
        private async void MiniWidnows(bool mini)
        {
            isMini = mini;
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (mini)
            {
                BottomBtnFullWindows_Click(this, null);
                StandardControl.Visibility = Visibility.Collapsed;
                MiniControl.Visibility = Visibility.Visible;

                if (ApplicationView.GetForCurrentView().IsViewModeSupported(ApplicationViewMode.CompactOverlay))
                {
                    //隐藏标题栏
                    this.Margin = new Thickness(0, -40, 0, 0);
                    await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay);
                    DanmuControl.DanmakuSizeZoom = 0.5;
                    DanmuControl.DanmakuDuration = 6;
                    DanmuControl.ClearAll();
                }
            }
            else
            {
                BottomBtnExitFullWindows_Click(this, null);
                this.Margin = new Thickness(0, 0, 0, 0);
                StandardControl.Visibility = Visibility.Visible;
                MiniControl.Visibility = Visibility.Collapsed;
                await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
                DanmuControl.DanmakuSizeZoom = SettingService.GetValue<double>(SettingConstants.Live.FONT_ZOOM, 1);
                DanmuControl.DanmakuDuration = SettingService.GetValue<int>(SettingConstants.Live.SPEED, 10);
                DanmuControl.ClearAll();
                DanmuControl.Visibility = SettingService.GetValue<Visibility>(SettingConstants.Live.SHOW, Visibility.Visible);
            }
            MessageCenter.SetMiniWindow(mini);
        }

        private void btnRemoveWords_Click(object sender, RoutedEventArgs e)
        {
            var word = (sender as HyperlinkButton).DataContext as string;
            settingVM.LiveWords.Remove(word);
            SettingService.SetValue(SettingConstants.Live.SHIELD_WORD, settingVM.LiveWords);

        }

        private void DanmuSettingAddWord_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(DanmuSettingTxtWord.Text))
            {
                Notify.ShowMessageToast("关键字不能为空");
                return;
            }
            if (!settingVM.LiveWords.Contains(DanmuSettingTxtWord.Text))
            {
                settingVM.LiveWords.Add(DanmuSettingTxtWord.Text);
                SettingService.SetValue(SettingConstants.Live.SHIELD_WORD, settingVM.LiveWords);
            }

            DanmuSettingTxtWord.Text = "";
        }

        #region 播放器手势
        int showControlsFlag = 0;
        bool pointer_in_player = false;

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowControl(control.Visibility == Visibility.Collapsed);

        }
        bool runing = false;
        private async void ShowControl(bool show)
        {
            if (runing) return;
            runing = true;
            if (show)
            {
                showControlsFlag = 0;
                control.Visibility = Visibility.Visible;
                await control.FadeInAsync(280);

            }
            else
            {
                if (pointer_in_player)
                {
                    Window.Current.CoreWindow.PointerCursor = null;
                }
                await control.FadeOutAsync(280);
                control.Visibility = Visibility.Collapsed;
            }
            runing = false;
        }
        private void Grid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (!m_playerController.ScreenState.IsFullscreen)
            {
                BottomBtnFull_Click(sender, null);
            }
            else
            {
                BottomBtnExitFull_Click(sender, null);
            }
        }
        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            pointer_in_player = true;
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            pointer_in_player = false;
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
        }

        private void Grid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (Window.Current.CoreWindow.PointerCursor == null)
            {
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            }

        }

        bool ManipulatingBrightness = false;
        private void Grid_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;
            //progress.Visibility = Visibility.Visible;
            if (ManipulatingBrightness)
                HandleSlideBrightnessDelta(e.Delta.Translation.Y);
            else
                HandleSlideVolumeDelta(e.Delta.Translation.Y);
        }

        private void HandleSlideVolumeDelta(double delta)
        {
            if (delta > 0)
            {
                var dd = delta / (this.ActualHeight * 0.8);
                var volume = m_player.Volume - dd;
                if (volume < 0) volume = 0;
                SliderVolume.Value = volume;
            }
            else
            {
                var dd = Math.Abs(delta) / (this.ActualHeight * 0.8);
                var volume = m_player.Volume + dd;
                if (volume > 1) volume = 1;
                SliderVolume.Value = volume;
            }
            TxtToolTip.Text = "音量:" + m_player.Volume.ToString("P");
        }

        private void HandleSlideBrightnessDelta(double delta)
        {
            double dd = Math.Abs(delta) / (this.ActualHeight * 0.8);
            if (delta > 0)
            {
                Brightness = Math.Min(Brightness + dd, 1);
            }
            else
            {
                Brightness = Math.Max(Brightness - dd, 0);
            }
            TxtToolTip.Text = "亮度:" + Math.Abs(Brightness - 1).ToString("P");
        }

        private void Grid_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            e.Handled = true;
            TxtToolTip.Text = "";
            ToolTip.Visibility = Visibility.Visible;

            if (e.Position.X < this.ActualWidth / 2)
                ManipulatingBrightness = true;
            else
                ManipulatingBrightness = false;

        }

        double _brightness;
        double Brightness
        {
            get => _brightness;
            set
            {
                _brightness = value;
                BrightnessShield.Opacity = value;
                SettingService.SetValue<double>(SettingConstants.Player.PLAYER_BRIGHTNESS, _brightness);
            }
        }

        private void Grid_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            e.Handled = true;
            ToolTip.Visibility = Visibility.Collapsed;
        }
        #endregion

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.Properties.Title = m_liveRoomViewModel.LiveInfo.RoomInfo.Title;
            request.Data.SetWebLink(new Uri("https://live.bilibili.com/" + m_liveRoomViewModel.RoomID));
        }

        private void btnShare_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

        private void btnShareCopy_Click(object sender, RoutedEventArgs e)
        {
            $"{m_liveRoomViewModel.LiveInfo.RoomInfo.Title} - {m_liveRoomViewModel.LiveInfo.AnchorInfo.BaseInfo.Uname}的直播间\r\nhttps://live.bilibili.com/{m_liveRoomViewModel.RoomID}".SetClipboard();
            Notify.ShowMessageToast("已复制内容到剪切板");
        }

        private void btnShareCopyUrl_Click(object sender, RoutedEventArgs e)
        {
            ("https://live.bilibili.com/" + m_liveRoomViewModel.RoomID).SetClipboard();
            Notify.ShowMessageToast("已复制链接到剪切板");
        }

        private void Player_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var rectangle = new RectangleGeometry();
            rectangle.Rect = new Rect(0, 0, PlayerView.ActualWidth, PlayerView.ActualHeight);
            DanmuControl.Clip = rectangle;
        }

        private async void PlayerModeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SettingService.SetValue(SettingConstants.Player.DEFAULT_LIVE_PLAYER_MODE, m_viewModel.LivePlayerMode);
            m_playerConfig.PlayMode = m_viewModel.LivePlayerMode;
            await LoadPlayer();
        }

        private async void PlayUrlSourceComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SettingService.SetValue(SettingConstants.Live.DEFAULT_LIVE_PLAY_URL_SOURCE, m_viewModel.LivePlayUrlSource);
            LiveRoomViewModelChangedPlayUrl(null, null);
            await LoadPlayer();
        }
    }
}
