﻿using Atelier39;
using BiliLite.Controls.Dialogs;
using BiliLite.Converters;
using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Danmaku;
using BiliLite.Models.Common.Player;
using BiliLite.Models.Common.Video;
using BiliLite.Models.Common.Video.PlayUrlInfos;
using BiliLite.Modules;
using BiliLite.Modules.Player;
using BiliLite.Services;
using BiliLite.Services.Interfaces;
using BiliLite.ViewModels;
using BiliLite.ViewModels.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using NSDanmaku.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Text;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using BiliLite.Modules.ExtraInterface;
using PlayInfo = BiliLite.Models.Common.Video.PlayInfo;
//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls
{
    public sealed partial class PlayerControl : UserControl, IDisposable
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly bool m_useNsDanmaku = true;
        private readonly IDanmakuController m_danmakuController;
        private readonly VideoDanmakuSettingsControlViewModel m_danmakuSettingsControlViewModel;
        private readonly PlayControlViewModel m_viewModel;
        private readonly PlayerToastService m_playerToastService;
        private readonly PlaySpeedMenuService m_playSpeedMenuService;
        private readonly SoundQualitySliderTooltipConverter m_soundQualitySliderTooltipConverter;
        private readonly QualitySliderTooltipConverter m_qualitySliderTooltipConverter;
        private readonly PlaySpeedSliderTooltipConverter m_playSpeedSliderTooltipConverter;
        private DateTime m_startTime = DateTime.Now;
        public event PropertyChangedEventHandler PropertyChanged;
        private GestureRecognizer gestureRecognizer;
        private bool m_firstMediaOpened;
        private ThemeService m_themeService;
        private bool m_isLocalFileMode;
        private readonly IPlayerSponsorBlockControl m_playerSponsorBlockControl;

        private void DoPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        InteractionVideoVM interactionVideoVM;

        public Canvas PlayerToastContainer => ToolTipContainer;

        /// <summary>
        /// 铺满窗口事件
        /// </summary>
        public event EventHandler<bool> FullWindowEvent;
        /// <summary>
        /// 全屏事件
        /// </summary>
        public event EventHandler<bool> FullScreenEvent;
        /// <summary>
        /// 全部播放完毕
        /// </summary>
        public event EventHandler AllMediaEndEvent;
        /// <summary>
        /// 切换剧集事件
        /// </summary>

        public event EventHandler<int> ChangeEpisodeEvent;
        /// <summary>
        /// 播放列表
        /// </summary>
        public List<PlayInfo> PlayInfos { get; set; }
        /// <summary>
        /// 当前播放
        /// </summary>
        public int CurrentPlayIndex { get; set; }

        public bool IsPlaying => Player.PlayState == PlayState.Playing || Player.PlayState == PlayState.End;

        /// <summary>
        /// 当前播放
        /// </summary>
        public PlayInfo CurrentPlayItem { get; set; }
        readonly PlayerVM playerHelper;
        readonly NSDanmaku.Helper.DanmakuParse danmakuParse;
        private BiliPlayUrlQualitesInfo _playUrlInfo;
        private PlayerKeyRightAction m_playerKeyRightAction;
        /// <summary>
        /// 播放地址信息
        /// </summary>
        public BiliPlayUrlQualitesInfo playUrlInfo
        {
            get { return _playUrlInfo; }
            set { _playUrlInfo = value; DoPropertyChanged("playUrlInfo"); }
        }

        private readonly bool m_autoSkipOpEdFlag = false;
        private DispatcherTimer m_autoRefreshTimer;
        private DispatcherTimer m_positionTimer;
        DispatcherTimer danmuTimer;
        /// <summary>
        /// 弹幕信息
        /// </summary>
        IDictionary<int, List<NSDanmaku.Model.DanmakuModel>> danmakuPool = new Dictionary<int, List<NSDanmaku.Model.DanmakuModel>>();
        List<int> danmakuLoadedSegment;
        DisplayRequest dispRequest;
        SystemMediaTransportControls _systemMediaTransportControls;
        DispatcherTimer timer_focus;
        public Player PlayerInstance { get { return Player; } }
        /// <summary>
        /// 当前选中的字幕名称
        /// </summary>
        private string CurrentSubtitleName { get; set; } = "无";

        DisplayInformation displayInformation = DisplayInformation.GetForCurrentView();

        public PlayerControl()
        {
            m_viewModel = new PlayControlViewModel();
            m_playSpeedMenuService = App.ServiceProvider.GetRequiredService<PlaySpeedMenuService>();
            m_playerToastService = App.ServiceProvider.GetRequiredService<PlayerToastService>();
            m_themeService = App.ServiceProvider.GetRequiredService<ThemeService>();
            m_playerToastService.Init(this);
            InitializeComponent();
            dispRequest = new DisplayRequest();
            playerHelper = new PlayerVM();
            m_danmakuSettingsControlViewModel = App.ServiceProvider.GetRequiredService<VideoDanmakuSettingsControlViewModel>();

            danmakuParse = new NSDanmaku.Helper.DanmakuParse();
            //每过2秒就设置焦点
            timer_focus = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(2) };
            timer_focus.Tick += Timer_focus_Tick;

            //自动刷新播放地址
            if (SettingService.GetValue(SettingConstants.Player.AUTO_REFRESH_PLAY_URL,
                    SettingConstants.Player.DEFAULT_AUTO_REFRESH_PLAY_URL))
            {
                var timeMin = SettingService.GetValue(SettingConstants.Player.AUTO_REFRESH_PLAY_URL_TIME,
                    SettingConstants.Player.DEFAULT_AUTO_REFRESH_PLAY_URL_TIME);
                m_autoRefreshTimer = new DispatcherTimer();
                m_autoRefreshTimer.Interval = TimeSpan.FromMinutes(timeMin);
                m_autoRefreshTimer.Tick += AutoRefreshTimer_Tick; ;
            }

            danmuTimer = new DispatcherTimer();
            danmuTimer.Interval = TimeSpan.FromSeconds(1);
            danmuTimer.Tick += DanmuTimer_Tick;

            m_positionTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(100) };
            m_positionTimer.Tick += PositionTimer_Tick;
            m_autoSkipOpEdFlag = SettingService.GetValue(SettingConstants.Player.AUTO_SKIP_OP_ED,
                SettingConstants.Player.DEFAULT_AUTO_SKIP_OP_ED);

            Loaded += PlayerControl_Loaded;
            Unloaded += PlayerControl_Unloaded;
            m_playerKeyRightAction = (PlayerKeyRightAction)SettingService.GetValue(SettingConstants.Player.PLAYER_KEY_RIGHT_ACTION, (int)PlayerKeyRightAction.ControlProgress);

            gestureRecognizer = new GestureRecognizer();
            InitializeGesture();

            m_soundQualitySliderTooltipConverter = new SoundQualitySliderTooltipConverter();
            m_qualitySliderTooltipConverter = new QualitySliderTooltipConverter();
            m_playSpeedSliderTooltipConverter = new PlaySpeedSliderTooltipConverter(m_playSpeedMenuService);

            m_useNsDanmaku = (DanmakuEngineType)SettingService.GetValue(SettingConstants.VideoDanmaku.DANMAKU_ENGINE,
                (int)SettingConstants.VideoDanmaku.DEFAULT_DANMAKU_ENGINE) == DanmakuEngineType.NSDanmaku;
            if (m_useNsDanmaku)
            {
                m_danmakuController = App.ServiceProvider.GetRequiredService<NsDanmakuController>();
                m_danmakuController.Init(DanmuControl);
            }
            else
            {
                m_danmakuController = App.ServiceProvider.GetRequiredService<FrostMasterDanmakuController>();
                m_danmakuController.Init(DanmakuCanvas);
            }

            // 加载 SponsorBlockControl 如果有的话
            if (SettingService.GetValue(SettingConstants.Player.SPONSOR_BLOCK, SettingConstants.Player.DEFAULT_SPONSOR_BLOCK))
            {
                m_playerSponsorBlockControl = App.ServiceProvider.GetService<IPlayerSponsorBlockControl>();
                if (m_playerSponsorBlockControl != null)
                {
                    ExtraToolsPanel.Children.Add(m_playerSponsorBlockControl as UIElement);
                    m_playerSponsorBlockControl.UpdatePosition += PlayerSponsorBlockControlOnUpdatePosition;

                    void PlayerSponsorBlockControlOnUpdatePosition(object sender, double e)
                    {
                        SetPosition(e);
                    }
                }
            }
        }

        private async void AutoRefreshTimer_Tick(object sender, object e)
        {
            m_autoRefreshTimer.Stop();
            _postion = Player.Position;
            var info = await GetPlayUrlQualitesInfo();
            if (!info.Success)
            {
                await NotificationShowExtensions.ShowMessageDialog($"请求信息:\r\n{info.Message}", "读取视频播放地址失败");
            }
            else
            {
                playUrlInfo = info;
                InitSoundQuality();
                InitQuality();
            }
            NotificationShowExtensions.ShowMessageToast("已根据设置自动刷新播放地址");
            m_startTime = DateTime.Now;
        }
        private void Timer_focus_Tick(object sender, object e)
        {
            var elent = FocusManager.GetFocusedElement();
            if (elent is Button || elent is AppBarButton || elent is HyperlinkButton || elent is MenuFlyoutItem)
            {
                BtnFoucs.Focus(FocusState.Programmatic);
            }

        }

        bool runing = false;
        bool pointer_in_player = false;
        private async void ShowControl(bool show)
        {
            if (runing) return;
            runing = true;
            if (show)
            {
                BottomImageBtnPlay.Margin = new Thickness(24, 24, 24, 100);
                showControlsFlag = 0;
                control.Visibility = Visibility.Visible;
                await control.FadeInAsync(400);
            }
            else
            {
                if (pointer_in_player)
                {
                    Window.Current.CoreWindow.PointerCursor = null;
                }
                BottomImageBtnPlay.Margin = new Thickness(24, 24, 24, 24);
                await control.FadeOutAsync(400);
                control.Visibility = Visibility.Collapsed;

            }
            runing = false;
        }
        private void PlayerControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            if (_systemMediaTransportControls != null)
            {
                _systemMediaTransportControls.DisplayUpdater.ClearAll();
                _systemMediaTransportControls.IsEnabled = false;
                _systemMediaTransportControls = null;
            }
            timer_focus.Stop();
        }

        private async void PlayerControl_Loaded(object sender, RoutedEventArgs e)
        {
            m_danmakuController.Clear();
            BtnFoucs.Focus(FocusState.Programmatic);
            _systemMediaTransportControls = SystemMediaTransportControls.GetForCurrentView();
            _systemMediaTransportControls.IsPlayEnabled = true;
            _systemMediaTransportControls.IsPauseEnabled = true;
            if (CurrentPlayItem != null)
            {
                SystemMediaTransportControlsDisplayUpdater updater = _systemMediaTransportControls.DisplayUpdater;
                updater.Type = MediaPlaybackType.Video;
                updater.VideoProperties.Title = CurrentPlayItem.title;
                updater.Update();
            }

            _systemMediaTransportControls.ButtonPressed += _systemMediaTransportControls_ButtonPressed;

            LoadPlayerSetting();
            LoadDanmuSetting();
            LoadSutitleSetting();

            danmuTimer.Start();
            timer_focus.Start();
            m_positionTimer.Start();
        }

        private async void _systemMediaTransportControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        await Play();
                    });
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        Pause();
                    });
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 检查音量和亮度是否偏低
        /// </summary>
        private void CheckVolumeAndBrightnessLower()
        {
            // 检查音量是否偏低
            if (Player.Volume > 0.95) return;
            var toolTipText = "";
            if (Player.Volume == 0)
            {
                toolTipText = "静音";
            }
            else
            {
                toolTipText = "音量:" + Player.Volume.ToString("P");
            }

            m_playerToastService.Show(PlayerToastService.VOLUME_KEY, toolTipText);

            // 检查亮度是否偏低
            if (Math.Abs(Brightness - 1) > 0.8) return;
            m_playerToastService.Show(PlayerToastService.BRIGHTNESS_KEY, "亮度:" + Math.Abs(Brightness - 1).ToString("P"));
        }

        private void LoadDanmuSetting()
        {
            try
            {
                //顶部
                DanmuSettingHideTop.IsOn = SettingService.GetValue<bool>(SettingConstants.VideoDanmaku.HIDE_TOP, false);
                if (DanmuSettingHideTop.IsOn)
                {
                    m_danmakuController.HideTop();
                }
                DanmuSettingHideTop.Toggled += new RoutedEventHandler((e, args) =>
                {
                    SettingService.SetValue<bool>(SettingConstants.VideoDanmaku.HIDE_TOP, DanmuSettingHideTop.IsOn);
                    if (DanmuSettingHideTop.IsOn)
                    {
                        m_danmakuController.HideTop();
                    }
                    else
                    {
                        m_danmakuController.ShowTop();
                    }
                });
                //底部
                DanmuSettingHideBottom.IsOn = SettingService.GetValue<bool>(SettingConstants.VideoDanmaku.HIDE_BOTTOM, false);
                if (DanmuSettingHideBottom.IsOn)
                {
                    m_danmakuController.HideBottom();
                }
                DanmuSettingHideBottom.Toggled += new RoutedEventHandler((e, args) =>
                {
                    SettingService.SetValue<bool>(SettingConstants.VideoDanmaku.HIDE_BOTTOM, DanmuSettingHideBottom.IsOn);
                    if (DanmuSettingHideBottom.IsOn)
                    {
                        m_danmakuController.HideBottom();
                    }
                    else
                    {
                        m_danmakuController.ShowBottom();
                    }
                });
                //滚动
                DanmuSettingHideRoll.IsOn = SettingService.GetValue<bool>(SettingConstants.VideoDanmaku.HIDE_ROLL, false);
                if (DanmuSettingHideRoll.IsOn)
                {
                    m_danmakuController.HideScroll();
                }
                DanmuSettingHideRoll.Toggled += new RoutedEventHandler((e, args) =>
                {
                    SettingService.SetValue<bool>(SettingConstants.VideoDanmaku.HIDE_ROLL, DanmuSettingHideRoll.IsOn);
                    if (DanmuSettingHideRoll.IsOn)
                    {
                        m_danmakuController.HideScroll();
                    }
                    else
                    {
                        m_danmakuController.ShowScroll();
                    }
                });
                //弹幕大小
                m_danmakuController.SetFontZoom(SettingService.GetValue<double>(SettingConstants.VideoDanmaku.FONT_ZOOM, 1));
                DanmuSettingFontZoom.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
                {
                    if (miniWin) return;
                    SettingService.SetValue<double>(SettingConstants.VideoDanmaku.FONT_ZOOM, DanmuSettingFontZoom.Value);
                    m_danmakuController.SetFontZoom(DanmuSettingFontZoom.Value);
                });
                //弹幕显示区域
                m_danmakuController.SetArea(SettingService.GetValue<double>(SettingConstants.VideoDanmaku.AREA, 1));
                DanmuSettingArea.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
                {
                    if (miniWin) return;
                    SettingService.SetValue<double>(SettingConstants.VideoDanmaku.AREA, DanmuSettingArea.Value);
                    m_danmakuController.SetArea(DanmuSettingArea.Value);
                });

                //弹幕速度
                m_danmakuController.SetSpeed(SettingService.GetValue<int>(SettingConstants.VideoDanmaku.SPEED, 10));
                DanmuSettingSpeed.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
                {
                    if (miniWin) return;
                    SettingService.SetValue<int>(SettingConstants.VideoDanmaku.SPEED, (int)DanmuSettingSpeed.Value);
                    m_danmakuController.SetSpeed((int)DanmuSettingSpeed.Value);
                });
                //弹幕顶部距离
                var marginTop = SettingService.GetValue<double>(SettingConstants.VideoDanmaku.TOP_MARGIN, 0);
                m_danmakuController.SetTopMargin(marginTop);
                DanmuTopMargin.Value = marginTop;
                DanmuTopMargin.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
                {
                    SettingService.SetValue<double>(SettingConstants.VideoDanmaku.TOP_MARGIN, DanmuTopMargin.Value);
                    m_danmakuController.SetTopMargin(DanmuTopMargin.Value);
                });
                //弹幕透明度
                m_danmakuController.SetOpacity(SettingService.GetValue<double>(SettingConstants.VideoDanmaku.OPACITY, 1.0));
                DanmuSettingOpacity.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
                {
                    SettingService.SetValue<double>(SettingConstants.VideoDanmaku.OPACITY, DanmuSettingOpacity.Value);
                    m_danmakuController.SetOpacity(DanmuSettingOpacity.Value);
                });
                //弹幕最大值
                DanmuSettingMaxNum.Value = SettingService.GetValue<double>(SettingConstants.VideoDanmaku.MAX_NUM, 0);
                m_danmakuController.SetDensity((int)DanmuSettingMaxNum.Value);
                DanmuSettingMaxNum.ValueChanged += async (e, args) =>
                {
                    SettingService.SetValue<double>(SettingConstants.VideoDanmaku.MAX_NUM, DanmuSettingMaxNum.Value);
                    m_danmakuController.SetDensity((int)DanmuSettingMaxNum.Value);
                    if (!m_useNsDanmaku)
                    {
                        var segIndex = System.Convert.ToInt32(Math.Ceiling(Player.Position / (60 * 6d)));
                        if (segIndex <= 0) segIndex = 1;
                        await LoadDanmaku(segIndex);
                    }
                };

                //弹幕云屏蔽等级
                DanmuSettingShieldLevel.Value = SettingService.GetValue<int>(SettingConstants.VideoDanmaku.SHIELD_LEVEL, 0);
                DanmuSettingShieldLevel.ValueChanged += async (e, args) =>
                {
                    SettingService.SetValue<int>(SettingConstants.VideoDanmaku.SHIELD_LEVEL, System.Convert.ToInt32(DanmuSettingShieldLevel.Value));
                    if (!m_useNsDanmaku)
                    {
                        var segIndex = System.Convert.ToInt32(Math.Ceiling(Player.Position / (60 * 6d)));
                        if (segIndex <= 0) segIndex = 1;
                        await LoadDanmaku(segIndex);
                    }
                };

                //弹幕加粗
                m_danmakuController.SetBold(SettingService.GetValue<bool>(SettingConstants.VideoDanmaku.BOLD, false));
                DanmuSettingBold.Toggled += new RoutedEventHandler((e, args) =>
                {
                    SettingService.SetValue<bool>(SettingConstants.VideoDanmaku.BOLD, DanmuSettingBold.IsOn);
                    m_danmakuController.SetBold(DanmuSettingBold.IsOn);
                });
                //弹幕样式
                m_danmakuController.SetBolderStyle(SettingService.GetValue<int>(SettingConstants.VideoDanmaku.BORDER_STYLE, 2));
                DanmuSettingStyle.SelectionChanged += new SelectionChangedEventHandler((e, args) =>
                {
                    if (DanmuSettingStyle.SelectedIndex != -1)
                    {
                        SettingService.SetValue<int>(SettingConstants.VideoDanmaku.BORDER_STYLE, DanmuSettingStyle.SelectedIndex);
                    }
                });

                //弹幕字体
                var fontFamily =
                    SettingService.GetValue<string>(SettingConstants.VideoDanmaku.DANMAKU_FONT_FAMILY, string.Empty);
                m_danmakuController.SetFont(fontFamily);
                DanmuSettingFont.Text = fontFamily;
                DanmuSettingFont.QuerySubmitted += (e, args) =>
                {
                    m_danmakuController.SetFont(DanmuSettingFont.Text);
                    SettingService.SetValue(SettingConstants.VideoDanmaku.DANMAKU_FONT_FAMILY, DanmuSettingFont.Text);
                };
                //合并弹幕
                DanmuSettingMerge.IsOn = SettingService.GetValue<bool>(SettingConstants.VideoDanmaku.MERGE, false);
                DanmuSettingMerge.Toggled += async (e, args) =>
                {
                    SettingService.SetValue<bool>(SettingConstants.VideoDanmaku.MERGE, DanmuSettingMerge.IsOn);
                    if (!m_useNsDanmaku)
                    {
                        var segIndex = System.Convert.ToInt32(Math.Ceiling(Player.Position / (60 * 6d)));
                        if (segIndex <= 0) segIndex = 1;
                        await LoadDanmaku(segIndex);
                    }
                };
                //屏蔽彩色弹幕
                DanmuSettingDisableColorful.IsOn = SettingService.GetValue<bool>(SettingConstants.VideoDanmaku.DISABLE_COLORFUL, false);
                DanmuSettingDisableColorful.Toggled += async (e, args) =>
                {
                    SettingService.SetValue<bool>(SettingConstants.VideoDanmaku.DISABLE_COLORFUL, DanmuSettingDisableColorful.IsOn);
                    if (!m_useNsDanmaku)
                    {
                        var segIndex = System.Convert.ToInt32(Math.Ceiling(Player.Position / (60 * 6d)));
                        if (segIndex <= 0) segIndex = 1;
                        await LoadDanmaku(segIndex);
                    }
                };
                //半屏显示
                //DanmuControl.DanmakuArea = SettingService.GetValue<bool>(SettingConstants.VideoDanmaku.DOTNET_HIDE_SUBTITLE, false)?1:.5;
                //DanmuSettingDotHideSubtitle.Toggled += new RoutedEventHandler((e, args) =>
                //{
                //    SettingService.SetValue<bool>(SettingConstants.VideoDanmaku.DOTNET_HIDE_SUBTITLE, DanmuSettingDotHideSubtitle.IsOn);
                //});

                //弹幕开关
                if (SettingService.GetValue<Visibility>(SettingConstants.VideoDanmaku.SHOW, Visibility.Visible) == Visibility.Visible)
                {
                    m_danmakuController.Show();
                }
                else
                {
                    m_danmakuController.Hide();
                }
                DanmuSettingWords.ItemsSource = m_danmakuSettingsControlViewModel.ShieldWords;
            }
            catch (Exception ex)
            {
                _logger.Warn("加载弹幕设置失败" + ex.Message, ex);
            }
        }
        private void LoadPlayerSetting()
        {
            //音量
            Player.Volume = SettingService.GetValue<double>(SettingConstants.Player.PLAYER_VOLUME, SettingConstants.Player.DEFAULT_PLAYER_VOLUME);

            var lockPlayerVolume = SettingService.GetValue(SettingConstants.Player.LOCK_PLAYER_VOLUME, SettingConstants.Player.DEFAULT_LOCK_PLAYER_VOLUME);
            if (!lockPlayerVolume)
            {
                SliderVolume.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
                {
                    SettingService.SetValue<double>(SettingConstants.Player.PLAYER_VOLUME, SliderVolume.Value);
                });
            }
            //亮度
            lockBrightness = SettingService.GetValue(SettingConstants.Player.LOCK_PLAYER_BRIGHTNESS, SettingConstants.Player.DEFAULT_LOCK_PLAYER_BRIGHTNESS);
            _brightness = SettingService.GetValue<double>(SettingConstants.Player.PLAYER_BRIGHTNESS, SettingConstants.Player.DEFAULT_PLAYER_BRIGHTNESS);
            BrightnessShield.Opacity = _brightness;

            //播放模式
            var selectedValue = (PlayUrlCodecMode)SettingService.GetValue(SettingConstants.Player.DEFAULT_VIDEO_TYPE, (int)DefaultVideoTypeOptions.DEFAULT_VIDEO_TYPE);
            PlayerSettingMode.SelectedItem = DefaultVideoTypeOptions.GetOption(selectedValue);
            PlayerSettingMode.SelectionChanged += (e, args) =>
            {
                SettingService.SetValue(SettingConstants.Player.DEFAULT_VIDEO_TYPE, (int)PlayerSettingMode.SelectedValue);
            };
            //播放列表
            PlayerSettingPlayMode.SelectedIndex = SettingService.GetValue<int>(SettingConstants.Player.DEFAULT_PLAY_MODE, 0);
            PlayerSettingPlayMode.SelectionChanged += new SelectionChangedEventHandler((e, args) =>
            {
                SettingService.SetValue<int>(SettingConstants.Player.DEFAULT_PLAY_MODE, PlayerSettingPlayMode.SelectedIndex);
            });
            //使用其他网站视频
            PlayerSettingUseOtherSite.IsOn = SettingService.GetValue<bool>(SettingConstants.Player.USE_OTHER_SITEVIDEO, false);
            PlayerSettingUseOtherSite.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingService.SetValue<bool>(SettingConstants.Player.USE_OTHER_SITEVIDEO, PlayerSettingUseOtherSite.IsOn);
            });
            //自动跳转
            PlayerSettingAutoToPosition.IsOn = SettingService.GetValue<bool>(SettingConstants.Player.AUTO_TO_POSITION, true);
            PlayerSettingAutoToPosition.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingService.SetValue<bool>(SettingConstants.Player.AUTO_TO_POSITION, PlayerSettingAutoToPosition.IsOn);
            });
            //自动跳转
            PlayerSettingAutoNext.IsOn = SettingService.GetValue<bool>(SettingConstants.Player.AUTO_NEXT, true);
            PlayerSettingAutoNext.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingService.SetValue<bool>(SettingConstants.Player.AUTO_NEXT, PlayerSettingAutoNext.IsOn);
            });
            //播放比例
            PlayerSettingRatio.SelectedIndex = SettingService.GetValue<int>(SettingConstants.Player.RATIO, 0);
            Player.SetRatioMode(PlayerSettingRatio.SelectedIndex);
            PlayerSettingRatio.SelectionChanged += new SelectionChangedEventHandler((e, args) =>
            {
                SettingService.SetValue<int>(SettingConstants.Player.RATIO, PlayerSettingRatio.SelectedIndex);
                Player.SetRatioMode(PlayerSettingRatio.SelectedIndex);
            });
            // 播放倍数
            InitPlaySpeed();

            _autoPlay = SettingService.GetValue<bool>(SettingConstants.Player.AUTO_PLAY, false);
            //A-B播放
            PlayerSettingABPlayMode.Toggled += new RoutedEventHandler((e, args) =>
            {
                if (PlayerSettingABPlayMode.IsOn)
                {
                    PlayerSettingABPlaySetPointA.Visibility = Visibility.Visible;
                }
                else
                {
                    Player.ABPlay = null;
                    VideoPlayHistoryHelper.SetABPlayHistory(CurrentPlayItem, null);
                    PlayerSettingABPlaySetPointA.Visibility = Visibility.Collapsed;
                    PlayerSettingABPlaySetPointB.Visibility = Visibility.Collapsed;
                    PlayerSettingABPlaySetPointA.Content = "设置A点";
                    PlayerSettingABPlaySetPointB.Content = "设置B点";
                }
            });
            //仅播放音频
            SwitchVideoEnable.Toggled += (e, args) =>
            {
                Player.SetVideoEnable(!SwitchVideoEnable.IsOn);
            };
        }
        private void LoadSutitleSetting()
        {
            //字幕加粗
            SubtitleSettingBold.IsOn = SettingService.GetValue<bool>(SettingConstants.Player.SUBTITLE_BOLD, true);
            SubtitleSettingBold.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingService.SetValue<bool>(SettingConstants.Player.SUBTITLE_BOLD, SubtitleSettingBold.IsOn);
                UpdateSubtitle();
            });

            //字幕大小
            SubtitleSettingSize.Value = SettingService.GetValue<double>(SettingConstants.Player.SUBTITLE_SIZE, 40);
            SubtitleSettingSize.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                if (miniWin) return;
                SettingService.SetValue<double>(SettingConstants.Player.SUBTITLE_SIZE, SubtitleSettingSize.Value);
                UpdateSubtitle();
            });
            //字幕描边颜色
            SubtitleSettingBorderColor.SelectedIndex = SettingService.GetValue<int>(SettingConstants.Player.SUBTITLE_BORDER_COLOR, 0);
            SubtitleSettingBorderColor.SelectionChanged += new SelectionChangedEventHandler((e, args) =>
            {
                SettingService.SetValue<int>(SettingConstants.Player.SUBTITLE_BORDER_COLOR, SubtitleSettingBorderColor.SelectedIndex);
                UpdateSubtitle();
            });
            //字幕边框颜色
            SubtitleSettingOutsideBorderColor.Text = SettingService.GetValue<string>(SettingConstants.Player.SUBTITLE_OUTSIDE_BORDER_COLOR, SettingConstants.Player.DEFAULT_SUBTITLE_OUTSIDE_BORDER_COLOR);
            SubtitleSettingOutsideBorderColor.QuerySubmitted += (e, args) =>
            {
                SettingService.SetValue(SettingConstants.Player.SUBTITLE_OUTSIDE_BORDER_COLOR, SubtitleSettingOutsideBorderColor.Text);
                UpdateSubtitle();
            };
            //字幕颜色
            SubtitleSettingColor.SelectedIndex = SettingService.GetValue<int>(SettingConstants.Player.SUBTITLE_COLOR, 0);
            SubtitleSettingColor.SelectionChanged += new SelectionChangedEventHandler((e, args) =>
            {
                SettingService.SetValue<int>(SettingConstants.Player.SUBTITLE_COLOR, SubtitleSettingColor.SelectedIndex);
                UpdateSubtitle();
            });

            //字幕对齐
            SubtitleSettingAlign.SelectedIndex = SettingService.GetValue<int>(SettingConstants.Player.SUBTITLE_ALIGN, 0);
            SubtitleSettingAlign.SelectionChanged += new SelectionChangedEventHandler((e, args) =>
            {
                SettingService.SetValue<int>(SettingConstants.Player.SUBTITLE_ALIGN, SubtitleSettingAlign.SelectedIndex);
                UpdateSubtitle();
            });

            //字幕透明度
            SubtitleSettingOpacity.Value = SettingService.GetValue<double>(SettingConstants.Player.SUBTITLE_OPACITY, 1.0);
            SubtitleSettingOpacity.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                SettingService.SetValue<double>(SettingConstants.Player.SUBTITLE_OPACITY, SubtitleSettingOpacity.Value);
            });
            //字幕底部距离
            SubtitleSettingBottom.Value = SettingService.GetValue<double>(SettingConstants.Player.SUBTITLE_BOTTOM, 40);
            BorderSubtitle.Margin = new Thickness(0, 0, 0, SubtitleSettingBottom.Value);
            SubtitleSettingBottom.ValueChanged += new RangeBaseValueChangedEventHandler((e, args) =>
            {
                BorderSubtitle.Margin = new Thickness(0, 0, 0, SubtitleSettingBottom.Value);
                SettingService.SetValue<double>(SettingConstants.Player.SUBTITLE_BOTTOM, SubtitleSettingBottom.Value);
            });
            //字幕转换
            SubtitleSettingToSimplified.IsOn = SettingService.GetValue<bool>(SettingConstants.Player.TO_SIMPLIFIED, true);
            SubtitleSettingToSimplified.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingService.SetValue<bool>(SettingConstants.Player.TO_SIMPLIFIED, SubtitleSettingToSimplified.IsOn);
                if (SubtitleSettingToSimplified.IsOn)
                {
                    currentSubtitleText = currentSubtitleText.ToSimplifiedChinese();
                    UpdateSubtitle();
                }
            });
        }

        public void LoadSponsorBlock()
        {
            if (CurrentPlayItem == null || CurrentPlayItem.bvid == null) return;
            if (m_playerSponsorBlockControl == null) return;
            m_playerSponsorBlockControl?.LoadSponsorBlock(CurrentPlayItem.bvid, CurrentPlayItem.cid, CurrentPlayItem.duration);
        }

        public void InitializePlayInfo(List<PlayInfo> playInfos, int index)
        {
            //保持屏幕常亮
            dispRequest.RequestActive();

            PlayInfos = playInfos;
            EpisodeList.ItemsSource = PlayInfos;
            if (PlayInfos.Count > 1)
            {
                ShowPlaylistButton = true;
            }
            else if (PlayInfos.Count == 1 && PlayInfos[0].is_interaction)
            {
                ShowPlaylistButton = true;
            }
            else
            {
                ShowPlaylistButton = false;
            }
            EpisodeList.SelectedIndex = index;
        }

        private List<DanmakuItem> FilterFrostDanmaku(IEnumerable<DanmakuItem> danmakus)
        {
            try
            {
                var needDistinct = DanmuSettingMerge.IsOn;
                var level = DanmuSettingShieldLevel.Value;
                var max = System.Convert.ToInt32(DanmuSettingMaxNum.Value);
                //云屏蔽
                danmakus = danmakus.Where(x => x.Weight >= level);
                //去重
                danmakus = danmakus.DistinctIf(needDistinct, new CompareDanmakuItem());

                //关键词
                foreach (var item in m_danmakuSettingsControlViewModel.ShieldWords)
                {
                    danmakus = danmakus.Where(x => !x.Text.Contains(item));
                }
                //用户
                foreach (var item in m_danmakuSettingsControlViewModel.ShieldUsers)
                {
                    danmakus = danmakus.Where(x => !x.MidHash.Equals(item));
                }
                //正则
                foreach (var item in m_danmakuSettingsControlViewModel.ShieldRegulars)
                {
                    danmakus = danmakus.Where(x => !Regex.IsMatch(x.Text, item));
                }
                //彩色
                danmakus = danmakus.WhereIf(
                    DanmuSettingDisableColorful.IsOn,
                    x => x.TextColor == Colors.White);

                // 同屏密度
                if (max > 0)
                {
                    // 弹幕按每秒分组，每组取前x项
                    danmakus = danmakus.GroupBy(x => (x.StartMs / 1000) * 1000)
                        .ToDictionary(x => (int)x.Key, x => x.ToList())
                        .SelectMany(x => x.Value.Take(max));
                }

                // 移除当前播放时间之前的弹幕，避免弹幕堆叠
                danmakus = danmakus.Where(x => x.StartMs > Player.Position * 1000);

                return danmakus.ToList();
            }
            catch (Exception ex)
            {
                _logger.Error("弹幕筛选错误:" + ex.Message, ex);
                return new List<DanmakuItem>();
            }
        }

        private async Task SelectNsDanmakuAndLoad(int position, double level, bool needDistinct, int max)
        {
            try
            {
                if (danmakuPool != null && danmakuPool.ContainsKey(position))
                {
                    var data = danmakuPool[position] as IEnumerable<DanmakuModel>;
                    data = data.Where(x => true);
                    //云屏蔽
                    data = data.Where(x => x.weight >= level);
                    //去重
                    if (needDistinct)
                    {
                        data = data.Distinct(new CompareDanmakuModel());
                    }
                    //关键词
                    foreach (var item in m_danmakuSettingsControlViewModel.ShieldWords)
                    {
                        data = data.Where(x => !x.text.Contains(item));
                    }
                    //用户
                    foreach (var item in m_danmakuSettingsControlViewModel.ShieldUsers)
                    {
                        data = data.Where(x => !x.sendID.Equals(item));
                    }
                    //正则
                    foreach (var item in m_danmakuSettingsControlViewModel.ShieldRegulars)
                    {
                        data = data.Where(x => !Regex.IsMatch(x.text, item));
                    }
                    //彩色
                    data = data.WhereIf(
                        DanmuSettingDisableColorful.IsOn,
                        x => x.color == Colors.White);
                    if (max > 0)
                    {
                        data = data.Take(max);
                    }
                    //加载弹幕
                    m_danmakuController.Load(data);
                    data = null;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("弹幕筛选错误:" + ex.Message, ex);
            }
        }

        private async void DanmuTimer_Tick(object sender, object e)
        {
            if (!SettingService.GetValue(SettingConstants.Player.ALWAYS_SHOW_VIDEO_PROGRESS_BAR, SettingConstants.Player.DEFAULT_ALWAYS_SHOW_VIDEO_PROGRESS_BAR))
            {
                if (showControlsFlag != -1)
                {
                    if (showControlsFlag >= 5)
                    {
                        var elent = FocusManager.GetFocusedElement();
                        if (elent is not TextBox && elent is not AutoSuggestBox)
                        {
                            ShowControl(false);
                            showControlsFlag = -1;
                        }
                        //FadeOut.Begin();
                        //control.Visibility = Visibility.Collapsed;

                    }
                    else
                    {
                        showControlsFlag++;
                    }
                }
            }

            var position = Convert.ToInt32(Player.Position);
            var segIndex = Convert.ToInt32(Math.Ceiling(Player.Position / (60 * 6d)));
            if (segIndex <= 0) segIndex = 1;
            if (danmakuLoadedSegment != null && !danmakuLoadedSegment.Contains(segIndex))
            {
                await LoadDanmaku(segIndex);
            }
            else if (position < m_danmakuController.Position && !m_useNsDanmaku && Player.PlayState == PlayState.Playing)
            {
                await LoadDanmaku(segIndex);
            }

            if (Buffering)
            {
                return;
            }
            if (Player.PlayState != PlayState.Playing || GridBuffering.Visibility == Visibility.Visible)
            {
                return;
            }

            if (m_danmakuController.DanmakuViewModel.IsHide)
            {
                return;
            }
            m_danmakuController.UpdateTime(position);

            var needDistinct = DanmuSettingMerge.IsOn;
            var level = DanmuSettingShieldLevel.Value;
            var max = System.Convert.ToInt32(DanmuSettingMaxNum.Value);

            if (m_useNsDanmaku)
            {
                await SelectNsDanmakuAndLoad(position, level, needDistinct, max);
            }

            if (Player.PlayState == PlayState.Pause)
            {
                m_danmakuController.Pause();
            }
        }

        private void PositionTimer_Tick(object sender, object e)
        {
            if (!IsPlaying) return;
            m_viewModel.Position = Player.Position;
            //PluginCenter.BroadcastPosition(this, Player.Position);
            if (CurrentPlayItem == null) return;
            if (CurrentPlayItem.EpisodeSkip != null)
            {
                if (!m_autoSkipOpEdFlag) return;
                SkipSection(CurrentPlayItem.EpisodeSkip.Op, "SkipOp", "自动跳过OP");
                SkipSection(CurrentPlayItem.EpisodeSkip.Ed, "SkipEd", "自动跳过ED");
            }

            if (m_playerSponsorBlockControl == null) return;
            if (m_playerSponsorBlockControl.SegmentSkipItems != null)
            {
                foreach (var seg in m_playerSponsorBlockControl.SegmentSkipItems)
                {
                    SkipSection(seg, "SkipSponsor", "");
                }
            }
        }

        private void SkipSection(PlayerSkipItem section, string toastId, string message)
        {
            if (section == null) return;

            var gap = Math.Abs(Player.Position - section.Start);
            if (!section.IsSectionValid || gap > 0.5) return; //更大的宽容范围检测

            Task.Delay(TimeSpan.FromSeconds(gap));
            if (section.CategoryEnum == SponsorBlockType.Sponsor)
            {
                SetPosition(section.End);
                m_playerToastService.Show(toastId, $"自动跳过{section.SegmentName}", seg: section);
            }
            else
            {
                var showTime = (long)((section.End - section.Start) * 1000);
                m_playerToastService.Show(toastId, $"跳过{section.SegmentName}？", showTime > 10000 ? 10000 : showTime - 1500, section);
            }
        }

        private async Task SetPlayItem(int index)
        {
            if (PlayInfos == null || PlayInfos.Count == 0)
            {
                return;
            }
            //清空字幕
            subtitles = null;
            subtitleTimer?.Stop();
            subtitleTimer = null;
            Pause();
            Player.ClosePlay();

            m_autoRefreshTimer?.Stop();

            if (index >= PlayInfos.Count)
            {
                index = PlayInfos.Count - 1;
            }

            CurrentPlayIndex = index;
            CurrentPlayItem = PlayInfos[index];
            if (CurrentPlayItem.is_interaction)
            {
                ShowPlaylistButton = false;
                ShowPlayNodeButton = true;
            }
            //设置标题
            TopTitle.Text = CurrentPlayItem.title;
            if (_systemMediaTransportControls != null)
            {
                SystemMediaTransportControlsDisplayUpdater updater = _systemMediaTransportControls.DisplayUpdater;
                updater.Type = MediaPlaybackType.Video;
                updater.VideoProperties.Title = CurrentPlayItem.title;
                updater.Update();
            }

            //设置下一集按钮的显示
            if (PlayInfos.Count >= 1 && index != PlayInfos.Count - 1)
            {
                BottomBtnNext.Visibility = Visibility.Visible;
            }
            else
            {
                BottomBtnNext.Visibility = Visibility.Collapsed;
            }
            ChangeEpisodeEvent?.Invoke(this, index);

            playUrlInfo = null;
            //if (CurrentPlayItem.play_mode == VideoPlayType.Season)
            //{
            //   // Player._ffmpegConfig.FFmpegOptions["referer"] = "https://www.bilibili.com/bangumi/play/ep" + CurrentPlayItem.ep_id;
            //}
            if (SettingService.GetValue<bool>(SettingConstants.Player.AUTO_TO_POSITION, true))
            {
                _postion = SettingService.GetValue<double>(CurrentPlayItem.season_id != 0 ? "ep" + CurrentPlayItem.ep_id : CurrentPlayItem.cid, 0);

                //从头播放完播视频
                long totalSecend = CurrentPlayItem.duration;
                int lastTimeOffset = SettingService.GetValue(SettingConstants.Player.REPLAY_VIEDO_FROM_END_LAST_TIME, 
                    SettingConstants.Player.DEFAULT_REPLAY_VIEDO_FROM_END_LAST_TIME);
                if (totalSecend - _postion < lastTimeOffset && totalSecend > 30)
                {
                    _postion = 0;
                }
                //减去两秒防止视频直接结束了
                if (_postion >= 2) _postion -= 2;
            }
            else
            {
                _postion = 0;
            }
            _logger.Trace("SetPlayItem,上报进度");
            await ReportHistory(0);
            await SetDanmaku();

            if (!await CheckDownloaded())
            {
                var info = await GetPlayUrlQualitesInfo();
                if (!info.Success)
                {
                    await NotificationShowExtensions.ShowMessageDialog($"请求信息:\r\n{info.Message}", "读取视频播放地址失败");
                }
                else
                {
                    playUrlInfo = info;
                }
            }

            if (m_isLocalFileMode || playUrlInfo != null)
            {
                InitSoundQuality();
                InitQuality();
            }

            await GetPlayerInfo();

            CheckVolumeAndBrightnessLower();

            Player.ABPlay = VideoPlayHistoryHelper.FindABPlayHistory(CurrentPlayItem);
            if (Player.ABPlay == null)
            {
                PlayerSettingABPlayMode.IsOn = false;
            }
            else
            {
                PlayerSettingABPlayMode.IsOn = true;
                PlayerSettingABPlaySetPointA.Visibility = Visibility.Visible;
                PlayerSettingABPlaySetPointB.Visibility = Visibility.Visible;
                PlayerSettingABPlaySetPointA.Content = "A: " + TimeSpan.FromSeconds(Player.ABPlay.PointA).ToString(@"hh\:mm\:ss\.fff");
                if (Player.ABPlay.PointB != double.MaxValue)
                    PlayerSettingABPlaySetPointB.Content = "B: " + TimeSpan.FromSeconds(Player.ABPlay.PointB).ToString(@"hh\:mm\:ss\.fff");
            }

            LoadSponsorBlock();
        }


        /// <summary>
        /// 字幕文件
        /// </summary>
        SubtitleModel subtitles;
        /// <summary>
        /// 字幕Timer
        /// </summary>
        DispatcherTimer subtitleTimer;
        /// <summary>
        /// 当前显示的字幕文本
        /// </summary>
        string currentSubtitleText = "";
        /// <summary>
        /// 选择字幕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Menuitem_Click(object sender, RoutedEventArgs e)
        {

            foreach (ToggleMenuFlyoutItem item in (BottomBtnSelctSubtitle.Flyout as MenuFlyout).Items)
            {
                item.IsChecked = false;
            }
            var menuitem = (sender as ToggleMenuFlyoutItem);
            CurrentSubtitleName = menuitem.Text;
            if (menuitem.Text == "无")
            {
                ClearSubTitle();
            }
            else
            {
                SetSubTitle(menuitem.Tag.ToString());
            }
            menuitem.IsChecked = true;
        }
        /// <summary>
        /// 设置字幕文件
        /// </summary>
        /// <param name="url"></param>
        private async void SetSubTitle(string url)
        {
            try
            {
                subtitles = await playerHelper.GetSubtitle(url);
                if (subtitles != null)
                {
                    //转为简体
                    if (SettingService.GetValue<bool>(SettingConstants.Player.TO_SIMPLIFIED, true) && CurrentSubtitleName == "中文（繁体）")
                    {
                        foreach (var item in subtitles.body)
                        {
                            item.content = item.content.ToSimplifiedChinese();
                        }
                    }
                    subtitleTimer = new DispatcherTimer();
                    subtitleTimer.Interval = TimeSpan.FromMilliseconds(100);
                    subtitleTimer.Tick += SubtitleTimer_Tick;
                    subtitleTimer.Start();
                }
            }
            catch (Exception)
            {
                NotificationShowExtensions.ShowMessageToast("加载字幕失败了");
            }


        }

        private async void SubtitleTimer_Tick(object sender, object e)
        {
            if (Player.PlayState != PlayState.Playing) return;

            if (subtitles == null)
            {
                return;
            }
            var time = Player.Position;
            if (subtitles.body == null) return;
            var first = subtitles.body.FirstOrDefault(x => x.from <= time && x.to >= time);
            if (first != null)
            {
                if (first.content == currentSubtitleText) return;
                BorderSubtitle.Visibility = Visibility.Visible;
                BorderSubtitle.Child = await GenerateSubtitleItem(first.content);
                BorderSubtitle.Background = new SolidColorBrush(SubtitleSettingOutsideBorderColor.Text.StrToColor());
                currentSubtitleText = first.content;
            }
            else
            {
                BorderSubtitle.Visibility = Visibility.Collapsed;
                currentSubtitleText = "";
            }
        }

        private async void UpdateSubtitle()
        {
            if (BorderSubtitle.Visibility == Visibility.Visible && currentSubtitleText != "")
            {
                BorderSubtitle.Child = await GenerateSubtitleItem(currentSubtitleText);
            }


        }

        private async Task<Grid> GenerateSubtitleItem(string text)
        {
            //行首行尾加空格，防止字体描边超出
            text = " " + text.Replace("\n", " \n ") + " ";

            var fontSize = (float)SubtitleSettingSize.Value;
            var color = (SubtitleSettingColor.SelectedItem as ComboBoxItem).Tag.ToString().StrToColor();
            var borderColor = (SubtitleSettingBorderColor.SelectedItem as ComboBoxItem).Tag.ToString().StrToColor();

            CanvasHorizontalAlignment canvasHorizontalAlignment = CanvasHorizontalAlignment.Center;
            TextAlignment textAlignment = TextAlignment.Center;
            if (SubtitleSettingAlign.SelectedIndex == 1)
            {
                canvasHorizontalAlignment = CanvasHorizontalAlignment.Left;
                textAlignment = TextAlignment.Left;
            }
            else if (SubtitleSettingAlign.SelectedIndex == 2)
            {
                canvasHorizontalAlignment = CanvasHorizontalAlignment.Right;
                textAlignment = TextAlignment.Right;
            }
            CanvasDevice device = CanvasDevice.GetSharedDevice();
            CanvasTextFormat fmt = new CanvasTextFormat() { FontSize = fontSize, HorizontalAlignment = canvasHorizontalAlignment, };
            var tb = new TextBlock { Text = text, FontSize = fontSize, TextAlignment = textAlignment };
            if (SubtitleSettingBold.IsOn)
            {
                fmt.FontWeight = FontWeights.Bold;
                tb.FontWeight = FontWeights.Bold;
            }

            tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            var myBitmap = new CanvasRenderTarget(device, (float)tb.DesiredSize.Width + 4, (float)tb.DesiredSize.Height, displayInformation.LogicalDpi);

            CanvasTextLayout canvasTextLayout = new CanvasTextLayout(device, text, fmt, (float)tb.DesiredSize.Width + 4, (float)tb.DesiredSize.Height);

            CanvasGeometry combinedGeometry = CanvasGeometry.CreateText(canvasTextLayout);

            using (var ds = myBitmap.CreateDrawingSession())
            {
                ds.Clear(Colors.Transparent);
                ds.DrawGeometry(combinedGeometry, borderColor, 4f, new CanvasStrokeStyle()
                {
                    DashStyle = CanvasDashStyle.Solid
                });
                ds.FillGeometry(combinedGeometry, color);
            }
            Image image = new Image();
            BitmapImage im = new BitmapImage();
            using (InMemoryRandomAccessStream oStream = new InMemoryRandomAccessStream())
            {
                await myBitmap.SaveAsync(oStream, CanvasBitmapFileFormat.Png, 1.0f);
                await im.SetSourceAsync(oStream);
            }
            image.Width = tb.DesiredSize.Width;
            image.Source = im;
            image.Stretch = Stretch.Uniform;
            Grid grid = new Grid();

            grid.Tag = text;
            grid.Children.Add(image);

            return grid;
        }


        /// <summary>
        /// 清除字幕
        /// </summary>
        private void ClearSubTitle()
        {
            if (subtitles != null)
            {
                if (subtitleTimer != null)
                {
                    subtitleTimer.Stop();
                    subtitleTimer = null;
                }
                BorderSubtitle.Visibility = Visibility.Collapsed;
                subtitles = null;
            }
        }



        public void ChangePlayIndex(int index)
        {
            ClearSubTitle();
            m_danmakuController.Clear();
            EpisodeList.SelectedIndex = index;
        }
        private async void EpisodeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EpisodeList.SelectedItem == null)
            {
                return;
            }
            m_danmakuController.Clear();
            await SetPlayItem(EpisodeList.SelectedIndex);
        }
        private async void NodeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NodeList.SelectedItem == null || interactionVideoVM.Loading)
            {
                return;
            }
            var item = NodeList.SelectedItem as InteractionEdgeInfoStoryListModel;
            await ChangedNode(item.edge_id, item.cid.ToString());
        }
        private async void SelectChoice_ItemClick(object sender, ItemClickEventArgs e)
        {
            var choice = e.ClickedItem as InteractionEdgeInfoChoiceModel;
            await ChangedNode(choice.id, choice.cid.ToString());

        }
        private async Task ChangedNode(int node_id, string cid)
        {
            InteractionChoices.Visibility = Visibility.Collapsed;
            CurrentPlayItem.cid = cid;

            await interactionVideoVM.GetNodes(node_id);
            m_viewModel.Questions = interactionVideoVM.Info.edges.questions;

            TopTitle.Text = interactionVideoVM.Select.title;
            //if ((interactionVideoVM.Info.edges?.questions?.Count ?? 0) <= 0)
            //{
            //    NotificationShowExtensions.ShowMessageToast("播放完毕，请点击右下角节点，重新开始");
            //    return;
            //}
            _postion = 0;
            _autoPlay = true;
            m_danmakuController.Clear();
            await SetDanmaku();


            if (!await CheckDownloaded())
            {
                var info = await GetPlayUrlQualitesInfo();
                if (!info.Success)
                {
                    await NotificationShowExtensions.ShowMessageDialog($"请求信息:\r\n{info.Message}", "读取视频播放地址失败");
                }
                else
                {
                    playUrlInfo = info;
                    InitSoundQuality();
                    InitQuality();
                }
            }
        }


        double _postion = 0;
        bool _autoPlay = false;
        private async Task SetDanmaku(bool update = false)
        {
            try
            {

                if (CurrentPlayItem.play_mode == VideoPlayType.Download && !update)
                {
                    var danmakuFile = await StorageFile.GetFileFromPathAsync(CurrentPlayItem.LocalPlayInfo.DanmakuPath);
                    if (m_useNsDanmaku)
                    {
                        var danmuList = danmakuParse.ParseBiliBili(await FileIO.ReadTextAsync(danmakuFile));
                        danmakuPool = danmuList.GroupBy(x => x.time_s).ToDictionary(x => x.Key, x => x.ToList());
                        TxtDanmuCount.Text = danmuList.Count.ToString();
                        danmuList.Clear();
                        danmuList = null;
                    }
                    else
                    {
                        var file = await FileIO.ReadTextAsync(danmakuFile);
                        var danmuList = BilibiliDanmakuXmlParser.GetDanmakuList(file, null, false, out _, out _, out _);
                        //danmakuPool = danmuList.GroupBy(x => x.StartMs).ToDictionary(x => (int)x.Key, x => (object)x.ToList());

                        m_danmakuController.Load(danmuList);
                        TxtDanmuCount.Text = danmuList.Count.ToString();
                        danmuList.Clear();
                        danmuList = null;
                    }
                }
                else
                {
                    if (update)
                    {
                        var segIndex = Math.Ceiling(Player.Position / (60 * 6d));
                        await LoadDanmaku(segIndex.ToInt32());
                        NotificationShowExtensions.ShowMessageToast($"已更新弹幕");
                    }
                    else
                    {
                        await LoadDanmaku(1);
                    }
                    //var danmuList = (await danmakuParse.ParseBiliBili(System.Convert.ToInt64(CurrentPlayItem.cid)));
                    ////await playerHelper.GetDanmaku(CurrentPlayItem.cid, 1) ;
                    //danmakuPool = danmuList.GroupBy(x=>x.time_s).ToDictionary(x => x.Key, x => x.ToList());
                    //TxtDanmuCount.Text = danmuList.Count.ToString();

                    //danmuList.Clear();
                    //danmuList = null;
                }


            }
            catch (Exception ex)
            {
                NotificationShowExtensions.ShowMessageToast("弹幕加载失败");
                _logger.Error("弹幕加载失败", ex);
            }
        }
        bool loadingDanmaku = false;

        private async Task LoadNSDanmaku(int segmentIndex)
        {
            var danmuList = await playerHelper.GetDanmaku(CurrentPlayItem.cid, segmentIndex);
            foreach (var item in danmuList.GroupBy(x => x.time_s).ToDictionary(x => x.Key, x => x.ToList()))
            {
                if (danmakuPool.ContainsKey(item.Key))
                {
                    danmakuPool[item.Key] = item.Value;
                }
                else
                {
                    danmakuPool.Add(item.Key, item.Value);
                }
            }
            TxtDanmuCount.Text = danmuList.Count.ToString();
            danmuList.Clear();
        }

        private async Task LoadFrostMasterDanmaku(int segmentIndex)
        {
            var danmuList = await playerHelper.GetDanmakuForDanmakuFrostMaster(CurrentPlayItem.cid, segmentIndex);
            danmuList = FilterFrostDanmaku(danmuList);
            m_danmakuController.Clear();
            m_danmakuController.Load(danmuList);
            TxtDanmuCount.Text = danmuList.Count.ToString();
        }

        private async Task LoadDanmaku(int segmentIndex)
        {

            try
            {
                if (loadingDanmaku) return;
                loadingDanmaku = true;
                if (segmentIndex <= 1)
                {
                    danmakuPool.Clear();
                    segmentIndex = 1;
                }

                if (m_useNsDanmaku)
                {
                    await LoadNSDanmaku(segmentIndex);
                }
                else
                {
                    await LoadFrostMasterDanmaku(segmentIndex);
                }

                if (segmentIndex == 1)
                {
                    danmakuLoadedSegment = new List<int>() { 1 };
                }
                else
                {
                    danmakuLoadedSegment.Add(segmentIndex);
                }
            }
            catch (Exception)
            {

            }
            finally
            {
                loadingDanmaku = false;
            }

        }

        private async Task<bool> CheckDownloaded()
        {
            if (CurrentPlayItem.play_mode != VideoPlayType.Download)
            {
                return false;
            }

            await PlayLocalFile();
            return true;
        }

        private async Task<BiliPlayUrlQualitesInfo> GetPlayUrlQualitesInfo()
        {
            VideoLoading.Visibility = Visibility.Visible;
            if (playUrlInfo != null && playUrlInfo.CurrentQuality != null)
            {
                playUrlInfo.CurrentQuality = null;
            }

            var qn = SettingService.GetValue<int>(SettingConstants.Player.DEFAULT_QUALITY, 80);
            var soundQualityId = SettingService.GetValue<int>(SettingConstants.Player.DEFAULT_SOUND_QUALITY, 0);
            var info = await playerHelper.GetPlayUrls(CurrentPlayItem, qn, soundQualityId);
            return info;
        }

        #region Slider

        private void InitSoundQuality()
        {
            var audioTrackInfos = m_isLocalFileMode
                ? CurrentPlayItem.LocalPlayInfo?.AudioTrackInfos
                : playUrlInfo?.AudioQualites;

            if (audioTrackInfos == null || !audioTrackInfos.Any())
            {
                return;
            }

            var currentAudioTrack = m_isLocalFileMode
                ? CurrentPlayItem.LocalPlayInfo.CurrentAudioTrack
                : playUrlInfo.CurrentAudioQuality;

            MinSoundQuality.Text = audioTrackInfos.First().QualityName;
            MaxSoundQuality.Text = audioTrackInfos.Last().QualityName;

            BottomBtnSoundQuality.IsEnabled = audioTrackInfos.Count > 1;
            BottomBtnSoundQuality.Content = currentAudioTrack.QualityName;
            SliderSoundQuality.Maximum = audioTrackInfos.Count - 1;
            SliderSoundQuality.Value = audioTrackInfos.IndexOf(currentAudioTrack);
            m_soundQualitySliderTooltipConverter.AudioQualites = audioTrackInfos;
            SliderSoundQuality.ThumbToolTipValueConverter = m_soundQualitySliderTooltipConverter;

            // ChangeQuality(current_quality_info, playUrlInfo.CurrentAudioQuality).RunWithoutAwait();
        }

        private async void SliderSoundQuality_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!m_firstMediaOpened) return;
            _postion = Player.Position;
            _autoPlay = Player.PlayState == PlayState.Playing;
            BiliDashAudioPlayUrlInfo latestChoice = null;
            if (m_isLocalFileMode)
            {
                latestChoice = CurrentPlayItem.LocalPlayInfo.AudioTrackInfos[(int)SliderSoundQuality.Value];
                CurrentPlayItem.LocalPlayInfo.Info.DashInfo.Audio = latestChoice.Audio;
                await ChangeQualityPlayLocalVideo(CurrentPlayItem.LocalPlayInfo.Info.DashInfo);
            }
            else
            {
                latestChoice = playUrlInfo.AudioQualites[(int)SliderSoundQuality.Value];
                SettingService.SetValue<int>(SettingConstants.Player.DEFAULT_SOUND_QUALITY, latestChoice.QualityID);
                await ChangeQuality(current_quality_info, latestChoice);
            }
            BottomBtnSoundQuality.Content = latestChoice.QualityName;
        }

        private void InitQuality()
        {
            var videoTrackInfos = m_isLocalFileMode
                ? CurrentPlayItem.LocalPlayInfo?.VideoTrackInfos
                : playUrlInfo?.Qualites;

            if (videoTrackInfos == null || !videoTrackInfos.Any())
            {
                return;
            }

            var currentVideoTrack = m_isLocalFileMode
                ? CurrentPlayItem.LocalPlayInfo.CurrentVideoTrack
                : playUrlInfo.CurrentQuality;

            SetQualityControls(videoTrackInfos, currentVideoTrack);

            if (!m_isLocalFileMode)
            {
                ChangeQuality(playUrlInfo.CurrentQuality, playUrlInfo.CurrentAudioQuality).RunWithoutAwait();
            }
        }

        private void SetQualityControls(List<BiliPlayUrlInfo> videoTrackInfos, BiliPlayUrlInfo currentVideoTrack)
        {
            MinQuality.Text = videoTrackInfos.First().QualityName;
            MaxQuality.Text = videoTrackInfos.Last().QualityName;

            BottomBtnQuality.IsEnabled = videoTrackInfos.Count > 1;
            BottomBtnQuality.Content = currentVideoTrack.QualityName;
            SliderQuality.Maximum = videoTrackInfos.Count - 1;
            SliderQuality.Value = videoTrackInfos.IndexOf(currentVideoTrack);
            m_qualitySliderTooltipConverter.Qualites = videoTrackInfos;
            SliderQuality.ThumbToolTipValueConverter = m_qualitySliderTooltipConverter;
        }

        private async void SliderQuality_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!m_firstMediaOpened) return;
            _postion = Player.Position;
            _autoPlay = Player.PlayState == PlayState.Playing;

            BiliPlayUrlInfo latestChoice = null;

            if (m_isLocalFileMode)
            {
                latestChoice = CurrentPlayItem.LocalPlayInfo.VideoTrackInfos[(int)SliderQuality.Value];
                CurrentPlayItem.LocalPlayInfo.Info.DashInfo.Video = latestChoice.DashInfo.Video;
                await ChangeQualityPlayLocalVideo(CurrentPlayItem.LocalPlayInfo.Info.DashInfo);
            }
            else
            {
                latestChoice = playUrlInfo.Qualites[(int)SliderQuality.Value];
                SettingService.SetValue<int>(SettingConstants.Player.DEFAULT_QUALITY, latestChoice.QualityID);
                await ChangeQuality(latestChoice, current_audio_quality_info);
            }

            BottomBtnQuality.Content = latestChoice.QualityName;
        }

        private void InitPlaySpeed()
        {
            MinPlaySpeed.Text = m_playSpeedMenuService.MenuItems[0].Content;
            MaxPlaySpeed.Text = m_playSpeedMenuService.MenuItems[m_playSpeedMenuService.MenuItems.Count - 1].Content;
            SliderPlaySpeed.Maximum = m_playSpeedMenuService.MenuItems.Count - 1;
            SliderPlaySpeed.Minimum = 0;

            // 强行居中矫正1x倍速
            var lessThanOneCount = m_playSpeedMenuService.MenuItems.ToList().FindIndex(x => x.Value == 1);
            var moreThanOneCount = m_playSpeedMenuService.MenuItems.Count - lessThanOneCount - 1;
            if (lessThanOneCount != 0 && moreThanOneCount != 0)
            {
                var differenceCount = lessThanOneCount - moreThanOneCount;
                switch (differenceCount)
                {
                    case > 0:
                        SliderPlaySpeed.Maximum = m_playSpeedMenuService.MenuItems.Count - 1 + differenceCount;
                        SliderPlaySpeed.Minimum = 0;
                        break;
                    case < 0:
                        SliderPlaySpeed.Maximum = m_playSpeedMenuService.MenuItems.Count - 1;
                        SliderPlaySpeed.Minimum = differenceCount;
                        break;
                }
            }

            var value = SettingService.GetValue<double>(SettingConstants.Player.DEFAULT_VIDEO_SPEED, 1.0d);
            var CurrentPlaySpeed = m_playSpeedMenuService.MenuItems.FirstOrDefault(x => x.Value == value);

            BottomBtnPlaySpeed.IsEnabled = m_playSpeedMenuService.MenuItems.Count > 1;
            BottomBtnPlaySpeed.Content = CurrentPlaySpeed.Content;
            SliderPlaySpeed.Value = m_playSpeedMenuService.MenuItems.IndexOf(CurrentPlaySpeed);
            SliderPlaySpeed.ThumbToolTipValueConverter = m_playSpeedSliderTooltipConverter;

            Player.SetRate(CurrentPlaySpeed.Value);
        }

        private void SliderPlaySpeed_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (SliderPlaySpeed.Value < 0)
            {
                SliderPlaySpeed.Value = 0;
            }
            if (SliderPlaySpeed.Value > m_playSpeedMenuService.MenuItems.Count - 1)
            {
                SliderPlaySpeed.Value = m_playSpeedMenuService.MenuItems.Count - 1;
            }

            var latestChoice = m_playSpeedMenuService.MenuItems[(int)SliderPlaySpeed.Value];
            Player.SetRate(latestChoice.Value);
            BottomBtnPlaySpeed.Content = latestChoice.Content;

            SettingService.SetValue(SettingConstants.Player.DEFAULT_VIDEO_SPEED, latestChoice.Value);
        }

        // 快捷键减速播放
        public void SlowDown()
        {
            var index = (int)SliderPlaySpeed.Value;
            if (index <= 0)
            {
                NotificationShowExtensions.ShowMessageToast("不能再慢啦");
                return;
            }

            SliderPlaySpeed.Value = index - 1;
            m_playerToastService.Show(PlayerToastService.SPEED_KEY, $"{m_playSpeedMenuService.MenuItems[(int)SliderPlaySpeed.Value].Content}");
        }

        // 快捷键加速播放
        public void FastUp()
        {
            var index = (int)SliderPlaySpeed.Value;
            if (index >= m_playSpeedMenuService.MenuItems.Count - 1)
            {
                NotificationShowExtensions.ShowMessageToast("不能再快啦");
                return;
            }

            SliderPlaySpeed.Value = index + 1;
            m_playerToastService.Show(PlayerToastService.SPEED_KEY, $"{m_playSpeedMenuService.MenuItems[(int)SliderPlaySpeed.Value].Content}");
        }

        // 快捷键获取播放速度
        public double GetPlaySpeed() => SliderPlaySpeed.Value;

        // 快捷键设置播放速度
        public void SetPlaySpeed(double speed)
        {
            SliderPlaySpeed.Value = speed;
        }
        #endregion

        private async Task PlayLocalFile()
        {
            VideoLoading.Visibility = Visibility.Visible;
            m_isLocalFileMode = true;
            PlayerOpenResult result = new PlayerOpenResult()
            {
                result = false
            };
            var info = CurrentPlayItem.LocalPlayInfo.Info;
            if (info.PlayUrlType == BiliPlayUrlType.DASH)
            {
                var playInfo = CurrentPlayItem.LocalPlayInfo.VideoTrackInfos.FirstOrDefault();
                CurrentPlayItem.LocalPlayInfo.CurrentAudioTrack = CurrentPlayItem.LocalPlayInfo.AudioTrackInfos.FirstOrDefault();
                CurrentPlayItem.LocalPlayInfo.CurrentVideoTrack = CurrentPlayItem.LocalPlayInfo.VideoTrackInfos.FirstOrDefault();
                CurrentPlayItem.LocalPlayInfo.Info.DashInfo = playInfo.DashInfo;
                playInfo.DashInfo.Audio = CurrentPlayItem.LocalPlayInfo.CurrentAudioTrack?.Audio;
                playInfo.DashInfo.Video = CurrentPlayItem.LocalPlayInfo.CurrentVideoTrack?.DashInfo.Video;
                result = await Player.PlayDashUseFFmpegInterop(playInfo.DashInfo, "", "", positon: _postion,
                    isLocal: true);
            }
            else if (CurrentPlayItem.LocalPlayInfo.Info.PlayUrlType == BiliPlayUrlType.SingleFLV)
            {
                result = await Player.PlayerSingleMp4UseNativeAsync(info.FlvInfo.First().Url, positon: _postion, isLocal: true);
            }
            else if (CurrentPlayItem.LocalPlayInfo.Info.PlayUrlType == BiliPlayUrlType.MultiFLV)
            {
                //TODO 本地播放
            }
            if (result.result)
            {
                VideoLoading.Visibility = Visibility.Collapsed;
                await Play();
            }
            else
            {
                NotificationShowExtensions.ShowVideoErrorMessageDialog(result.message + "[LocalFile]");
            }
        }

        private async Task GetPlayerInfo()
        {
            TopOnline.Text = "";
            var autoAISubtitle = SettingService.GetValue<bool>(SettingConstants.Player.AUTO_OPEN_AI_SUBTITLE, false);
            if (CurrentPlayItem.play_mode == VideoPlayType.Download)
            {
                if (CurrentPlayItem.LocalPlayInfo.Subtitles != null && CurrentPlayItem.LocalPlayInfo.Subtitles.Count > 0)
                {

                    var menu = new MenuFlyout();
                    foreach (var item in CurrentPlayItem.LocalPlayInfo.Subtitles)
                    {
                        ToggleMenuFlyoutItem menuitem = new ToggleMenuFlyoutItem() { Text = item.Key, Tag = item.Value };
                        menuitem.Click += Menuitem_Click;
                        menu.Items.Add(menuitem);
                    }
                    ToggleMenuFlyoutItem noneItem = new ToggleMenuFlyoutItem() { Text = "无" };
                    noneItem.Click += Menuitem_Click;
                    menu.Items.Add(noneItem);
                    var firstMenuItem = (menu.Items[0] as ToggleMenuFlyoutItem);
                    if ((firstMenuItem.Text.Contains("自动") || firstMenuItem.Text.Contains("AI")) && !autoAISubtitle)
                    {
                        noneItem.IsChecked = true;
                        CurrentSubtitleName = noneItem.Text;
                    }
                    else
                    {
                        firstMenuItem.IsChecked = true;
                        CurrentSubtitleName = firstMenuItem.Text;
                        SetSubTitle(firstMenuItem.Tag.ToString());
                    }
                    BottomBtnSelctSubtitle.Flyout = menu;
                    BottomBtnSelctSubtitle.Visibility = Visibility.Visible;
                    BorderSubtitle.Visibility = Visibility.Collapsed;
                }
                else
                {
                    var menu = new MenuFlyout();
                    menu.Items.Add(new ToggleMenuFlyoutItem() { Text = "无", IsChecked = true });
                    CurrentSubtitleName = "无";
                    BottomBtnSelctSubtitle.Flyout = menu;
                    BottomBtnSelctSubtitle.Visibility = Visibility.Collapsed;
                    BorderSubtitle.Visibility = Visibility.Collapsed;
                }
                return;
            }
            var player_info = await playerHelper.GetPlayInfo(CurrentPlayItem.avid, CurrentPlayItem.cid);
            if (player_info.subtitle != null && player_info.subtitle.subtitles != null && player_info.subtitle.subtitles.Count != 0)
            {
                var menu = new MenuFlyout();
                foreach (var item in player_info.subtitle.subtitles)
                {
                    ToggleMenuFlyoutItem menuitem = new ToggleMenuFlyoutItem() { Text = item.lan_doc, Tag = item.subtitle_url };
                    menuitem.Click += Menuitem_Click;
                    menu.Items.Add(menuitem);
                }
                ToggleMenuFlyoutItem noneItem = new ToggleMenuFlyoutItem() { Text = "无" };
                noneItem.Click += Menuitem_Click;
                menu.Items.Add(noneItem);
                var firstMenuItem = (menu.Items[0] as ToggleMenuFlyoutItem);
                if ((firstMenuItem.Text.Contains("自动") || firstMenuItem.Text.Contains("AI")) && !autoAISubtitle)
                {
                    noneItem.IsChecked = true;
                    CurrentSubtitleName = noneItem.Text;
                }
                else
                {
                    firstMenuItem.IsChecked = true;
                    CurrentSubtitleName = firstMenuItem.Text;
                    SetSubTitle(firstMenuItem.Tag.ToString());
                }

                BottomBtnSelctSubtitle.Flyout = menu;
                BottomBtnSelctSubtitle.Visibility = Visibility.Visible;
                BorderSubtitle.Visibility = Visibility.Collapsed;
            }
            else
            {
                var menu = new MenuFlyout();
                menu.Items.Add(new ToggleMenuFlyoutItem() { Text = "无", IsChecked = true });
                CurrentSubtitleName = "无";
                BottomBtnSelctSubtitle.Flyout = menu;
                BottomBtnSelctSubtitle.Visibility = Visibility.Collapsed;
                BorderSubtitle.Visibility = Visibility.Collapsed;
            }

            if (player_info.interaction != null)
            {
                //设置互动视频
                if (interactionVideoVM == null)
                {
                    interactionVideoVM = new InteractionVideoVM(CurrentPlayItem.avid, player_info.interaction.graph_version);
                    NodeList.DataContext = interactionVideoVM;
                    ShowPlaylistButton = false;
                    await interactionVideoVM.GetNodes();
                    m_viewModel.Questions = interactionVideoVM.Info.edges.questions;
                    TopTitle.Text = interactionVideoVM.Select.title;
                }
            }

            if (player_info.ViewPoints != null && player_info.ViewPoints.Any())
            {
                m_viewModel.ViewPoints = player_info.ViewPoints;
                m_viewModel.ShowViewPointsBtn = true;
                UpdateViewPointPosition();
            }

            TopOnline.Text = await playerHelper.GetOnline(CurrentPlayItem.avid, CurrentPlayItem.cid);
        }

        public async Task ReportHistory(double progress = double.NaN)
        {
            if (!(SettingService.GetValue(SettingConstants.Player.REPORT_HISTORY,
                    SettingConstants.Player.DEFAULT_REPORT_HISTORY)))
                return;
            if (double.IsNaN(progress))
            {
                progress = Player.Position;
            }

            await playerHelper.ReportHistory(CurrentPlayItem, progress);
        }

        BiliPlayUrlInfo current_quality_info = null;
        BiliDashAudioPlayUrlInfo current_audio_quality_info = null;

        private async Task<bool> ChangeQualityGetPlayUrls(BiliPlayUrlInfo quality, BiliDashAudioPlayUrlInfo soundQuality = null)
        {
            if (quality.HasPlayUrl)
            {
                return true;
            }
            var soundQualityId = soundQuality?.QualityID;
            if (soundQualityId == null)
            {
                soundQualityId = 0;
            }
            var info = await playerHelper.GetPlayUrls(CurrentPlayItem, quality.QualityID, soundQualityId.Value);
            if (!info.Success)
            {
                await NotificationShowExtensions.ShowMessageDialog(info.Message, "切换清晰度失败");
                return false;
            }
            if (!info.CurrentQuality.HasPlayUrl)
            {
                await NotificationShowExtensions.ShowMessageDialog("无法读取到播放地址，试试换个清晰度?", "播放失败");
                return false;
            }
            quality = info.CurrentQuality;
            return true;
        }

        private async Task ChangeQualityPlayLocalVideo(BiliDashPlayUrlInfo dashInfo)
        {
            Pause();
            VideoLoading.Visibility = Visibility.Visible;
            var result = await Player.PlayDashUseFFmpegInterop(dashInfo, "", "", positon: _postion,
            isLocal: true);
            if (result.result)
            {
                VideoLoading.Visibility = Visibility.Collapsed;
            }
            else
            {
                NotificationShowExtensions.ShowVideoErrorMessageDialog(result.message + "[LocalFile]");
            }
        }

        private async Task<PlayerOpenResult> ChangeQualityPlayVideo(BiliPlayUrlInfo quality, BiliDashAudioPlayUrlInfo audioQuality)
        {
            var result = new PlayerOpenResult()
            {
                result = false
            };
            if (quality.PlayUrlType == BiliPlayUrlType.DASH)
            {
                var realPlayerType = (RealPlayerType)SettingService.GetValue(SettingConstants.Player.USE_REAL_PLAYER_TYPE, (int)SettingConstants.Player.DEFAULT_USE_REAL_PLAYER_TYPE);
                if (realPlayerType == RealPlayerType.Native)
                {
                    result = await Player.PlayerDashUseNative(quality.DashInfo, quality.UserAgent, quality.Referer, positon: _postion);

                    if (!result.result)
                    {
                        result = await Player.PlayDashUseFFmpegInterop(quality.DashInfo, quality.UserAgent, quality.Referer,
                            positon: _postion);
                    }
                }
                else if (realPlayerType == RealPlayerType.FFmpegInterop)
                {
                    result = await Player.PlayDashUseFFmpegInterop(quality.DashInfo, quality.UserAgent, quality.Referer,
                        positon: _postion);

                    if (!result.result)
                    {
                        result = await Player.PlayerDashUseNative(quality.DashInfo, quality.UserAgent, quality.Referer, positon: _postion);
                    }
                }
                else if (realPlayerType == RealPlayerType.ShakaPlayer)
                {
                    result = await Player.PlayerDashUseShaka(quality, quality.UserAgent, quality.Referer, positon: _postion);
                }
            }
            else if (quality.PlayUrlType == BiliPlayUrlType.SingleFLV)
            {
                result = await Player.PlaySingleFlvUseFFmpegInterop(quality.FlvInfo.First().Url, quality.UserAgent, quality.Referer, positon: _postion);
            }
            else if (quality.PlayUrlType == BiliPlayUrlType.MultiFLV)
            {
                result = await Player.PlayVideoUseSYEngine(quality.FlvInfo, quality.UserAgent, quality.Referer, positon: _postion, epId: CurrentPlayItem.ep_id);
            }
            return result;
        }

        private async Task ChangeQuality(BiliPlayUrlInfo quality, BiliDashAudioPlayUrlInfo soundQuality = null)
        {
            VideoLoading.Visibility = Visibility.Visible;
            if (quality == null)
            {
                return;
            }

            if (soundQuality != null)
            {
                quality.DashInfo.Audio = soundQuality.Audio;
            }
            current_quality_info = quality;
            current_audio_quality_info = soundQuality;
            if (!await ChangeQualityGetPlayUrls(quality, soundQuality))
            {
                return;
            }
            var result = await ChangeQualityPlayVideo(quality, soundQuality);
            if (result.result)
            {
                VideoLoading.Visibility = Visibility.Collapsed;
            }
            else
            {
                NotificationShowExtensions.ShowVideoErrorMessageDialog(result.message + "[ChangeQuality]");
            }
        }

        #region 全屏处理
        public void FullScreen(bool fullScreen)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            FullScreenEvent?.Invoke(this, fullScreen);
            MessageCenter.SetFullscreen(fullScreen);
            m_danmakuController.SetFullscreen(fullScreen);
            if (fullScreen)
            {
                BottomBtnExitFull.Visibility = Visibility.Visible;
                BottomBtnFull.Visibility = Visibility.Collapsed;
                BottomBtnFullWindows.Visibility = Visibility.Collapsed;
                BottomBtnExitFullWindows.Visibility = Visibility.Collapsed;

                //全屏
                if (!view.IsFullScreenMode)
                {
                    view.TryEnterFullScreenMode();
                }
            }
            else
            {
                BottomBtnExitFull.Visibility = Visibility.Collapsed;
                BottomBtnFull.Visibility = Visibility.Visible;
                TopControlBar.Margin = new Thickness(0, 0, 0, 0);
                if (SettingService.GetValue(SettingConstants.UI.DISPLAY_MODE, 0) > 0)
                {
                    TopControlBar.Margin = new Thickness(0, 0, 0, 0);
                }
                if (IsFullWindow)
                {
                    FullWidnow(true);
                    BottomBtnFullWindows.Visibility = Visibility.Collapsed;
                    BottomBtnExitFullWindows.Visibility = Visibility.Visible;
                }
                else
                {
                    BottomBtnFullWindows.Visibility = Visibility.Visible;
                    BottomBtnExitFullWindows.Visibility = Visibility.Collapsed;
                }
                //退出全屏
                if (view.IsFullScreenMode)
                {
                    view.ExitFullScreenMode();
                }
            }
            BtnFoucs.Focus(FocusState.Programmatic);
        }
        public void FullWidnow(bool fullWindow)
        {

            if (fullWindow)
            {
                BottomBtnFullWindows.Visibility = Visibility.Collapsed;
                BottomBtnExitFullWindows.Visibility = Visibility.Visible;
            }
            else
            {
                BottomBtnFullWindows.Visibility = Visibility.Visible;
                BottomBtnExitFullWindows.Visibility = Visibility.Collapsed;
            }
            FullWindowEvent?.Invoke(this, fullWindow);
            Focus(FocusState.Programmatic);
        }
        private void BottomBtnExitFull_Click(object sender, RoutedEventArgs e)
        {
            IsFullScreen = false;
        }

        private void BottomBtnFull_Click(object sender, RoutedEventArgs e)
        {
            IsFullScreen = true;
        }

        private void BottomBtnExitFullWindows_Click(object sender, RoutedEventArgs e)
        {
            IsFullWindow = false;
        }

        private void BottomBtnFullWindows_Click(object sender, RoutedEventArgs e)
        {
            IsFullWindow = true;
        }
        public bool IsFullScreen
        {
            get { return (bool)GetValue(IsFullScreenProperty); }
            set { SetValue(IsFullScreenProperty, value); }
        }
        public static readonly DependencyProperty IsFullScreenProperty =
            DependencyProperty.Register("IsFullScreen", typeof(bool), typeof(PlayerControl), new PropertyMetadata(false, OnIsFullScreenChanged));
        private static void OnIsFullScreenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as PlayerControl;
            sender.FullScreen((bool)e.NewValue);
        }
        public bool IsFullWindow
        {
            get { return (bool)GetValue(IsFullWindowProperty); }
            set { SetValue(IsFullWindowProperty, value); }
        }
        public static readonly DependencyProperty IsFullWindowProperty =
            DependencyProperty.Register("IsFullWindow", typeof(bool), typeof(PlayerControl), new PropertyMetadata(false, OnIsFullWidnowChanged));
        private static void OnIsFullWidnowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as PlayerControl;
            sender.FullWidnow((bool)e.NewValue);
        }

        #endregion


        public bool ShowPlaylistButton
        {
            get { return (bool)GetValue(ShowPlaylistButtonProperty); }
            set { SetValue(ShowPlaylistButtonProperty, value); }
        }
        public static readonly DependencyProperty ShowPlaylistButtonProperty =
            DependencyProperty.Register("ShowPlaylistButton", typeof(bool), typeof(PlayerControl), new PropertyMetadata(false));


        public bool ShowPlayNodeButton
        {
            get { return (bool)GetValue(ShowPlayNodeButtonProperty); }
            set { SetValue(ShowPlayNodeButtonProperty, value); }
        }
        public static readonly DependencyProperty ShowPlayNodeButtonProperty =
            DependencyProperty.Register("ShowPlayNodeButton", typeof(bool), typeof(PlayerControl), new PropertyMetadata(false));


        private bool _buffering = false;
        public bool Buffering
        {
            get { return _buffering; }
            set { _buffering = value; DoPropertyChanged("_Buffering"); }
        }
        private double _BufferingProgress;
        public double BufferingProgress
        {
            get { return _BufferingProgress; }
            set { _BufferingProgress = value; DoPropertyChanged("BufferingProgress"); }
        }


        private void TopBtnOpenDanmaku_Click(object sender, RoutedEventArgs e)
        {
            m_danmakuController.Show();
            SettingService.SetValue<Visibility>(SettingConstants.VideoDanmaku.SHOW, Visibility.Visible);
        }

        private void TopBtnCloseDanmaku_Click(object sender, RoutedEventArgs e)
        {
            m_danmakuController.Hide();
            SettingService.SetValue<Visibility>(SettingConstants.VideoDanmaku.SHOW, Visibility.Collapsed);
        }
        #region 播放器手势
        int showControlsFlag = 0;
        bool HandlingGesture = false;
        bool HandlingHolding = false;
        bool DirectionX = false;
        bool DirectionY = false;

        bool tapFlag;
        double ssValue = 0;
        bool ManipulatingBrightness = false;
        double _brightness = 0;
        private bool lockBrightness = true;
        PlayerHoldingAction m_playerHoldingAction;
        double Brightness
        {
            get => _brightness;
            set
            {
                _brightness = value;
                BrightnessShield.Opacity = value;
                if (!lockBrightness)
                    SettingService.SetValue<double>(SettingConstants.Player.PLAYER_BRIGHTNESS, _brightness);
            }
        }
        private void InitializeGesture()
        {
            m_playerHoldingAction = (PlayerHoldingAction)SettingService.GetValue(SettingConstants.Player.HOLDING_GESTURE_ACTION, (int)PlayerHoldingAction.None);
            gestureRecognizer.GestureSettings = GestureSettings.Hold | GestureSettings.HoldWithMouse | GestureSettings.ManipulationTranslateX | GestureSettings.ManipulationTranslateY;

            gestureRecognizer.Holding += OnHolding;
            gestureRecognizer.ManipulationStarted += OnManipulationStarted;
            gestureRecognizer.ManipulationUpdated += OnManipulationUpdated;
            gestureRecognizer.ManipulationCompleted += OnManipulationCompleted;
        }

        private void OnHolding(GestureRecognizer sender, HoldingEventArgs args)
        {
            if (Player.PlayState != PlayState.Playing || m_playerHoldingAction == PlayerHoldingAction.None)
                return;

            switch (args.HoldingState)
            {
                case HoldingState.Started:
                    {
                        StartHolding();
                        break;
                    }
                case HoldingState.Completed:
                    {
                        StopHolding();
                        break;
                    }
                case HoldingState.Canceled:
                    {
                        var canCancel = SettingService.GetValue(SettingConstants.Player.HOLDING_GESTURE_CAN_CANCEL, true);
                        if (!canCancel) break;
                        StopHolding();
                        break;
                    }
            }
        }

        private void StartHolding()
        {
            HandlingHolding = true;
            StartHighRateSpeedPlay();
        }

        private void StopHolding()
        {
            HandlingHolding = false;
            StopHighRateSpeedPlay();
        }

        public void StartHighRateSpeedPlay()
        {
            m_playerToastService.KeepStart(PlayerToastService.ACCELERATING_KEY, "倍速播放中");
            var highRatePlaySpeed = SettingService.GetValue(SettingConstants.Player.HIGH_RATE_PLAY_SPEED, 2.0d);
            Player.SetRate(highRatePlaySpeed);
        }

        public void StopHighRateSpeedPlay()
        {
            m_playerToastService.KeepClose(PlayerToastService.ACCELERATING_KEY);
            Player.SetRate(SettingService.GetValue<double>(SettingConstants.Player.DEFAULT_VIDEO_SPEED, 1.0d));
        }

        private void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            ssValue = 0;

            if (e.Position.X < ActualWidth / 2)
                ManipulatingBrightness = true;
            else
                ManipulatingBrightness = false;

        }

        private void OnManipulationUpdated(object sender, ManipulationUpdatedEventArgs e)
        {
            var x = e.Delta.Translation.X;
            var y = e.Delta.Translation.Y;

            if (HandlingHolding)
                return;
            if (HandlingGesture == false)
            {
                if (Math.Abs(x) > Math.Abs(y))
                {
                    HandlingGesture = true;
                    DirectionX = true;

                    HandleSlideProgressDelta(e.Delta.Translation.X);
                }
                else
                {
                    HandlingGesture = true;
                    DirectionY = true;

                    if (ManipulatingBrightness)
                        HandleSlideBrightnessDelta(e.Delta.Translation.Y);
                    else
                        HandleSlideVolumeDelta(e.Delta.Translation.Y);
                }
            }
            else
            {
                if (DirectionX)
                {
                    HandleSlideProgressDelta(x);
                }
                if (DirectionY)
                {
                    if (ManipulatingBrightness)
                        HandleSlideBrightnessDelta(y);
                    else
                        HandleSlideVolumeDelta(y);
                }
            }

        }

        private void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            if (HandlingHolding)
            {
                StopHolding();
            }
            HandlingGesture = false;
            DirectionX = false;
            DirectionY = false;
            if (ssValue != 0)
            {
                SetPosition(Player.Position + ssValue);
            }
        }

        private void Grid_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            try
            {
                var par = e.GetCurrentPoint(sender as Frame).Properties.PointerUpdateKind;
                if (SettingService.GetValue(SettingConstants.UI.MOUSE_MIDDLE_ACTION, (int)MouseMiddleActions.Back) == (int)MouseMiddleActions.Back
                && par == Windows.UI.Input.PointerUpdateKind.XButton1Pressed || par == Windows.UI.Input.PointerUpdateKind.MiddleButtonPressed)
                {
                    Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
                    MessageCenter.GoBack(this);
                    return;
                }
                var ps = e.GetIntermediatePoints(null);
                if (ps != null && ps.Count > 0 && HandlingGesture != true)
                {
                    gestureRecognizer.ProcessDownEvent(ps[0]);
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                // 重复获取鼠标指针导致异常
            }
        }

        private void Grid_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var ps = e.GetIntermediatePoints(null);
            if (ps != null && ps.Count > 0)
            {
                gestureRecognizer.ProcessUpEvent(ps[0]);
                e.Handled = true;
                gestureRecognizer.CompleteGesture();
            }
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            //ShowControl(true);
            ////FadeIn.Begin();
            ////control.Visibility = Visibility.Visible;
            pointer_in_player = true;
            //showControlsFlag = 0;
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            //showControlsFlag = 3;
            pointer_in_player = false;
            Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
        }

        private void Grid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            //showControlsFlag = 0;
            //ShowControl(true);
            ////control.Visibility = Visibility.Visible;
            if (Window.Current.CoreWindow.PointerCursor == null)
            {
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Arrow, 0);
            }
            gestureRecognizer.ProcessMoveEvents(e.GetIntermediatePoints(null));
            e.Handled = true;
        }
        private async void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            tapFlag = true;
            await Task.Delay(200);
            //if (control.Visibility == Visibility.Visible)
            //{
            //    if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse&& !Player.Opening)
            //    {
            //        if (Player.PlayState == PlayState.Pause || Player.PlayState == PlayState.End)
            //        {
            //            Player.Play();
            //        }
            //        else if (Player.PlayState == PlayState.Playing)
            //        {
            //            Pause();
            //        }
            //    }

            //}
            if (!tapFlag) return;
            ShowControl(control.Visibility == Visibility.Collapsed);
        }
        private async void Grid_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            tapFlag = false;
            var fullScreen = SettingService.GetValue<bool>(SettingConstants.Player.DOUBLE_CLICK_FULL_SCREEN, false);
            if (!fullScreen)
            {
                if (Player.PlayState == PlayState.Pause || Player.PlayState == PlayState.End)
                {
                    await Play();
                }
                else if (Player.PlayState == PlayState.Playing)
                {
                    Pause();
                }
            }
            else
            {
                IsFullScreen = !IsFullScreen;
            }
        }
        private void HandleSlideProgressDelta(double delta)
        {
            if (Player.PlayState != PlayState.Playing && Player.PlayState != PlayState.Pause)
                return;

            if (delta > 0)
            {
                double dd = delta / ActualWidth;
                double d = dd * 90;
                ssValue += d;
                //slider.Value += d;
            }
            else
            {
                double dd = Math.Abs(delta) / ActualWidth;
                double d = dd * 90;
                ssValue -= d;
                //slider.Value -= d;
            }
            var pos = Player.Position;
            pos += ssValue;

            if (pos < 0)
                pos = 0;
            else if (pos > Player.Duration)
                pos = Player.Duration;
            //txt_Post.Text = ts.Hours.ToString("00") + ":" + ts.Minutes.ToString("00") + ":" + ts.Seconds.ToString("00") + "/" + mediaElement.MediaPlayer.PlaybackSession.NaturalDuration.TimeSpan.Hours.ToString("00") + ":" + mediaElement.MediaPlayer.PlaybackSession.NaturalDuration.TimeSpan.Minutes.ToString("00") + ":" + mediaElement.MediaPlayer.PlaybackSession.NaturalDuration.TimeSpan.Seconds.ToString("00");

            m_playerToastService.Show(PlayerToastService.PROGRESS_KEY, TimeSpan.FromSeconds(pos).ToString(@"hh\:mm\:ss"));
        }

        private void HandleSlideVolumeDelta(double delta)
        {
            if (delta > 0)
            {
                double dd = delta / (ActualHeight * 0.8);

                //slider_V.Value -= d;
                var volume = Player.Volume - dd;
                Player.Volume = volume;

            }
            else
            {
                double dd = Math.Abs(delta) / (ActualHeight * 0.8);
                var volume = Player.Volume + dd;
                Player.Volume = volume;
                //slider_V.Value += d;
            }
            m_playerToastService.Show(PlayerToastService.VOLUME_KEY, "音量:" + Player.Volume.ToString("P"));
        }
        private void HandleSlideBrightnessDelta(double delta)
        {
            double dd = Math.Abs(delta) / (ActualHeight * 0.8);
            if (delta > 0)
            {
                Brightness = Math.Min(Brightness + dd, 1);
            }
            else
            {
                Brightness = Math.Max(Brightness - dd, 0);
            }
            m_playerToastService.Show(PlayerToastService.BRIGHTNESS_KEY, "亮度:" + Math.Abs(Brightness - 1).ToString("P"));
        }
        #endregion
        private void BottomBtnList_Click(object sender, RoutedEventArgs e)
        {
            NodeList.Visibility = Visibility.Collapsed;
            EpisodeList.Visibility = Visibility.Visible;
            SettingPivot.SelectedIndex = 0;
            SplitView.IsPaneOpen = true;

        }
        private void BottomBtnNode_Click(object sender, RoutedEventArgs e)
        {
            NodeList.Visibility = Visibility.Visible;
            EpisodeList.Visibility = Visibility.Collapsed;
            SettingPivot.SelectedIndex = 0;
            SplitView.IsPaneOpen = true;
        }



        private void TopBtnSettingDanmaku_Click(object sender, RoutedEventArgs e)
        {
            SettingPivot.SelectedIndex = 1;
            SplitView.IsPaneOpen = true;
        }

        private void TopBtnMore_Click(object sender, RoutedEventArgs e)
        {
            SettingPivot.SelectedIndex = 2;
            SplitView.IsPaneOpen = true;
        }

        private void BottomBtnPause_Click(object sender, RoutedEventArgs e)
        {
            if (Player.Opening)
            {
                return;
            }
            Pause();
        }

        private async void BottomBtnPlay_Click(object sender, RoutedEventArgs e)
        {
            if (Player.Opening)
            {
                return;
            }
            if (Player.PlayState == PlayState.Pause || Player.PlayState == PlayState.End)
            {
                await Play();
                m_danmakuController.Resume();
            }
        }



        private void BottomBtnNext_Click(object sender, RoutedEventArgs e)
        {
            EpisodeList.SelectedIndex = EpisodeList.SelectedIndex + 1;
        }

        private void KeepScreenOn(bool value = true)
        {
            try
            {
                if (dispRequest != null)
                {
                    if (value)
                    {
                        dispRequest.RequestActive();
                    }
                    else
                    {
                        dispRequest.RequestRelease();
                    }
                }
            }
            catch (Exception ex)
            {
                //Debug.WriteLine(ex.Message);
                //throws an error but it works;
                //A method was called at an unexpected time
            }
        }

        private void Player_PlayStateChanged(object sender, PlayState e)
        {
            BottomImageBtnPlay.Visibility = Visibility.Collapsed;
            switch (e)
            {
                case PlayState.Loading:
                    KeepScreenOn(false);
                    if (_systemMediaTransportControls != null)
                    {
                        _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Changing;
                    }
                    BottomBtnLoading.Visibility = Visibility.Visible;
                    BottomBtnPlay.Visibility = Visibility.Collapsed;
                    BottomBtnPause.Visibility = Visibility.Collapsed;
                    // 更新画面比例
                    Player.SetRatioMode(PlayerSettingRatio.SelectedIndex);
                    break;
                case PlayState.Playing:
                    KeepScreenOn(true);
                    if (_systemMediaTransportControls != null)
                    {
                        _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                    }
                    BottomBtnLoading.Visibility = Visibility.Collapsed;
                    BottomBtnPlay.Visibility = Visibility.Collapsed;
                    BottomBtnPause.Visibility = Visibility.Visible;
                    m_danmakuController.Resume();
                    break;
                case PlayState.Pause:
                    KeepScreenOn(false);
                    if (_systemMediaTransportControls != null)
                    {
                        _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                    }
                    BottomImageBtnPlay.Visibility = Visibility.Visible;
                    BottomBtnLoading.Visibility = Visibility.Collapsed;
                    BottomBtnPlay.Visibility = Visibility.Visible;
                    BottomBtnPause.Visibility = Visibility.Collapsed;
                    m_danmakuController.Pause();
                    break;
                case PlayState.End:
                    KeepScreenOn(false);
                    if (_systemMediaTransportControls != null)
                    {
                        _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
                    }
                    BottomBtnLoading.Visibility = Visibility.Collapsed;
                    BottomBtnPlay.Visibility = Visibility.Visible;
                    BottomBtnPause.Visibility = Visibility.Collapsed;
                    break;
                case PlayState.Error:
                    KeepScreenOn(false);
                    if (_systemMediaTransportControls != null)
                    {
                        _systemMediaTransportControls.PlaybackStatus = MediaPlaybackStatus.Closed;
                    }
                    BottomBtnLoading.Visibility = Visibility.Visible;
                    BottomBtnPlay.Visibility = Visibility.Collapsed;
                    BottomBtnPause.Visibility = Visibility.Collapsed;
                    break;
                default:
                    break;
            }
        }

        private void Player_PlayBufferStart(object sender, EventArgs e)
        {
            Buffering = true;
            GridBuffering.Visibility = Visibility.Visible;
            TxtBuffering.Text = "正在缓冲...";
            BufferingProgress = 0;
            m_danmakuController.Pause();
        }

        private void Player_PlayBuffering(object sender, double e)
        {
            Buffering = true;
            GridBuffering.Visibility = Visibility.Visible;
            TxtBuffering.Text = "正在缓冲" + e.ToString("p");
            BufferingProgress = e;
        }

        private void Player_PlayBufferEnd(object sender, EventArgs e)
        {
            GridBuffering.Visibility = Visibility.Collapsed;
            Buffering = false;
            m_danmakuController.Resume();
        }

        private async void Player_PlayMediaEnded(object sender, EventArgs e)
        {
            if (CurrentPlayItem.is_interaction)
            {
                if (interactionVideoVM.Info.is_leaf == 1)
                {
                    NotificationShowExtensions.ShowMessageToast("播放完毕，请点击右下角节点，重新开始");
                    return;
                }
                m_danmakuController.Pause();
                InteractionChoices.Visibility = Visibility.Visible;
                return;
            }
            _logger.Debug("视频结束，上报进度");
            Pause();

            await ReportHistory(Player.Duration);

            if (SettingService.GetValue(SettingConstants.Player.REPORT_HISTORY_ZERO_WHEN_VIDEO_END,
                    SettingConstants.Player.DEFAULT_REPORT_HISTORY_ZERO_WHEN_VIDEO_END))
            {
                _logger.Debug("进度归0");
                await ReportHistory(0);
            }
            // 播完停止
            if (PlayerSettingPlayMode.SelectedIndex == 3)
            {
                return;
            }
            //列表顺序播放
            if (PlayerSettingPlayMode.SelectedIndex == 0)
            {
                if (CurrentPlayIndex == PlayInfos.Count - 1)
                {
                    if (AllMediaEndEvent != null)
                    {
                        AllMediaEndEvent?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        NotificationShowExtensions.ShowMessageToast("播放完毕");
                    }

                }
                else
                {
                    if (PlayerSettingAutoNext.IsOn)
                    {
                        _autoPlay = true;
                        ChangePlayIndex(CurrentPlayIndex + 1);
                    }
                    else
                    {
                        NotificationShowExtensions.ShowMessageToast("本P播放完成");
                    }

                }
                return;
            }
            //单P循环
            if (PlayerSettingPlayMode.SelectedIndex == 1)
            {
                ClearSubTitle();
                m_danmakuController.Clear();
                await Play();
                return;
            }
            //列表循环播放
            if (PlayerSettingPlayMode.SelectedIndex == 2)
            {
                if (!PlayerSettingAutoNext.IsOn)
                {
                    NotificationShowExtensions.ShowMessageToast("本P播放完成");
                    return;
                }
                //只有一P,重新播放
                if (PlayInfos.Count == 1)
                {
                    ClearSubTitle();
                    m_danmakuController.Clear();
                    await Play();
                    return;
                }
                _autoPlay = true;
                if (CurrentPlayIndex == PlayInfos.Count - 1)
                {
                    ChangePlayIndex(0);
                }
                else
                {
                    ChangePlayIndex(CurrentPlayIndex + 1);
                }
                return;
            }


        }
        private async void Player_PlayMediaError(object sender, string e)
        {
            _logger.Error($"播放失败:{e}");
            await NotificationShowExtensions.ShowMessageDialog(e, "播放失败");
        }

        private async void DanmuSettingUpdateDanmaku_Click(object sender, RoutedEventArgs e)
        {
            await SetDanmaku(true);
        }

        private async void TopBtnScreenshot_Click(object sender, RoutedEventArgs e)
        {
            await CaptureVideoCore();
        }

        private async Task CaptureVideoCore()
        {
            try
            {
                string fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + ".jpg";
                StorageFolder applicationFolder = KnownFolders.PicturesLibrary;
                StorageFolder folder = await applicationFolder.CreateFolderAsync("哔哩哔哩截图", CreationCollisionOption.OpenIfExists);
                StorageFile saveFile = await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);

                if (Player.ShowShakaPlayer)
                {
                    var imageData = await Player.WebPlayer.CaptureVideo();
                    await FileIO.WriteBytesAsync(saveFile, imageData);
                    NotificationShowExtensions.ShowMessageToast("截图已经保存至图片库");
                    return;
                }

                RenderTargetBitmap bitmap = new RenderTargetBitmap();
                await bitmap.RenderAsync(Player);
                var pixelBuffer = await bitmap.GetPixelsAsync();
                using (var fileStream = await saveFile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, fileStream);
                    encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Ignore,
                         (uint)bitmap.PixelWidth,
                         (uint)bitmap.PixelHeight,
                         displayInformation.LogicalDpi,
                         displayInformation.LogicalDpi,
                         pixelBuffer.ToArray());
                    await encoder.FlushAsync();
                }
                NotificationShowExtensions.ShowMessageToast("截图已经保存至图片库");
            }
            catch (Exception ex)
            {
                _logger.Error("截图失败", ex);
                NotificationShowExtensions.ShowMessageToast("截图失败");
            }
        }

        private async void Player_ChangeEngine(object sender, ChangePlayerEngine e)
        {
            if (!e.need_change)
            {
                _logger.Warn($"[ChangeEngine] {e.message}");
                return;
            }
            VideoLoading.Visibility = Visibility.Visible;
            PlayerOpenResult result = new PlayerOpenResult()
            {
                result = false,
                message = ""
            };
            if (e.play_type == PlayMediaType.Dash && e.change_engine == PlayEngine.FFmpegInteropMSS)
            {
                result = await Player.PlayDashUseFFmpegInterop(current_quality_info.DashInfo, current_quality_info.UserAgent, current_quality_info.Referer, positon: _postion);
            }
            if (e.play_type == PlayMediaType.Single && e.change_engine == PlayEngine.SYEngine)
            {
                result = await Player.PlaySingleFlvUseSYEngine(current_quality_info.FlvInfo.First().Url, current_quality_info.UserAgent, current_quality_info.Referer, positon: _postion);
            }
            if (!result.result)
            {
                _logger.Error($"播放失败:{result.message}");
                await NotificationShowExtensions.ShowMessageDialog(result.message, "播放失败");
                return;
            }

        }

        private async void Player_PlayMediaOpened(object sender, EventArgs e)
        {
            txtInfo.Text = Player.GetMediaInfo();
            VideoLoading.Visibility = Visibility.Collapsed;
            if (_postion != 0 && _postion < Player.Duration)
            {
                Player.SetPosition(_postion);
            }

            if (_autoPlay)
            {
                await Play();
            }
            m_firstMediaOpened = true;
            m_autoRefreshTimer?.Start();
        }

        private async void BottomBtnSendDanmakuWide_Click(object sender, RoutedEventArgs e)
        {
            Pause();
            SendDanmakuDialog sendDanmakuDialog = new SendDanmakuDialog(CurrentPlayItem.avid, CurrentPlayItem.cid, Player.Position);
            sendDanmakuDialog.DanmakuSended += new EventHandler<SendDanmakuModel>((obj, arg) =>
            {
                m_danmakuController.Add(new BiliDanmakuItem()
                {
                    Color = NSDanmaku.Utils.ToColor(arg.color),
                    Text = arg.text,
                    Location = (DanmakuLocation)arg.location,
                    Size = 25,
                    Time = Player.Position
                }, true);
            });
            await NotificationShowExtensions.ShowContentDialog(sendDanmakuDialog);
            await Play();
        }

        private async void DanmuSettingSyncWords_Click(object sender, RoutedEventArgs e)
        {
            await m_danmakuSettingsControlViewModel.SyncDanmuFilter();
        }

        private async void DanmuSettingAddWord_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(DanmuSettingTxtWord.Text))
            {
                NotificationShowExtensions.ShowMessageToast("关键词不能为空");
                return;
            }
            m_danmakuSettingsControlViewModel.ShieldWords.Add(DanmuSettingTxtWord.Text);
            SettingService.SetValue(SettingConstants.VideoDanmaku.SHIELD_WORD, m_danmakuSettingsControlViewModel.ShieldWords);
            var result = await m_danmakuSettingsControlViewModel.AddDanmuFilterItem(DanmuSettingTxtWord.Text, 0);
            DanmuSettingTxtWord.Text = "";
            if (!result)
            {
                NotificationShowExtensions.ShowMessageToast("已经添加到本地，但远程同步失败");
            }
        }

        public void SetPosition(double position)
        {
            Player.SetPosition(position);
        }

        public void ToggleSubtitle()
        {
            if (BottomBtnSelctSubtitle.Flyout is not MenuFlyout subtitleMenu) return;

            var targetItem = subtitleMenu.Items.OfType<ToggleMenuFlyoutItem>().FirstOrDefault(item =>
                (CurrentSubtitleName == "无" && item.Text != "无") ||
                (CurrentSubtitleName != "无" && item.Text == "无")
            );

            if (targetItem != null)
                Menuitem_Click(targetItem, null);
        }

        public void ToggleVideoEnable()
        {
            SwitchVideoEnable.IsOn = !SwitchVideoEnable.IsOn;
        }

        public async void Dispose()
        {
            _logger.Trace("Dispose PlayerControl");
            if (CurrentPlayItem != null)
            {
                SettingService.SetValue<double>(CurrentPlayItem.season_id != 0 ? "ep" + CurrentPlayItem.ep_id : CurrentPlayItem.cid, Player.Position);
                //当视频播放结束的话，Position为0
                if (Player.PlayState != PlayState.End)
                    await ReportHistory(Player.Position);
            }

            Player.PlayStateChanged -= Player_PlayStateChanged;
            Player.PlayMediaEnded -= Player_PlayMediaEnded;
            Player.PlayMediaError -= Player_PlayMediaError;
            Player.ChangeEngine -= Player_ChangeEngine;
            //Player.PlayBufferEnd -= Player_PlayBufferEnd;
            //Player.PlayBufferStart -= Player_PlayBufferStart;
            //Player.PlayBuffering -= Player_PlayBuffering;
            Player.Dispose();
            if (danmuTimer != null)
            {
                danmuTimer.Stop();
                danmuTimer = null;
            }
            if (m_positionTimer != null)
            {
                m_positionTimer.Stop();
                m_positionTimer = null;
            }

            if (m_autoRefreshTimer != null)
            {
                m_autoRefreshTimer.Stop();
                m_autoRefreshTimer = null;
            }
            danmakuPool = null;
            if (dispRequest != null)
            {
                dispRequest = null;
            }
        }


        private void GridViewSelectColor_ItemClick(object sender, ItemClickEventArgs e)
        {
            SendDanmakuColorText.Text = e.ClickedItem.ToString();
        }

        private void SendDanmakuTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SendDanmaku();
            }
        }

        private void SendDanmakuButton_Click(object sender, RoutedEventArgs e)
        {
            SendDanmaku();
        }
        private async void SendDanmaku()
        {
            int modeInt = 1;
            var location = DanmakuLocation.Scroll;
            if (SendDanmakuMode.SelectedIndex == 2)
            {
                modeInt = 4;
                location = DanmakuLocation.Bottom;
            }
            if (SendDanmakuMode.SelectedIndex == 1)
            {
                modeInt = 5;
                location = DanmakuLocation.Top;
            }
            var color = "16777215";
            if (SendDanmakuColorBorder.Background != null)
            {
                color = System.Convert.ToInt32((SendDanmakuColorBorder.Background as SolidColorBrush).Color.ToString().Replace("#FF", ""), 16).ToString();
            }

            var result = await playerHelper.SendDanmaku(CurrentPlayItem.avid, CurrentPlayItem.cid, SendDanmakuTextBox.Text, System.Convert.ToInt32(Player.Position), modeInt, color);
            if (result)
            {
                m_danmakuController.Add(new BiliDanmakuItem()
                {
                    Color = NSDanmaku.Utils.ToColor(color),
                    Text = SendDanmakuTextBox.Text,
                    Location = location,
                    Size = 25,
                    Time = Player.Position
                }, true);
                SendDanmakuTextBox.Text = "";
            }

        }

        bool miniWin = false;
        private void BottomBtnExitMiniWindows_Click(object sender, RoutedEventArgs e)
        {

            MiniWidnows(false);
        }

        private void BottomBtnMiniWindows_Click(object sender, RoutedEventArgs e)
        {
            MiniWidnows(true);
        }

        public async void MiniWidnows(bool mini)
        {
            miniWin = mini;
            ApplicationView view = ApplicationView.GetForCurrentView();
            FullWindowEvent?.Invoke(this, IsFullWindow);
            if (mini)
            {
                IsFullWindow = true;
                StandardControl.Visibility = Visibility.Collapsed;
                MiniControl.Visibility = Visibility.Visible;
                //处理CC字幕
                if (ApplicationView.GetForCurrentView().IsViewModeSupported(ApplicationViewMode.CompactOverlay))
                {
                    Margin = new Thickness(0, -40, 0, 0);
                    await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay);
                    SubtitleSettingSize.Value = 14;

                    m_danmakuController.SetFontZoom(0.5);
                    m_danmakuController.SetSpeed(6);
                    m_danmakuController.Clear();
                }
            }
            else
            {
                Margin = new Thickness(0, 0, 0, 0);
                StandardControl.Visibility = Visibility.Visible;
                MiniControl.Visibility = Visibility.Collapsed;
                await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);

                m_danmakuController.SetFontZoom(SettingService.GetValue<double>(SettingConstants.VideoDanmaku.FONT_ZOOM, 1));
                m_danmakuController.SetSpeed(SettingService.GetValue<int>(SettingConstants.VideoDanmaku.SPEED, 10));
                m_danmakuController.Clear();
                if (SettingService.GetValue<Visibility>(SettingConstants.VideoDanmaku.SHOW, Visibility.Visible) == Visibility.Visible)
                {
                    m_danmakuController.Show();
                }
                else
                {
                    m_danmakuController.Hide();
                }

                SubtitleSettingSize.Value = SettingService.GetValue<double>(SettingConstants.Player.SUBTITLE_SIZE, 40);
            }
            BtnFoucs.Focus(FocusState.Programmatic);
            MessageCenter.SetMiniWindow(mini);
        }

        public void ToggleMiniWindows()
        {
            MiniWidnows(!miniWin);
        }

        public void ToggleFullWindow()
        {
            IsFullWindow = !IsFullWindow;
        }

        public void ToggleFullscreen()
        {
            IsFullScreen = !IsFullScreen;
        }

        public async Task CaptureVideo()
        {
            await CaptureVideoCore();
        }

        public void GotoLastVideo()
        {
            if (EpisodeList.SelectedIndex == 0)
            {
                NotificationShowExtensions.ShowMessageToast("已经是第一P了");
            }
            else
            {
                EpisodeList.SelectedIndex = EpisodeList.SelectedIndex - 1;
            }
        }

        public void GotoNextVideo()
        {
            if (EpisodeList.SelectedIndex == EpisodeList.Items.Count - 1)
            {
                NotificationShowExtensions.ShowMessageToast("已经是最后一P了");
            }
            else
            {
                EpisodeList.SelectedIndex = EpisodeList.SelectedIndex + 1;
            }
        }

        public void ToggleMute()
        {
            if (Player.Volume >= 0)
            {
                Player.Volume = 0;
            }
            else
            {
                Player.Volume = 1;
            }
        }

        public void OpenDevMode()
        {
            Player.OpenDevMode();
        }

        public void ToggleDanmakuDisplay()
        {
            if (!m_danmakuController.DanmakuViewModel.IsHide)
            {
                m_danmakuController.Hide();
            }
            else
            {
                m_danmakuController.Show();
            }
        }

        public async Task Play()
        {
            // 超过一定时间后继续播放，检查播放地址是否仍然有效
            if (DateTime.Now - m_startTime > TimeSpan.FromMinutes(30))
            {
                if (!await Player.CheckPlayUrl())
                {
                    _postion = Player.Position;
                    var info = await GetPlayUrlQualitesInfo();
                    if (!info.Success)
                    {
                        await NotificationShowExtensions.ShowMessageDialog($"请求信息:\r\n{info.Message}", "读取视频播放地址失败");
                    }
                    else
                    {
                        playUrlInfo = info;
                        InitSoundQuality();
                        InitQuality();
                    }
                    NotificationShowExtensions.ShowMessageToast("检测到视频地址失效，已自动刷新");
                    m_startTime = DateTime.Now;
                    return;
                }
            }

            // 播放结束后再次播放应从进度0开始
            if (Player.PlayState == PlayState.End)
            {
                SetPosition(0);
            }
            Player.Play();
        }

        public void Pause()
        {
            m_danmakuController.Pause();
            Player.Pause();
        }

        public void PositionBack(double progress = 3)
        {
            if (Player.PlayState == PlayState.Playing || Player.PlayState == PlayState.Pause)
            {
                var _position = Player.Position - progress;
                if (_position < 0)
                {
                    _position = 0;
                }

                SetPosition(_position);

                m_playerToastService.Show(
                    PlayerToastService.PROGRESS_KEY,
                    "进度:" + TimeSpan.FromSeconds(Player.Position).ToString(@"hh\:mm\:ss"));
            }
        }

        public void PositionForward(double progress = 3)
        {
            if (Player.PlayState == PlayState.Playing || Player.PlayState == PlayState.Pause)
            {
                var _position = Player.Position + progress;
                if (_position > Player.Duration)
                {
                    _position = Player.Duration;
                }
                SetPosition(_position);

                m_playerToastService.Show(
                    PlayerToastService.PROGRESS_KEY, "进度:" + TimeSpan.FromSeconds(Player.Position).ToString(@"hh\:mm\:ss"));
            }
        }

        public void AddVolume()
        {
            Player.Volume += 0.1;
            m_playerToastService.Show(PlayerToastService.VOLUME_KEY, "音量:" + Player.Volume.ToString("P"));
        }

        public void MinusVolume()
        {
            Player.Volume -= 0.1;
            var txtToolTipText = "静音";
            if (Player.Volume > 0)
            {
                txtToolTipText = "音量:" + Player.Volume.ToString("P");
            }
            m_playerToastService.Show(PlayerToastService.VOLUME_KEY, txtToolTipText);
        }

        public void CancelFullscreen()
        {
            IsFullScreen = false;
        }

        public void PlayerSettingABPlaySetPointA_Click(object sender, RoutedEventArgs e)
        {
            if (Player.ABPlay != null)
            {
                Player.ABPlay = null;
                VideoPlayHistoryHelper.SetABPlayHistory(CurrentPlayItem, null);
                PlayerSettingABPlaySetPointA.Content = "设置A点";
                PlayerSettingABPlaySetPointB.Content = "设置B点";
                PlayerSettingABPlaySetPointB.Visibility = Visibility.Collapsed;

                NotificationShowExtensions.ShowMessageToast("已取消设置A点");
            }
            else
            {
                Player.ABPlay = new VideoPlayHistoryHelper.ABPlayHistoryEntry()
                {
                    PointA = Player.Position
                };
                PlayerSettingABPlaySetPointA.Content = "A: " + TimeSpan.FromSeconds(Player.ABPlay.PointA).ToString(@"hh\:mm\:ss\.fff");
                PlayerSettingABPlaySetPointB.Visibility = Visibility.Visible;

                NotificationShowExtensions.ShowMessageToast("已设置A点, 再次点击可取消设置");
            }
        }

        public void PlayerSettingABPlaySetPointB_Click(object sender, RoutedEventArgs e)
        {
            if (Player.ABPlay.PointB > 0 && Player.ABPlay.PointB != Double.MaxValue)
            {
                Player.ABPlay.PointB = double.MaxValue;
                PlayerSettingABPlaySetPointB.Content = "设置B点";

                NotificationShowExtensions.ShowMessageToast("已取消设置B点");
            }
            else
            {
                if (Player.Position <= Player.ABPlay.PointA)
                {
                    NotificationShowExtensions.ShowMessageToast("B点必须在A点之后");
                }
                else
                {
                    Player.ABPlay.PointB = Player.Position;
                    VideoPlayHistoryHelper.SetABPlayHistory(CurrentPlayItem, Player.ABPlay);
                    PlayerSettingABPlaySetPointB.Content = "B: " + TimeSpan.FromSeconds(Player.ABPlay.PointB).ToString(@"hh\:mm\:ss\.fff");

                    NotificationShowExtensions.ShowMessageToast("已设置B点, 再次点击可取消设置");
                }
            }
        }

        private void Player_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // 更新弹幕
            m_danmakuController.UpdateSize(SplitView.ActualWidth, SplitView.ActualHeight);
            // 更新画面比例
            Player.SetRatioMode(PlayerSettingRatio.SelectedIndex);

            UpdateViewPointPosition();
        }

        private void UpdateViewPointPosition()
        {
            if (m_viewModel.ViewPoints == null || m_viewModel.ViewPoints.Count <= 1) return;

            BottomProgressCanvas.Children.Clear();
            var duration = CurrentPlayItem.duration;
            var accentColor = (Color)m_themeService.AccentThemeResource["SystemAccentColor"];
            var brush = new SolidColorBrush(accentColor);

            foreach (var viewPoint in m_viewModel.ViewPoints.Skip(1))
            {
                var x = (((double)viewPoint.From / duration) * BottomProgress.ActualWidth) / 2;
                var line = new Line
                {
                    X1 = x,
                    X2 = x,
                    Y1 = 5,
                    Y2 = 15,
                    Stroke = brush,
                    StrokeThickness = 1,
                };
                BottomProgressCanvas.Children.Add(line);
                Canvas.SetLeft(line, x);
            }
        }

        private void TopBtnViewPoints_OnClick(object sender, RoutedEventArgs e)
        {
            m_viewModel.ShowViewPointsView = true;
        }

        private void ViewPointsGrid_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            m_viewModel.ShowViewPointsView = false;
        }

        private void ViewPoint_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (!(sender is FrameworkElement element)) return;
            if (!(element.DataContext is PlayerInfoViewPoint viewPoint)) return;
            SetPosition(viewPoint.From);
        }

        private void EpisodeList_Loaded(object sender, RoutedEventArgs e) => ScrollToItem();

        private void SplitView_PaneOpening(SplitView sender, object args) => ScrollToItem();

        private void ScrollToItem() => EpisodeList.ScrollIntoView(EpisodeList.SelectedItem);

        private void BottomProgress_OnPositionChanged(object sender, double e)
        {
            SetPosition(e);
        }

        private void BtnOpenWebPlayerToolbar_OnClick(object sender, RoutedEventArgs e)
        {
            WebPlayerToolbarControl.SetPlayer(Player.WebPlayer);
            m_viewModel.ShowWebPlayerToolbar = true;
        }

        private void WebPlayerToolbarControl_OnExitToolbar(object sender, EventArgs e)
        {
            m_viewModel.ShowWebPlayerToolbar = false;
        }

        private void Player_OnStatsUpdated(object sender, EventArgs e)
        {
            txtInfo.Text = Player.GetMediaInfo();
        }
    }
}
