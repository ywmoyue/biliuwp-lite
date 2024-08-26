using BiliLite.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BiliLite.Models.Common;
using BiliLite.Services;
using BiliLite.ViewModels.Settings;
using System.Linq;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls.Settings
{
    public sealed partial class PlaySettingsControl : UserControl
    {
        private readonly PlaySettingsControlViewModel m_viewModel;
        private readonly PlaySpeedMenuService m_playSpeedMenuService;

        public PlaySettingsControl()
        {
            m_viewModel = App.ServiceProvider.GetRequiredService<PlaySettingsControlViewModel>();
            m_playSpeedMenuService = App.ServiceProvider.GetRequiredService<PlaySpeedMenuService>();
            this.InitializeComponent();
            LoadPlayer();
        }

        private void LoadPlayer()
        {
            //播放类型
            var selectedValue = (PlayUrlCodecMode)SettingService.GetValue(SettingConstants.Player.DEFAULT_VIDEO_TYPE, (int)DefaultVideoTypeOptions.DEFAULT_VIDEO_TYPE);
            cbVideoType.SelectedItem = DefaultVideoTypeOptions.GetOption(selectedValue);
            cbVideoType.SelectionChanged += (e, args) =>
            {
                SettingService.SetValue(SettingConstants.Player.DEFAULT_VIDEO_TYPE, (int)cbVideoType.SelectedValue);
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
                    SettingService.SetValue(SettingConstants.Player.DEFAULT_VIDEO_SPEED, speeds[cbVideoSpeed.SelectedIndex]);
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
            swPlayerSettingAutoToPosition.IsOn = SettingService.GetValue<bool>(SettingConstants.Player.AUTO_TO_POSITION, true);
            swPlayerSettingAutoToPosition.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swPlayerSettingAutoToPosition.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.AUTO_TO_POSITION, swPlayerSettingAutoToPosition.IsOn);
                });
            });
            //自动铺满屏幕
            swPlayerSettingAutoFullWindows.IsOn = SettingService.GetValue<bool>(SettingConstants.Player.AUTO_FULL_WINDOW, false);
            swPlayerSettingAutoFullWindows.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swPlayerSettingAutoFullWindows.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.AUTO_FULL_WINDOW, swPlayerSettingAutoFullWindows.IsOn);
                });
            });
            //默认最大音质
            SwitchEnableDefaultMaxSoundQuality.IsOn = SettingService.GetValue(SettingConstants.Player.ENABLE_DEFAULT_MAX_SOUND_QUALITY, 
                SettingConstants.Player.DEFAULT_ENABLE_DEFAULT_MAX_SOUND_QUALITY);
            SwitchEnableDefaultMaxSoundQuality.Loaded += (sender, e) =>
            {
                SwitchEnableDefaultMaxSoundQuality.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.ENABLE_DEFAULT_MAX_SOUND_QUALITY, SwitchEnableDefaultMaxSoundQuality.IsOn);
                };
            };
            //自动全屏
            swPlayerSettingAutoFullScreen.IsOn = SettingService.GetValue<bool>(SettingConstants.Player.AUTO_FULL_SCREEN, false);
            swPlayerSettingAutoFullScreen.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swPlayerSettingAutoFullScreen.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.AUTO_FULL_SCREEN, swPlayerSettingAutoFullScreen.IsOn);
                });
            });


            //双击全屏
            swPlayerSettingDoubleClickFullScreen.IsOn = SettingService.GetValue<bool>(SettingConstants.Player.DOUBLE_CLICK_FULL_SCREEN, false);
            swPlayerSettingDoubleClickFullScreen.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swPlayerSettingDoubleClickFullScreen.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.DOUBLE_CLICK_FULL_SCREEN, swPlayerSettingDoubleClickFullScreen.IsOn);
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
            cbPlayerHoldingGestureAction.SelectedIndex = SettingService.GetValue(SettingConstants.Player.HOLDING_GESTURE_ACTION, (int)PlayerHoldingAction.None);
            cbPlayerHoldingGestureAction.Loaded += (sender, e) =>
            {
                cbPlayerHoldingGestureAction.SelectionChanged += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.HOLDING_GESTURE_ACTION, cbPlayerHoldingGestureAction.SelectedIndex);
                };
            };

            // 按住手势可被其他手势取消
            swPlayerHoldingGestureCanCancel.IsOn = SettingService.GetValue(SettingConstants.Player.HOLDING_GESTURE_CAN_CANCEL, true);
            swPlayerHoldingGestureCanCancel.Loaded += (sender, e) =>
            {
                swPlayerHoldingGestureCanCancel.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.HOLDING_GESTURE_CAN_CANCEL, swPlayerHoldingGestureCanCancel.IsOn);
                };
            };

            // 倍速播放速度
            cbRatePlaySpeed.SelectedIndex = SettingConstants.Player.HIGH_RATE_PLAY_SPEED_LIST.IndexOf(SettingService.GetValue(SettingConstants.Player.HIGH_RATE_PLAY_SPEED, 2.0d));
            cbRatePlaySpeed.Loaded += (sender, e) =>
            {
                cbRatePlaySpeed.SelectionChanged += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.HIGH_RATE_PLAY_SPEED, SettingConstants.Player.HIGH_RATE_PLAY_SPEED_LIST[cbRatePlaySpeed.SelectedIndex]);
                };
            };

            // 音量
            NumBoxVolume.Value = Math.Round((SettingService.GetValue(SettingConstants.Player.PLAYER_VOLUME,
                SettingConstants.Player.DEFAULT_PLAYER_VOLUME)) * 100, 2);
            NumBoxVolume.Loaded += (sender, e) =>
            {
                NumBoxVolume.ValueChanged += (obj, args) =>
                {
                    if (NumBoxVolume.Value > 100)
                    {
                        NumBoxVolume.Value = 100;
                    }

                    if (NumBoxVolume.Value < 0)
                    {
                        NumBoxVolume.Value = 0;
                    }
                    SettingService.SetValue(SettingConstants.Player.PLAYER_VOLUME, NumBoxVolume.Value / 100);
                };
            };

            // 锁定播放器音量设置
            SwLockPlayerVolume.IsOn = SettingService.GetValue(SettingConstants.Player.LOCK_PLAYER_VOLUME, SettingConstants.Player.DEFAULT_LOCK_PLAYER_VOLUME);
            SwLockPlayerVolume.Loaded += (sender, e) =>
            {
                SwLockPlayerVolume.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.LOCK_PLAYER_VOLUME, SwLockPlayerVolume.IsOn);
                };
            };

            // 亮度
            NumBoxBrightness.Value = Math.Round(
                (Math.Abs(SettingService.GetValue(
                SettingConstants.Player.PLAYER_BRIGHTNESS,
                SettingConstants.Player.DEFAULT_PLAYER_BRIGHTNESS) - 1)) * 100, 2);
            NumBoxBrightness.Loaded += (sender, e) =>
            {
                NumBoxBrightness.ValueChanged += (obj, args) =>
                {
                    if (NumBoxBrightness.Value > 100)
                    {
                        NumBoxBrightness.Value = 100;
                    }

                    if (NumBoxBrightness.Value < 0)
                    {
                        NumBoxBrightness.Value = 0;
                    }

                    var brightness = Math.Abs((NumBoxBrightness.Value / 100) - 1);
                    SettingService.SetValue(SettingConstants.Player.PLAYER_BRIGHTNESS, brightness);
                };
            };

            // 锁定播放器亮度设置
            SwLockPlayerBrightness.IsOn = SettingService.GetValue(SettingConstants.Player.LOCK_PLAYER_BRIGHTNESS, SettingConstants.Player.DEFAULT_LOCK_PLAYER_BRIGHTNESS);
            SwLockPlayerBrightness.Loaded += (sender, e) =>
            {
                SwLockPlayerBrightness.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.LOCK_PLAYER_BRIGHTNESS, SwLockPlayerBrightness.IsOn);
                };
            };

            //自动打开AI字幕
            swPlayerSettingAutoOpenAISubtitle.IsOn = SettingService.GetValue<bool>(SettingConstants.Player.AUTO_OPEN_AI_SUBTITLE, false);
            swPlayerSettingAutoOpenAISubtitle.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swPlayerSettingAutoOpenAISubtitle.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.AUTO_OPEN_AI_SUBTITLE, swPlayerSettingAutoOpenAISubtitle.IsOn);
                });
            });
            //上报历史纪录
            SwitchPlayerReportHistory.IsOn = SettingService.GetValue(SettingConstants.Player.REPORT_HISTORY, SettingConstants.Player.DEFAULT_REPORT_HISTORY);
            SwitchPlayerReportHistory.Loaded += (sender, e) =>
            {
                SwitchPlayerReportHistory.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.REPORT_HISTORY, SwitchPlayerReportHistory.IsOn);
                };
            };

            //视频结束上报历史纪录0
            SwitchReportHistoryZeroWhenVideoEnd.IsOn = SettingService.GetValue(SettingConstants.Player.REPORT_HISTORY_ZERO_WHEN_VIDEO_END, SettingConstants.Player.DEFAULT_REPORT_HISTORY_ZERO_WHEN_VIDEO_END);
            SwitchReportHistoryZeroWhenVideoEnd.Loaded += (sender, e) =>
            {
                SwitchReportHistoryZeroWhenVideoEnd.Toggled += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.REPORT_HISTORY_ZERO_WHEN_VIDEO_END, SwitchReportHistoryZeroWhenVideoEnd.IsOn);
                };
            };
            //替换CDN
            cbPlayerReplaceCDN.SelectedIndex = SettingService.GetValue<int>(SettingConstants.Player.REPLACE_CDN, 3);
            cbPlayerReplaceCDN.Loaded += new RoutedEventHandler((sender, e) =>
            {
                cbPlayerReplaceCDN.SelectionChanged += new SelectionChangedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Player.REPLACE_CDN, cbPlayerReplaceCDN.SelectedIndex);
                });
            });
            //CDN服务器
            var cdnServer = SettingService.GetValue<string>(SettingConstants.Player.CDN_SERVER, "upos-sz-mirrorhwo1.bilivideo.com");
            RoamingSettingCDNServer.SelectedIndex = m_viewModel.CDNServers.FindIndex(x => x.Server == cdnServer);
            RoamingSettingCDNServer.Loaded += new RoutedEventHandler((sender, e) =>
            {
                RoamingSettingCDNServer.SelectionChanged += new SelectionChangedEventHandler((obj, args) =>
                {
                    var server = m_viewModel.CDNServers[RoamingSettingCDNServer.SelectedIndex];
                    SettingService.SetValue(SettingConstants.Player.CDN_SERVER, server.Server);

                });
            });
        }

        private async void BtnEditPlaySpeedMenu_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = App.ServiceProvider.GetRequiredService<EditPlaySpeedMenuDialog>();
            await dialog.ShowAsync();
        }

        private void RoamingSettingTestCDN_Click(object sender, RoutedEventArgs e)
        {
            m_viewModel.CDNServerDelayTest();
        }
    }
}
