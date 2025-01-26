using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Services;
using Microsoft.Toolkit.Uwp.Helpers;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls.Settings
{
    public sealed partial class AboutSettingsControl : UserControl
    {
        public AboutSettingsControl()
        {
            InitializeComponent();
        }

        private async void btnCheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            await BiliExtensions.CheckVersion();
        }

        private void btnCleanUpdateIgnore_Click(object sender, RoutedEventArgs e)
        {
            SettingService.SetValue(SettingConstants.Other.IGNORE_VERSION, "");
        }

        private async void AboutSettingsControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                version.Text = $"版本 {SystemInformation.ApplicationVersion.Major}.{SystemInformation.ApplicationVersion.Minor}.{SystemInformation.ApplicationVersion.Build}.{SystemInformation.ApplicationVersion.Revision}";
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
