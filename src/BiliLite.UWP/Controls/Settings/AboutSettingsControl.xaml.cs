using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Services;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls.Settings
{
    public sealed partial class AboutSettingsControl : UserControl
    {
        public AboutSettingsControl()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement button = sender as FrameworkElement;
            switch (button.Tag as string)
            {
                case "btnCheckUpdate":
                    await BiliExtensions.CheckVersion(this);
                    break;
                case "btnCleanUpdateIgnore":
                    SettingService.SetValue(SettingConstants.Other.IGNORE_VERSION, "");
                    break;
            }

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
