using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using BiliLite.Services;
using BiliLite.Models.Common.Video.PlayUrlInfos;
using BiliLite.Models.Common;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common.Player;

namespace BiliLite.Controls
{
    [RegisterTransientService]
    public sealed partial class PlayerControlToolBarWithComboBox : UserControl, IPlayerControlToolBar
    {
        private readonly PlaySpeedMenuService m_playSpeedMenuService;
        private PlayerToastService m_playerToastService;

        public PlayerControlToolBarWithComboBox()
        {
            this.InitializeComponent();
            m_playSpeedMenuService = App.ServiceProvider.GetRequiredService<PlaySpeedMenuService>();
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

            SoundQualityComboBox.ItemsSource = audioQualites;
            SoundQualityComboBox.SelectedItem = currentAudioQuality;
            SoundQualityComboBox.IsEnabled = audioQualites.Count > 1;
            SoundQualityComboBox.DisplayMemberPath = "QualityName";
        }

        public void InitQuality(List<BiliPlayUrlInfo> videoQualites, BiliPlayUrlInfo currentQuality)
        {
            if (videoQualites == null || !videoQualites.Any())
            {
                return;
            }

            QualityComboBox.ItemsSource = videoQualites;
            QualityComboBox.SelectedItem = currentQuality;
            QualityComboBox.IsEnabled = videoQualites.Count > 1;
            QualityComboBox.DisplayMemberPath = "QualityName";
        }

        public void InitPlaySpeed()
        {
            PlaySpeedComboBox.ItemsSource = m_playSpeedMenuService.MenuItems;
            var value = SettingService.GetValue<double>(SettingConstants.Player.DEFAULT_VIDEO_SPEED, 1.0d);
            var currentPlaySpeed = m_playSpeedMenuService.MenuItems.FirstOrDefault(x => x.Value == value);
            PlaySpeedComboBox.SelectedItem = currentPlaySpeed;
            PlaySpeedComboBox.IsEnabled = m_playSpeedMenuService.MenuItems.Count > 1;
        }

        public void SlowDown()
        {
            var index = PlaySpeedComboBox.SelectedIndex;
            if (index <= 0)
            {
                NotificationShowExtensions.ShowMessageToast("不能再慢啦");
                return;
            }

            PlaySpeedComboBox.SelectedIndex = index - 1;
            m_playerToastService.Show(PlayerToastService.SPEED_KEY, $"{((PlaySpeedMenuItem)PlaySpeedComboBox.SelectedItem).Content}");
        }

        public void FastUp()
        {
            var index = PlaySpeedComboBox.SelectedIndex;
            if (index >= m_playSpeedMenuService.MenuItems.Count - 1)
            {
                NotificationShowExtensions.ShowMessageToast("不能再快啦");
                return;
            }

            PlaySpeedComboBox.SelectedIndex = index + 1;
            m_playerToastService.Show(PlayerToastService.SPEED_KEY, $"{((PlaySpeedMenuItem)PlaySpeedComboBox.SelectedItem).Content}");
        }

        public double GetPlaySpeed()
        {
            return PlaySpeedComboBox.SelectedItem != null ?
                ((PlaySpeedMenuItem)PlaySpeedComboBox.SelectedItem).Value : 1.0;
        }

        public void SetPlaySpeed(double speed)
        {
            var item = m_playSpeedMenuService.MenuItems.FirstOrDefault(x => x.Value == speed);
            if (item != null)
            {
                PlaySpeedComboBox.SelectedItem = item;
            }
        }

        private void SoundQualityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!FirstMediaOpened || SoundQualityComboBox.SelectedItem == null) return;
            SoundQualityChanged?.Invoke(this, (BiliDashAudioPlayUrlInfo)SoundQualityComboBox.SelectedItem);
        }

        private void QualityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!FirstMediaOpened || QualityComboBox.SelectedItem == null) return;
            QualityChanged?.Invoke(this, (BiliPlayUrlInfo)QualityComboBox.SelectedItem);
        }

        private void PlaySpeedComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PlaySpeedComboBox.SelectedItem == null) return;
            var selectedItem = (PlaySpeedMenuItem)PlaySpeedComboBox.SelectedItem;
            PlaySpeedChanged?.Invoke(this, selectedItem.Value);
        }
    }
}
