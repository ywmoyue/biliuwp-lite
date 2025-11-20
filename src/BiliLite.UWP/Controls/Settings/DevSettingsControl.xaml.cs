using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Services;
using BiliLite.ViewModels.Plugins;
using BiliLite.ViewModels.Settings;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using IMapper = AutoMapper.IMapper;
using Microsoft.Windows.AppLifecycle;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls.Settings
{
    public sealed partial class DevSettingsControl : UserControl
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();
        private readonly DevSettingsControlViewModel m_viewModel;
        private readonly PluginService m_pluginService;
        private readonly IMapper m_mapper;

        public DevSettingsControl()
        {
            m_viewModel = App.ServiceProvider.GetRequiredService<DevSettingsControlViewModel>();
            m_mapper = App.ServiceProvider.GetRequiredService<IMapper>();
            m_pluginService = App.ServiceProvider.GetRequiredService<PluginService>();
            m_viewModel.Plugins =
                m_mapper.Map<ObservableCollection<WebSocketPluginViewModel>>(m_pluginService.GetPlugins());
            InitializeComponent();
            LoadDev();
        }

        private void LoadDev()
        {
            //自动清理日志文件
            swAutoClearLogFile.IsOn = SettingService.GetValue<bool>(SettingConstants.Other.AUTO_CLEAR_LOG_FILE, true);
            swAutoClearLogFile.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingService.SetValue(SettingConstants.Other.AUTO_CLEAR_LOG_FILE, swAutoClearLogFile.IsOn);
            });
            //自动清理多少天前的日志文件
            numAutoClearLogDay.Value = SettingService.GetValue<int>(SettingConstants.Other.AUTO_CLEAR_LOG_FILE_DAY, 7);
            numAutoClearLogDay.ValueChanged += ((e, args) =>
            {
                SettingService.SetValue(SettingConstants.Other.AUTO_CLEAR_LOG_FILE_DAY, numAutoClearLogDay.Value);
            });
            //保护日志敏感信息
            swProtectLogInfo.IsOn = SettingService.GetValue<bool>(SettingConstants.Other.PROTECT_LOG_INFO, true);
            swProtectLogInfo.Toggled += ((e, args) =>
            {
                SettingService.SetValue(SettingConstants.Other.PROTECT_LOG_INFO, swProtectLogInfo.IsOn);
            });
            // 日志级别
            cbLogLevel.SelectedIndex = SettingService.GetValue(SettingConstants.Other.LOG_LEVEL, 2);
            cbLogLevel.Loaded += (sender, e) =>
            {
                cbLogLevel.SelectionChanged += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Other.LOG_LEVEL, cbLogLevel.SelectedIndex);
                };
            };

            // 优先使用Grpc请求动态
            swFirstGrpcRequestDynamic.IsOn =
                SettingService.GetValue<bool>(SettingConstants.Other.FIRST_GRPC_REQUEST_DYNAMIC, true);
            swFirstGrpcRequestDynamic.Toggled += ((e, args) =>
            {
                SettingService.SetValue(SettingConstants.Other.FIRST_GRPC_REQUEST_DYNAMIC,
                    swFirstGrpcRequestDynamic.IsOn);
            });

            RequestBuildTextBox.Text = SettingService.GetValue(SettingConstants.Other.REQUEST_BUILD,
                SettingConstants.Other.DEFAULT_REQUEST_BUILD);

            // BiliLiteWebApi
            BiliLiteWebApiTextBox.Text = SettingService.GetValue(SettingConstants.Other.BILI_LITE_WEB_API_BASE_URL,
                ApiConstants.BILI_LITE_WEB_API_DEFAULT_BASE_URL);
            BiliLiteWebApiTextBox.Loaded += (sender, e) =>
            {
                BiliLiteWebApiTextBox.QuerySubmitted += (sender2, args) =>
                {
                    var text = sender2.Text;
                    if (string.IsNullOrEmpty(text))
                    {
                        NotificationShowExtensions.ShowMessageToast("已取消自定义BiliLiteWebApi服务器");
                        SettingService.SetValue(SettingConstants.Other.BILI_LITE_WEB_API_BASE_URL, "");
                        return;
                    }

                    if (!text.EndsWith("/")) text += "/";
                    if (!Uri.IsWellFormedUriString(text, UriKind.Absolute))
                    {
                        NotificationShowExtensions.ShowMessageToast("地址格式错误");
                        return;
                    }

                    SettingService.SetValue(SettingConstants.Other.BILI_LITE_WEB_API_BASE_URL, text);
                    sender2.Text = text;
                    NotificationShowExtensions.ShowMessageToast("保存成功");
                };
            };

            // 更新json来源
            var selectedValue = SettingService.GetValue(SettingConstants.Other.UPDATE_JSON_ADDRESS,
                UpdateJsonAddressOptions.DEFAULT_UPDATE_JSON_ADDRESS);
            selectedValue = selectedValue.Replace("\"", ""); // 解决取出的值有奇怪的转义符
            updateJsonAddress.SelectedItem = UpdateJsonAddressOptions.GetOption(selectedValue);
            MirrorComboboxSelectAction(selectedValue);
            updateJsonAddress.Loaded += (sender, e) =>
            {
                updateJsonAddress.SelectionChanged += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Other.UPDATE_JSON_ADDRESS,
                        updateJsonAddress.SelectedValue);
                    MirrorComboboxSelectAction(updateJsonAddress.SelectedValue);
                };
            };
        }

        private void RequestBuildSaveBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var build = RequestBuildTextBox.Text;
            if (string.IsNullOrWhiteSpace(build))
            {
                NotificationShowExtensions.ShowMessageToast("请输入正确的build值");
                return;
            }

            SettingService.SetValue(SettingConstants.Other.REQUEST_BUILD, build);
            NotificationShowExtensions.ShowMessageToast("已保存");
        }

        private void RequestBuildDefaultBtn_OnClick(object sender, RoutedEventArgs e)
        {
            var build = SettingConstants.Other.DEFAULT_REQUEST_BUILD;
            SettingService.SetValue(SettingConstants.Other.REQUEST_BUILD, build);
            RequestBuildTextBox.Text = build;
            NotificationShowExtensions.ShowMessageToast("已恢复默认");
        }

        private void MirrorComboboxSelectAction(object selectedValue)
        {
            switch (selectedValue)
            {
                case ApiHelper.GHPROXY_GIT_RAW_URL:
                    {
                        mirrorDonateText.Visibility = Visibility.Visible;
                        mirrorDonateUrl.NavigateUri = new Uri("https://ghfast.top/donate");
                        break;
                    }
                case ApiHelper.KGITHUB_GIT_RAW_URL:
                    {
                        mirrorDonateText.Visibility = Visibility.Visible;
                        mirrorDonateUrl.NavigateUri = new Uri("https://help.kkgithub.com/donate");
                        break;
                    }
                case ApiHelper.JSDELIVR_GIT_RAW_URL:
                    {
                        mirrorDonateText.Visibility = Visibility.Visible;
                        mirrorDonateUrl.NavigateUri = new Uri("https://www.jsdelivr.com/become-a-sponsor");
                        break;
                    }
                default:
                    {
                        mirrorDonateText.Visibility = Visibility.Collapsed;
                        break;
                    }
            }
        }

        private async void BtnExportSettings_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var exportService = App.ServiceProvider.GetRequiredService<SettingsImportExportService>();
                await exportService.ExportSettings();
            }
            catch (Exception ex)
            {
                _logger.Error("导出失败", ex);
                NotificationShowExtensions.ShowMessageToast("导出失败，已记录错误");
            }
        }

        private async void BtnImportSettings_OnClick(object sender, RoutedEventArgs e)
        {
            var importService = App.ServiceProvider.GetRequiredService<SettingsImportExportService>();
            if (!await importService.ImportSettings())
            {
                return;
            }

            NotificationShowExtensions.ShowMessageToast("导入成功，正在重启应用");
            // 等用户看提示
            await Task.Delay(1500);
            var result = AppInstance.Restart("");

            if (result == AppRestartFailureReason.NotInForeground || result == AppRestartFailureReason.Other)
            {
                NotificationShowExtensions.ShowMessageToast("重启失败，请手动重启应用");
            }
        }

        private async void BtnExportSettingsWithAccount_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var exportService = App.ServiceProvider.GetRequiredService<SettingsImportExportService>();
                await exportService.ExportSettingsWithAccount();
            }
            catch (Exception ex)
            {
                _logger.Error("导出失败", ex);
                NotificationShowExtensions.ShowMessageToast("导出失败，已记录错误");
            }
        }

        private async void BtnOpenLogFolder_OnClick(object sender, RoutedEventArgs e)
        {
            var path = Windows.Storage.ApplicationData.Current.LocalFolder.Path + @"\log\";
            await Windows.System.Launcher.LaunchFolderPathAsync(path);
        }

        private async void BtnMigrateDb_OnClick(object sender, RoutedEventArgs e)
        {
            var migrateService = App.ServiceProvider.GetRequiredService<SqlMigrateService>();
            await migrateService.ExcuteAllMigrationScripts();
        }

        private async void BtnSettingPlugin_OnClick(object sender, RoutedEventArgs e)
        {
            await NotificationShowExtensions.ShowContentDialog(PluginsDialog);
        }

        private async void BtnImportPluginInfo_OnClick(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private async void BtnDeletePlugin_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button { DataContext: WebSocketPluginViewModel plugin })) return;
            await m_pluginService.RemovePlugin(plugin.Name);
            m_viewModel.RemovePlugin(plugin);
        }
    }
}
