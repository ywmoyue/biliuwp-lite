using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Services;
using BiliLite.ViewModels.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls.Settings
{
    public sealed partial class VideoDanmakuSettingsControl : UserControl
    {
        private readonly VideoDanmakuSettingsControlViewModel m_viewModel;

        public VideoDanmakuSettingsControl()
        {
            m_viewModel = App.ServiceProvider.GetRequiredService<VideoDanmakuSettingsControlViewModel>();
            InitializeComponent();
            LoadDanmu();
        }

        private void LoadDanmu()
        {
            // 弹幕引擎
            cbDanmakuEngine.SelectedValue = SettingService.GetValue(SettingConstants.VideoDanmaku.DANMAKU_ENGINE, (int)SettingConstants.VideoDanmaku.DEFAULT_DANMAKU_ENGINE);
            cbDanmakuEngine.Loaded += (sender, e) =>
            {
                cbDanmakuEngine.SelectionChanged += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.VideoDanmaku.DANMAKU_ENGINE, cbDanmakuEngine.SelectedValue);
                };
            };
            //弹幕开关
            var state = SettingService.GetValue<Visibility>(SettingConstants.VideoDanmaku.SHOW, Visibility.Visible) == Visibility.Visible;
            DanmuSettingState.IsOn = state;
            DanmuSettingState.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingService.SetValue(SettingConstants.VideoDanmaku.SHOW, DanmuSettingState.IsOn ? Visibility.Visible : Visibility.Collapsed);
            });
            //弹幕关键词
            DanmuSettingListWords.ItemsSource = m_viewModel.ShieldWords;

            //正则关键词
            DanmuSettingListRegulars.ItemsSource = m_viewModel.ShieldRegulars;

            //用户
            DanmuSettingListUsers.ItemsSource = m_viewModel.ShieldUsers;

            //弹幕顶部距离
            numDanmakuTopMargin.Value = SettingService.GetValue<double>(SettingConstants.VideoDanmaku.TOP_MARGIN, 0);
            numDanmakuTopMargin.Loaded += new RoutedEventHandler((sender, e) =>
            {
                numDanmakuTopMargin.ValueChanged += new TypedEventHandler<NumberBox, NumberBoxValueChangedEventArgs>((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.VideoDanmaku.TOP_MARGIN, args.NewValue);
                });
            });
            //弹幕最大数量
            numDanmakuMaxNum.Value = SettingService.GetValue<double>(SettingConstants.VideoDanmaku.MAX_NUM, 0);
            numDanmakuMaxNum.Loaded += new RoutedEventHandler((sender, e) =>
            {
                numDanmakuMaxNum.ValueChanged += new TypedEventHandler<NumberBox, NumberBoxValueChangedEventArgs>((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.VideoDanmaku.MAX_NUM, args.NewValue);
                });
            });

            SwitchDanmakuDebugMode.IsOn = SettingService.GetValue(SettingConstants.VideoDanmaku.DANMAKU_DEBUG_MODE, false);
            SwitchDanmakuDebugMode.Loaded += (_, _) =>
            {
                SwitchDanmakuDebugMode.Toggled += (_, _) =>
                {
                    SettingService.SetValue(SettingConstants.VideoDanmaku.DANMAKU_DEBUG_MODE,
                        SwitchDanmakuDebugMode.IsOn);
                };
            };
        }

        private async void DanmuSettingAddWord_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(DanmuSettingTxtWord.Text))
            {
                NotificationShowExtensions.ShowMessageToast("关键词不能为空");
                return;
            }
            m_viewModel.ShieldWords.Add(DanmuSettingTxtWord.Text);
            SettingService.SetValue(SettingConstants.VideoDanmaku.SHIELD_WORD, m_viewModel.ShieldWords);
            var result = await m_viewModel.AddDanmuFilterItem(DanmuSettingTxtWord.Text, 0);
            DanmuSettingTxtWord.Text = "";
            if (!result)
            {
                NotificationShowExtensions.ShowMessageToast("已经添加到本地，但远程同步失败");
            }
        }

        private async void DanmuSettingFilterImport_OnClick(object sender, RoutedEventArgs e)
        {
            await m_viewModel.ImportDanmuFilter();
        }

        private async void DanmuSettingSyncWords_Click(object sender, RoutedEventArgs e)
        {
            await m_viewModel.SyncDanmuFilter();
        }

        private void RemoveDanmuWord_Click(object sender, RoutedEventArgs e)
        {
            var word = (sender as AppBarButton).DataContext as string;
            m_viewModel.ShieldWords.Remove(word);
            SettingService.SetValue(SettingConstants.VideoDanmaku.SHIELD_WORD, m_viewModel.ShieldWords);
        }

        private void RemoveDanmuRegular_Click(object sender, RoutedEventArgs e)
        {
            var word = (sender as AppBarButton).DataContext as string;
            m_viewModel.ShieldRegulars.Remove(word);
            SettingService.SetValue(SettingConstants.VideoDanmaku.SHIELD_REGULAR, m_viewModel.ShieldRegulars);
        }

        private void RemoveDanmuUser_Click(object sender, RoutedEventArgs e)
        {
            var word = (sender as AppBarButton).DataContext as string;
            m_viewModel.ShieldUsers.Remove(word);
            SettingService.SetValue(SettingConstants.VideoDanmaku.SHIELD_USER, m_viewModel.ShieldUsers);
        }

        private async void DanmuSettingAddRegex_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(DanmuSettingTxtRegex.Text))
            {
                NotificationShowExtensions.ShowMessageToast("正则表达式不能为空");
                return;
            }
            var txt = DanmuSettingTxtRegex.Text.Trim('/');
            m_viewModel.ShieldRegulars.Add(txt);
            SettingService.SetValue(SettingConstants.VideoDanmaku.SHIELD_REGULAR, m_viewModel.ShieldRegulars);
            var result = await m_viewModel.AddDanmuFilterItem(txt, 1);
            DanmuSettingTxtRegex.Text = "";
            if (!result)
            {
                NotificationShowExtensions.ShowMessageToast("已经添加到本地，但远程同步失败");
            }
        }

        private async void DanmuSettingAddUser_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(DanmuSettingTxtUser.Text))
            {
                NotificationShowExtensions.ShowMessageToast("用户ID不能为空");
                return;
            }
            m_viewModel.ShieldUsers.Add(DanmuSettingTxtUser.Text);
            SettingService.SetValue(SettingConstants.VideoDanmaku.SHIELD_WORD, m_viewModel.ShieldUsers);
            var result = await m_viewModel.AddDanmuFilterItem(DanmuSettingTxtUser.Text, 2);
            DanmuSettingTxtUser.Text = "";
            if (!result)
            {
                NotificationShowExtensions.ShowMessageToast("已经添加到本地，但远程同步失败");
            }
        }

        private async void BtnExportDanmuFilter_OnClick(object sender, RoutedEventArgs e)
        {
            await m_viewModel.ExportDanmuFilter();
        }
    }
}
