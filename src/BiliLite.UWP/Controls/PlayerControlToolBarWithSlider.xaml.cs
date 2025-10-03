using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Microsoft.Extensions.DependencyInjection;
using BiliLite.Converters;
using BiliLite.Services;
using BiliLite.Models.Common.Video.PlayUrlInfos;
using BiliLite.Models.Common;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Attributes;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls
{
    [RegisterTransientService]
    public sealed partial class PlayerControlToolBarWithSlider : UserControl, IPlayerControlToolBar
    {
        private readonly PlaySpeedMenuService m_playSpeedMenuService;
        private readonly SoundQualitySliderTooltipConverter m_soundQualitySliderTooltipConverter;
        private readonly QualitySliderTooltipConverter m_qualitySliderTooltipConverter;
        private readonly PlaySpeedSliderTooltipConverter m_playSpeedSliderTooltipConverter;
        private PlayerToastService m_playerToastService;

        public PlayerControlToolBarWithSlider()
        {
            this.InitializeComponent();
            m_playSpeedMenuService = App.ServiceProvider.GetRequiredService<PlaySpeedMenuService>();
            m_soundQualitySliderTooltipConverter = new SoundQualitySliderTooltipConverter();
            m_qualitySliderTooltipConverter = new QualitySliderTooltipConverter();
            m_playSpeedSliderTooltipConverter = new PlaySpeedSliderTooltipConverter(m_playSpeedMenuService);
        }


        public event EventHandler<BiliDashAudioPlayUrlInfo> SoundQualityChanged;
        public event EventHandler<BiliPlayUrlInfo> QualityChanged;
        public event EventHandler<double> PlaySpeedChanged;

        public PlayerToastService PlayerToastService
        {
            set => m_playerToastService = value;
        }

        public bool FirstMediaOpened { get; set; }

        public void InitSoundQuality(List<BiliDashAudioPlayUrlInfo> audioQualites, BiliDashAudioPlayUrlInfo currentAudioQuality)
        {
            if (audioQualites == null || !audioQualites.Any())
            {
                return;
            }

            MinSoundQuality.Text = audioQualites.First().QualityName;
            MaxSoundQuality.Text = audioQualites.Last().QualityName;

            BottomBtnSoundQuality.IsEnabled = audioQualites.Count > 1;
            BottomBtnSoundQuality.Content = currentAudioQuality.QualityName;
            SliderSoundQuality.Maximum = audioQualites.Count - 1;
            SliderSoundQuality.Value = audioQualites.IndexOf(currentAudioQuality);
            m_soundQualitySliderTooltipConverter.AudioQualites = audioQualites;
            SliderSoundQuality.ThumbToolTipValueConverter = m_soundQualitySliderTooltipConverter;
        }

        public void InitQuality(List<BiliPlayUrlInfo> videoQualites, BiliPlayUrlInfo currentQuality)
        {
            if (videoQualites == null || !videoQualites.Any())
            {
                return;
            }

            MinQuality.Text = videoQualites.First().QualityName;
            MaxQuality.Text = videoQualites.Last().QualityName;

            BottomBtnQuality.IsEnabled = videoQualites.Count > 1;
            BottomBtnQuality.Content = currentQuality.QualityName;
            SliderQuality.Maximum = videoQualites.Count - 1;
            SliderQuality.Value = videoQualites.IndexOf(currentQuality);
            m_qualitySliderTooltipConverter.Qualites = videoQualites;
            SliderQuality.ThumbToolTipValueConverter = m_qualitySliderTooltipConverter;
        }

        public void InitPlaySpeed()
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
        }


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

        private void SliderSoundQuality_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!FirstMediaOpened) return;
            var audioQualites = m_soundQualitySliderTooltipConverter.AudioQualites;
            var latestChoice = audioQualites[(int)SliderSoundQuality.Value];
            BottomBtnSoundQuality.Content = latestChoice.QualityName;

            SoundQualityChanged?.Invoke(this, latestChoice);
        }

        private void SliderQuality_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!FirstMediaOpened) return;
            var videoQualites = m_qualitySliderTooltipConverter.Qualites;
            var latestChoice = videoQualites[(int)SliderQuality.Value];
            BottomBtnQuality.Content = latestChoice.QualityName;

            QualityChanged?.Invoke(this, latestChoice);
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
            BottomBtnPlaySpeed.Content = latestChoice.Content;

            PlaySpeedChanged?.Invoke(this, latestChoice.Value);
        }
    }
}
