
using BiliLite.Controls.Dialogs;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Player;
using BiliLite.Services;
using BiliLite.ViewModels.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls.Settings
{
    public sealed partial class PlaySettingsControl : UserControl
    {
        private readonly PlaySettingsControlViewModel m_viewModel;
        private readonly PlaySpeedMenuService m_playSpeedMenuService;
        private readonly RealPlayerTypes m_realPlayerTypes = new RealPlayerTypes();
        private readonly RealPlayerTypeOption[] m_livePlayerTypes = LivePlayerTypeOptions.Options;
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        public PlaySettingsControl()
        {
            try
            {
                m_viewModel = App.ServiceProvider.GetRequiredService<PlaySettingsControlViewModel>();
                m_playSpeedMenuService = App.ServiceProvider.GetRequiredService<PlaySpeedMenuService>();
                InitializeComponent();
                LoadPlayer();
            }
            catch (Exception ex)
            {
                _logger.Warn("播放设置加载失败", ex);
            }
        }

        private void LoadPlayer()
        {
            //播放类型
            var selectedValue = (PlayUrlCodecMode)SettingService.GetValue(SettingConstants.Player.DEFAULT_VIDEO_TYPE,
                (int)DefaultVideoTypeOptions.DEFAULT_VIDEO_TYPE);
            cbVideoType.SelectedItem = DefaultVideoTypeOptions.GetOption(selectedValue);
            cbVideoType.SelectionChanged += (e, args) =>
            {
                SettingService.SetValue(SettingConstants.Player.DEFAULT_VIDEO_TYPE, (int)cbVideoType.SelectedValue);
            };

            //优先播放器类型
            var realPlayerType = (RealPlayerType)SettingService.GetValue(SettingConstants.Player.USE_REAL_PLAYER_TYPE,
                (int)SettingConstants.Player.DEFAULT_USE_REAL_PLAYER_TYPE);
            ComboBoxUseRealPlayerType.SelectedItem =
                m_realPlayerTypes.Options.FirstOrDefault(x => x.Value == realPlayerType);
            ComboBoxUseRealPlayerType.SelectionChanged += (e, args) =>
            {
                SettingService.SetValue(SettingConstants.Player.USE_REAL_PLAYER_TYPE,
                    (int)ComboBoxUseRealPlayerType.SelectedValue);
            };

            //直播优先播放器类型
            var m_livePlayerType = (RealPlayerType)SettingService.GetValue(SettingConstants.Player.LIVE_PLAYER_TYPE,
                (int)LivePlayerTypeOptions.DEFAULT_LIVE_PLAYER_MODE);
            cbUseRealPlayerType.SelectedItem =
                m_livePlayerTypes.FirstOrDefault(x => x.Value == m_livePlayerType);
            cbUseRealPlayerType.SelectionChanged += (e, args) =>
            {
                SettingService.SetValue(SettingConstants.Player.LIVE_PLAYER_TYPE,
                    (int)cbUseRealPlayerType.SelectedValue);
            };

            //视频倍速
            var speeds = m_playSpeedMenuService.MenuItems
                .Select(x => x.Value)
                .ToList();
            cbVideoSpeed.SelectedIndex = speeds
                .IndexOf(SettingService.GetValue<double>(SettingConstants.Player.DEFAULT_VIDEO_SPEED, 1.0d));
            cbVideoSpeed.Loaded += (sender, e) =>
            {
                cbVideoSpeed.SelectionChanged += (obj, args) =>
                {
                    if (cbVideoSpeed.SelectedIndex == -1) // 空值初始化
                    {
                        speeds = m_playSpeedMenuService.MenuItems
                            .Select(x => x.Value)
                            .ToList();
                        SettingService.SetValue(SettingConstants.Player.DEFAULT_VIDEO_SPEED, 0);
                        cbVideoSpeed.SelectedIndex = 0;
                        return;
                    }

                    SettingService.SetValue(SettingConstants.Player.DEFAULT_VIDEO_SPEED,
                        speeds[cbVideoSpeed.SelectedIndex]);
                };
            };

            //硬解视频
            //swHardwareDecode.IsOn = SettingService.GetValue<bool>(SettingConstants.Player.HARDWARE_DECODING, true);
            //swHardwareDecode.Loaded += new RoutedEventHandler((sender, e) =>
            //{
            //    swHardwareDecode.Toggled += new RoutedEventHandler((obj, args) =>
            //    {
            //        SettingService.SetValue(SettingConstants.Player.HARDWARE_DECODING, swHardwareDecode.IsOn);
            //    });
            //});

            //自动播放
            swAutoPlay.IsOn = SettingService.GetValue<bool>(SettingConstants.Player.AUTO_PLAY, false);
            swAutoPlay.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swAutoPlay.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.AUTO_PLAY, swAutoPlay.IsOn);
                });
            });
            //自动跳过OP/ED
            SwSkipOpEd.IsOn = SettingService.GetValue(SettingConstants.Player.AUTO_SKIP_OP_ED,
                SettingConstants.Player.DEFAULT_AUTO_SKIP_OP_ED);
            SwSkipOpEd.Loaded += (sender, e) =>
            {
                SwSkipOpEd.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.AUTO_SKIP_OP_ED, SwSkipOpEd.IsOn);
                };
            };
            //自动跳转下一P
            swAutoNext.IsOn = SettingService.GetValue<bool>(SettingConstants.Player.AUTO_NEXT, true);
            swAutoNext.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swAutoNext.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.AUTO_NEXT, swAutoNext.IsOn);
                });
            });
            //使用其他网站
            //swPlayerSettingUseOtherSite.IsOn = SettingService.GetValue<bool>(SettingConstants.Player.USE_OTHER_SITEVIDEO, false);
            //swPlayerSettingUseOtherSite.Loaded += new RoutedEventHandler((sender, e) =>
            //{
            //    swPlayerSettingUseOtherSite.Toggled += new RoutedEventHandler((obj, args) =>
            //    {
            //        SettingService.SetValue(SettingConstants.Player.USE_OTHER_SITEVIDEO, swPlayerSettingUseOtherSite.IsOn);
            //    });
            //});

            //自动跳转进度
            swPlayerSettingAutoToPosition.IsOn =
                SettingService.GetValue<bool>(SettingConstants.Player.AUTO_TO_POSITION, true);
            swPlayerSettingAutoToPosition.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swPlayerSettingAutoToPosition.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.AUTO_TO_POSITION,
                        swPlayerSettingAutoToPosition.IsOn);
                });
            });
            //自动铺满屏幕
            swPlayerSettingAutoFullWindows.IsOn =
                SettingService.GetValue<bool>(SettingConstants.Player.AUTO_FULL_WINDOW, false);
            swPlayerSettingAutoFullWindows.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swPlayerSettingAutoFullWindows.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.AUTO_FULL_WINDOW,
                        swPlayerSettingAutoFullWindows.IsOn);
                });
            });
            //默认最大音质
            SwitchEnableDefaultMaxSoundQuality.IsOn = SettingService.GetValue(
                SettingConstants.Player.ENABLE_DEFAULT_MAX_SOUND_QUALITY,
                SettingConstants.Player.DEFAULT_ENABLE_DEFAULT_MAX_SOUND_QUALITY);
            SwitchEnableDefaultMaxSoundQuality.Loaded += (sender, e) =>
            {
                SwitchEnableDefaultMaxSoundQuality.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.ENABLE_DEFAULT_MAX_SOUND_QUALITY,
                        SwitchEnableDefaultMaxSoundQuality.IsOn);
                };
            };
            //自动全屏
            swPlayerSettingAutoFullScreen.IsOn =
                SettingService.GetValue<bool>(SettingConstants.Player.AUTO_FULL_SCREEN, false);
            swPlayerSettingAutoFullScreen.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swPlayerSettingAutoFullScreen.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.AUTO_FULL_SCREEN,
                        swPlayerSettingAutoFullScreen.IsOn);
                });
            });


            //双击全屏
            swPlayerSettingDoubleClickFullScreen.IsOn =
                SettingService.GetValue<bool>(SettingConstants.Player.DOUBLE_CLICK_FULL_SCREEN, false);
            swPlayerSettingDoubleClickFullScreen.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swPlayerSettingDoubleClickFullScreen.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.DOUBLE_CLICK_FULL_SCREEN,
                        swPlayerSettingDoubleClickFullScreen.IsOn);
                });
            });

            // 支持快捷键自定义后废弃
            //// 方向键右键行为
            //cbPlayerKeyRightAction.SelectedIndex = SettingService.GetValue(SettingConstants.Player.PLAYER_KEY_RIGHT_ACTION, (int)PlayerKeyRightAction.ControlProgress);
            //cbPlayerKeyRightAction.Loaded += (sender, e) =>
            //{
            //    cbPlayerKeyRightAction.SelectionChanged += (obj, args) =>
            //    {
            //        SettingService.SetValue(SettingConstants.Player.PLAYER_KEY_RIGHT_ACTION, cbPlayerKeyRightAction.SelectedIndex);
            //    };
            //};

            // 按住手势行为
            cbPlayerHoldingGestureAction.SelectedIndex =
                SettingService.GetValue(SettingConstants.Player.HOLDING_GESTURE_ACTION, (int)PlayerHoldingAction.None);
            cbPlayerHoldingGestureAction.Loaded += (sender, e) =>
            {
                cbPlayerHoldingGestureAction.SelectionChanged += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.HOLDING_GESTURE_ACTION,
                        cbPlayerHoldingGestureAction.SelectedIndex);
                };
            };

            // 按住手势可被其他手势取消
            swPlayerHoldingGestureCanCancel.IsOn =
                SettingService.GetValue(SettingConstants.Player.HOLDING_GESTURE_CAN_CANCEL, true);
            swPlayerHoldingGestureCanCancel.Loaded += (sender, e) =>
            {
                swPlayerHoldingGestureCanCancel.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.HOLDING_GESTURE_CAN_CANCEL,
                        swPlayerHoldingGestureCanCancel.IsOn);
                };
            };

            // 倍速播放速度
            cbRatePlaySpeed.SelectedIndex =
                SettingConstants.Player.HIGH_RATE_PLAY_SPEED_LIST.IndexOf(
                    SettingService.GetValue(SettingConstants.Player.HIGH_RATE_PLAY_SPEED, 2.0d));
            cbRatePlaySpeed.Loaded += (sender, e) =>
            {
                cbRatePlaySpeed.SelectionChanged += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.HIGH_RATE_PLAY_SPEED,
                        SettingConstants.Player.HIGH_RATE_PLAY_SPEED_LIST[cbRatePlaySpeed.SelectedIndex]);
                };
            };

            // 音量
            SliderVolume.Value = Math.Round((SettingService.GetValue(SettingConstants.Player.PLAYER_VOLUME,
                SettingConstants.Player.DEFAULT_PLAYER_VOLUME)) * 100, 2);
            SliderVolume.Loaded += (sender, e) =>
            {
                SliderVolume.ValueChanged += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.PLAYER_VOLUME, SliderVolume.Value / 100);
                };
            };

            // 锁定播放器音量设置
            SwLockPlayerVolume.IsOn = SettingService.GetValue(SettingConstants.Player.LOCK_PLAYER_VOLUME,
                SettingConstants.Player.DEFAULT_LOCK_PLAYER_VOLUME);
            SwLockPlayerVolume.Loaded += (sender, e) =>
            {
                SwLockPlayerVolume.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.LOCK_PLAYER_VOLUME,
                        SwLockPlayerVolume.IsOn);
                };
            };

            // 亮度
            SliderBrightness.Value = Math.Round(
                (Math.Abs(SettingService.GetValue(
                    SettingConstants.Player.PLAYER_BRIGHTNESS,
                    SettingConstants.Player.DEFAULT_PLAYER_BRIGHTNESS) - 1)) * 100, 2);
            SliderBrightness.Loaded += (sender, e) =>
            {
                SliderBrightness.ValueChanged += (obj, args) =>
                {
                    var brightness = Math.Abs((SliderBrightness.Value / 100) - 1);
                    SettingService.SetValue(SettingConstants.Player.PLAYER_BRIGHTNESS, brightness);
                };
            };

            // 锁定播放器亮度设置
            SwLockPlayerBrightness.IsOn = SettingService.GetValue(SettingConstants.Player.LOCK_PLAYER_BRIGHTNESS,
                SettingConstants.Player.DEFAULT_LOCK_PLAYER_BRIGHTNESS);
            SwLockPlayerBrightness.Loaded += (sender, e) =>
            {
                SwLockPlayerBrightness.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.LOCK_PLAYER_BRIGHTNESS,
                        SwLockPlayerBrightness.IsOn);
                };
            };

            // Web播放器调试模式
            SwitchEnableWebPlayerDebugMode.IsOn = SettingService.GetValue(SettingConstants.Player.WEB_PLAYER_ENABLE_DEBUG_MODE,
                false);
            SwitchEnableWebPlayerDebugMode.Loaded += (sender, e) =>
            {
                SwitchEnableWebPlayerDebugMode.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.WEB_PLAYER_ENABLE_DEBUG_MODE,
                        SwitchEnableWebPlayerDebugMode.IsOn);
                };
            };

            // Web播放器开发模式
            SwitchEnableWebPlayerDevMode.IsOn = SettingService.GetValue(SettingConstants.Player.WEB_PLAYER_ENABLE_DEV_MODE,
                false);
            SwitchEnableWebPlayerDevMode.Loaded += (sender, e) =>
            {
                SwitchEnableWebPlayerDevMode.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.WEB_PLAYER_ENABLE_DEV_MODE,
                        SwitchEnableWebPlayerDevMode.IsOn);
                };
            };

            //WebPlayer音视频进度同步阈值1
            NumberBoxWebPlayerAVPositionSyncThreshold1.Value = SettingService.GetValue(
                SettingConstants.Player.WEB_PLAYER_AV_POSITION_SYNC_THRESHOLD_1,
                SettingConstants.Player.DEFAULT_WEB_PLAYER_AV_POSITION_SYNC_THRESHOLD_1);
            NumberBoxWebPlayerAVPositionSyncThreshold1.Loaded += (_, _) =>
            {
                NumberBoxWebPlayerAVPositionSyncThreshold1.ValueChanged += (_, _) =>
                {
                    SettingService.SetValue(SettingConstants.Player.WEB_PLAYER_AV_POSITION_SYNC_THRESHOLD_1,
                        NumberBoxWebPlayerAVPositionSyncThreshold1.Value);
                };
            };

            //WebPlayer音视频进度同步阈值2
            NumberBoxWebPlayerAVPositionSyncThreshold2.Value = SettingService.GetValue(
                SettingConstants.Player.WEB_PLAYER_AV_POSITION_SYNC_THRESHOLD_2,
                SettingConstants.Player.DEFAULT_WEB_PLAYER_AV_POSITION_SYNC_THRESHOLD_2);
            NumberBoxWebPlayerAVPositionSyncThreshold2.Loaded += (_, _) =>
            {
                NumberBoxWebPlayerAVPositionSyncThreshold2.ValueChanged += (_, _) =>
                {
                    SettingService.SetValue(SettingConstants.Player.WEB_PLAYER_AV_POSITION_SYNC_THRESHOLD_2,
                        NumberBoxWebPlayerAVPositionSyncThreshold2.Value);
                };
            };

            //自动打开AI字幕
            swPlayerSettingAutoOpenAISubtitle.IsOn =
                SettingService.GetValue<bool>(SettingConstants.Player.AUTO_OPEN_AI_SUBTITLE, false);
            swPlayerSettingAutoOpenAISubtitle.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swPlayerSettingAutoOpenAISubtitle.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.AUTO_OPEN_AI_SUBTITLE,
                        swPlayerSettingAutoOpenAISubtitle.IsOn);
                });
            });
            //上报历史纪录
            SwitchPlayerReportHistory.IsOn = SettingService.GetValue(SettingConstants.Player.REPORT_HISTORY,
                SettingConstants.Player.DEFAULT_REPORT_HISTORY);
            SwitchPlayerReportHistory.Loaded += (sender, e) =>
            {
                SwitchPlayerReportHistory.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.REPORT_HISTORY,
                        SwitchPlayerReportHistory.IsOn);
                };
            };

            //视频结束上报历史纪录0
            SwitchReportHistoryZeroWhenVideoEnd.IsOn = SettingService.GetValue(
                SettingConstants.Player.REPORT_HISTORY_ZERO_WHEN_VIDEO_END,
                SettingConstants.Player.DEFAULT_REPORT_HISTORY_ZERO_WHEN_VIDEO_END);
            SwitchReportHistoryZeroWhenVideoEnd.Loaded += (sender, e) =>
            {
                SwitchReportHistoryZeroWhenVideoEnd.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.REPORT_HISTORY_ZERO_WHEN_VIDEO_END,
                        SwitchReportHistoryZeroWhenVideoEnd.IsOn);
                };
            };

            //自动刷新播放地址
            SwitchAutoRefreshPlayUrl.IsOn = SettingService.GetValue(SettingConstants.Player.AUTO_REFRESH_PLAY_URL,
                SettingConstants.Player.DEFAULT_AUTO_REFRESH_PLAY_URL);
            SwitchAutoRefreshPlayUrl.Loaded += (sender, e) =>
            {
                SwitchAutoRefreshPlayUrl.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.AUTO_REFRESH_PLAY_URL,
                        SwitchAutoRefreshPlayUrl.IsOn);
                };
            };

            SettingService.TryGetValue(
                SettingConstants.Player.AUTO_REFRESH_PLAY_URL_TIME,
                SettingConstants.Player.DEFAULT_AUTO_REFRESH_PLAY_URL_TIME, out var autoRefreshPlayUrlTime);
            NumAutoRefreshPlayUrlTime.Value = autoRefreshPlayUrlTime;

            NumAutoRefreshPlayUrlTime.Loaded += (_, _) =>
            {
                NumAutoRefreshPlayUrlTime.ValueChanged += (_, _) =>
                {
                    SettingService.SetValue(SettingConstants.Player.AUTO_REFRESH_PLAY_URL_TIME,
                        NumAutoRefreshPlayUrlTime.Value);
                };
            };

            //转简体
            RoamingSettingToSimplified.IsOn = SettingService.GetValue<bool>(SettingConstants.Player.TO_SIMPLIFIED, true);
            RoamingSettingToSimplified.Loaded += new RoutedEventHandler((sender, e) =>
            {
                RoamingSettingToSimplified.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.TO_SIMPLIFIED, RoamingSettingToSimplified.IsOn);
                });
            });

            //显示视频底部进度条
            SwShowVideoBottomProgress.IsOn = SettingService.GetValue(SettingConstants.Player.SHOW_VIDEO_BOTTOM_VIRTUAL_PROGRESS_BAR, SettingConstants.Player.DEFAULT_SHOW_VIDEO_BOTTOM_VIRTUAL_PROGRESS_BAR);
            SwShowVideoBottomProgress.Loaded += (sender, e) =>
            {
                SwShowVideoBottomProgress.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.SHOW_VIDEO_BOTTOM_VIRTUAL_PROGRESS_BAR, SwShowVideoBottomProgress.IsOn);
                };
            };

            //总是显示进度条
            SwAlwaysShowVideoProgress.IsOn = SettingService.GetValue(SettingConstants.Player.ALWAYS_SHOW_VIDEO_PROGRESS_BAR, SettingConstants.Player.DEFAULT_ALWAYS_SHOW_VIDEO_PROGRESS_BAR);
            SwAlwaysShowVideoProgress.Loaded += (sender, e) =>
            {
                SwAlwaysShowVideoProgress.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.ALWAYS_SHOW_VIDEO_PROGRESS_BAR, SwAlwaysShowVideoProgress.IsOn);
                };
            };
        }

        private async void BtnEditPlaySpeedMenu_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = App.ServiceProvider.GetRequiredService<EditPlaySpeedMenuDialog>();
            await NotificationShowExtensions.ShowContentDialog(dialog);
        }

        private async void BtnSaveFfmpegInteropXOptions_OnClick(object sender, RoutedEventArgs e)
        {
            // 等待viewModel更新
            await Task.Delay(50);
            SettingService.SetValue(SettingConstants.Player.FFMPEG_INTEROP_X_OPTIONS,
                m_viewModel.FFmpegInteropXOptions);
        }
    }
}
