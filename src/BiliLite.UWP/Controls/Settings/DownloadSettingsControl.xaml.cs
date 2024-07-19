using System;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BiliLite.Models.Common;
using BiliLite.Services;
using BiliLite.ViewModels.Download;
using Microsoft.Extensions.DependencyInjection;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls.Settings
{
    public sealed partial class DownloadSettingsControl : UserControl
    {
        private readonly DownloadPageViewModel m_downloadPageViewModel;

        public DownloadSettingsControl()
        {
            m_downloadPageViewModel = App.ServiceProvider.GetRequiredService<DownloadPageViewModel>();
            this.InitializeComponent();
            LoadDownlaod();
        }

        private void LoadDownlaod()
        {
            //下载路径
            txtDownloadPath.Text = SettingService.GetValue(SettingConstants.Download.DOWNLOAD_PATH, SettingConstants.Download.DEFAULT_PATH);
            DownloadOpenPath.Click += new RoutedEventHandler(async (e, args) =>
            {
                if (txtDownloadPath.Text == SettingConstants.Download.DEFAULT_PATH)
                {
                    var videosLibrary = Windows.Storage.KnownFolders.VideosLibrary;
                    videosLibrary = await videosLibrary.CreateFolderAsync("哔哩哔哩下载", CreationCollisionOption.OpenIfExists);

                    await Windows.System.Launcher.LaunchFolderAsync(videosLibrary);
                }
                else
                {
                    await Windows.System.Launcher.LaunchFolderPathAsync(txtDownloadPath.Text);
                }
            });
            DownloadChangePath.Click += new RoutedEventHandler(async (e, args) =>
            {
                FolderPicker folderPicker = new FolderPicker();
                folderPicker.FileTypeFilter.Add("*");
                folderPicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
                var folder = await folderPicker.PickSingleFolderAsync();
                if (folder != null)
                {
                    SettingService.SetValue(SettingConstants.Download.DOWNLOAD_PATH, folder.Path);
                    txtDownloadPath.Text = folder.Path;
                    Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(folder);
                    m_downloadPageViewModel.RefreshDownloaded();
                }
            });
            //旧版下载目录
            txtDownloadOldPath.Text = SettingService.GetValue(SettingConstants.Download.OLD_DOWNLOAD_PATH, SettingConstants.Download.DEFAULT_OLD_PATH);
            DownloadOpenOldPath.Click += new RoutedEventHandler(async (e, args) =>
            {
                if (txtDownloadOldPath.Text == SettingConstants.Download.DEFAULT_OLD_PATH)
                {
                    var videosLibrary = Windows.Storage.KnownFolders.VideosLibrary;
                    videosLibrary = await videosLibrary.CreateFolderAsync("BiliBiliDownload", CreationCollisionOption.OpenIfExists);
                    await Windows.System.Launcher.LaunchFolderAsync(videosLibrary);
                }
                else
                {
                    await Windows.System.Launcher.LaunchFolderPathAsync(txtDownloadOldPath.Text);
                }
            });
            DownloadChangeOldPath.Click += new RoutedEventHandler(async (e, args) =>
            {
                FolderPicker folderPicker = new FolderPicker();
                folderPicker.FileTypeFilter.Add("*");
                folderPicker.SuggestedStartLocation = PickerLocationId.VideosLibrary;
                var folder = await folderPicker.PickSingleFolderAsync();
                if (folder != null)
                {
                    SettingService.SetValue(SettingConstants.Download.OLD_DOWNLOAD_PATH, folder.Path);
                    txtDownloadOldPath.Text = folder.Path;
                    Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(folder);
                }
            });

            //并行下载
            swDownloadParallelDownload.IsOn = SettingService.GetValue<bool>(SettingConstants.Download.PARALLEL_DOWNLOAD, true);
            swDownloadParallelDownload.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingService.SetValue(SettingConstants.Download.PARALLEL_DOWNLOAD, swDownloadParallelDownload.IsOn);
                m_downloadPageViewModel.UpdateSetting();
            });
            //付费网络下载
            swDownloadAllowCostNetwork.IsOn = SettingService.GetValue<bool>(SettingConstants.Download.ALLOW_COST_NETWORK, false);
            swDownloadAllowCostNetwork.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingService.SetValue(SettingConstants.Download.ALLOW_COST_NETWORK, swDownloadAllowCostNetwork.IsOn);
                m_downloadPageViewModel.UpdateSetting();
            });
            //下载完成发送通知
            swDownloadSendToast.IsOn = SettingService.GetValue<bool>(SettingConstants.Download.SEND_TOAST, false);
            swDownloadSendToast.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingService.SetValue(SettingConstants.Download.SEND_TOAST, swDownloadSendToast.IsOn);
            });
            //下载类型
            var selectedValue = (PlayUrlCodecMode)SettingService.GetValue(SettingConstants.Download.DEFAULT_VIDEO_TYPE, (int)DefaultVideoTypeOptions.DEFAULT_VIDEO_TYPE);
            cbDownloadVideoType.SelectedItem = DefaultVideoTypeOptions.GetOption(selectedValue);
            cbDownloadVideoType.Loaded += (sender, e) =>
            {
                cbDownloadVideoType.SelectionChanged += (obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Download.DEFAULT_VIDEO_TYPE, (int)cbDownloadVideoType.SelectedValue);
                };
            };
            //加载旧版本下载的视频
            swDownloadLoadOld.IsOn = SettingService.GetValue<bool>(SettingConstants.Download.LOAD_OLD_DOWNLOAD, false);
            swDownloadLoadOld.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingService.SetValue(SettingConstants.Download.LOAD_OLD_DOWNLOAD, swDownloadLoadOld.IsOn);
            });
        }
    }
}
