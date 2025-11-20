using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Services;
using BiliLite.ViewModels.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls.Settings
{
    public sealed partial class LiveSettingsControl : UserControl
    {
        private readonly LiveSettingsControlViewModel m_viewModel;

        public LiveSettingsControl()
        {
            m_viewModel = App.ServiceProvider.GetRequiredService<LiveSettingsControlViewModel>();
            InitializeComponent();
            LoadLiveDanmu();
        }

        private void LoadLiveDanmu()
        {
            // 弹幕引擎
            cbLiveDanmakuEngine.SelectedValue = SettingService.GetValue(SettingConstants.Live.DANMAKU_ENGINE, (int)SettingConstants.Live.DEFAULT_DANMAKU_ENGINE);
            cbLiveDanmakuEngine.Loaded += (sender, e) =>
            {
                cbLiveDanmakuEngine.SelectionChanged += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Live.DANMAKU_ENGINE, cbLiveDanmakuEngine.SelectedValue);
                };
            };

            //弹幕开关
            var state = SettingService.GetValue<Visibility>(SettingConstants.Live.SHOW, Visibility.Visible) == Visibility.Visible;
            LiveDanmuSettingState.IsOn = state;
            LiveDanmuSettingState.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingService.SetValue(SettingConstants.Live.SHOW, LiveDanmuSettingState.IsOn ? Visibility.Visible : Visibility.Collapsed);
            });
            //弹幕关键词
            LiveDanmuSettingListWords.ItemsSource = m_viewModel.LiveShieldWords;
        }

        private void LiveDanmuSettingAddWord_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(LiveDanmuSettingTxtWord.Text))
            {
                NotificationShowExtensions.ShowMessageToast("关键字不能为空");
                return;
            }
            if (!m_viewModel.LiveShieldWords.Contains(LiveDanmuSettingTxtWord.Text))
            {
                m_viewModel.LiveShieldWords.Add(LiveDanmuSettingTxtWord.Text);
                SettingService.SetValue(SettingConstants.Live.SHIELD_WORD, m_viewModel.LiveShieldWords);
            }

            LiveDanmuSettingTxtWord.Text = "";
            SettingService.SetValue(SettingConstants.Live.SHIELD_WORD, m_viewModel.LiveShieldWords);
        }

        private void RemoveLiveDanmuWord_Click(object sender, RoutedEventArgs e)
        {
            var word = (sender as AppBarButton).DataContext as string;
            m_viewModel.LiveShieldWords.Remove(word);
            SettingService.SetValue(SettingConstants.Live.SHIELD_WORD, m_viewModel.LiveShieldWords);
        }

        private async void DanmuSettingFilterImport_OnClick(object sender, RoutedEventArgs e)
        {
            await m_viewModel.ImportDanmuFilter();
        }

        private async void BtnExportDanmuFilter_OnClick(object sender, RoutedEventArgs e)
        {
            await m_viewModel.ExportDanmuFilter();
        }
    }
}
