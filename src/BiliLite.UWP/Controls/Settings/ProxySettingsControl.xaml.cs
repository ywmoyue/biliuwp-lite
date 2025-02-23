using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Services;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls.Settings
{
    public sealed partial class ProxySettingsControl : UserControl
    {
        public ProxySettingsControl()
        {
            InitializeComponent();
            LoadRoaming();
        }

        private void LoadRoaming()
        {
            //使用自定义服务器
            RoamingSettingSetDefault.Click += RoamingSettingSetDefault_Click;
            RoamingSettingCustomServer.Text = SettingService.GetValue<string>(SettingConstants.Roaming.CUSTOM_SERVER_URL, ApiHelper.ROMAING_PROXY_URL);
            RoamingSettingCustomServer.Loaded += new RoutedEventHandler((sender, e) =>
            {
                RoamingSettingCustomServer.QuerySubmitted += RoamingSettingCustomServer_QuerySubmitted;
            });

            //自定义HK服务器
            RoamingSettingCustomServerHK.Text = SettingService.GetValue<string>(SettingConstants.Roaming.CUSTOM_SERVER_URL_HK, "");
            RoamingSettingCustomServerHK.Loaded += new RoutedEventHandler((sender, e) =>
            {
                RoamingSettingCustomServerHK.QuerySubmitted += new TypedEventHandler<AutoSuggestBox, AutoSuggestBoxQuerySubmittedEventArgs>((sender2, args) =>
                {
                    var text = sender2.Text;
                    if (string.IsNullOrEmpty(text))
                    {
                        NotificationShowExtensions.ShowMessageToast("已取消自定义香港代理服务器");
                        SettingService.SetValue<string>(SettingConstants.Roaming.CUSTOM_SERVER_URL_HK, "");
                        return;
                    }
                    if (!text.Contains("http"))
                    {
                        text = "https://" + text;
                    }
                    SettingService.SetValue<string>(SettingConstants.Roaming.CUSTOM_SERVER_URL_HK, text);
                    sender2.Text = text;
                    NotificationShowExtensions.ShowMessageToast("保存成功");
                });
            });

            //自定义TW服务器
            RoamingSettingCustomServerTW.Text = SettingService.GetValue<string>(SettingConstants.Roaming.CUSTOM_SERVER_URL_TW, "");
            RoamingSettingCustomServerTW.Loaded += new RoutedEventHandler((sender, e) =>
            {
                RoamingSettingCustomServerTW.QuerySubmitted += new TypedEventHandler<AutoSuggestBox, AutoSuggestBoxQuerySubmittedEventArgs>((sender2, args) =>
                {
                    var text = sender2.Text;
                    if (string.IsNullOrEmpty(text))
                    {
                        NotificationShowExtensions.ShowMessageToast("已取消自定义台湾代理服务器");
                        SettingService.SetValue<string>(SettingConstants.Roaming.CUSTOM_SERVER_URL_TW, "");
                        return;
                    }
                    if (!text.Contains("http"))
                    {
                        text = "https://" + text;
                    }
                    SettingService.SetValue<string>(SettingConstants.Roaming.CUSTOM_SERVER_URL_TW, text);
                    sender2.Text = text;
                    NotificationShowExtensions.ShowMessageToast("保存成功");
                });
            });

            //自定义大陆服务器
            RoamingSettingCustomServerCN.Text = SettingService.GetValue<string>(SettingConstants.Roaming.CUSTOM_SERVER_URL_CN, "");
            RoamingSettingCustomServerCN.Loaded += new RoutedEventHandler((sender, e) =>
            {
                RoamingSettingCustomServerCN.QuerySubmitted += new TypedEventHandler<AutoSuggestBox, AutoSuggestBoxQuerySubmittedEventArgs>((sender2, args) =>
                {
                    var text = sender2.Text;
                    if (string.IsNullOrEmpty(text))
                    {
                        NotificationShowExtensions.ShowMessageToast("已取消自定义大陆代理服务器");
                        SettingService.SetValue<string>(SettingConstants.Roaming.CUSTOM_SERVER_URL_CN, "");
                        return;
                    }
                    if (!text.Contains("http"))
                    {
                        text = "https://" + text;
                    }
                    SettingService.SetValue<string>(SettingConstants.Roaming.CUSTOM_SERVER_URL_CN, text);
                    sender2.Text = text;
                    NotificationShowExtensions.ShowMessageToast("保存成功");
                });
            });

            //Akamai
            //RoamingSettingAkamaized.IsOn = SettingService.GetValue<bool>(SettingConstants.Roaming.AKAMAI_CDN, false);
            //RoamingSettingAkamaized.Loaded += new RoutedEventHandler((sender, e) =>
            //{
            //    RoamingSettingAkamaized.Toggled += new RoutedEventHandler((obj, args) =>
            //    {
            //        SettingService.SetValue(SettingConstants.Roaming.AKAMAI_CDN, RoamingSettingAkamaized.IsOn);
            //    });
            //});
            //转简体
            RoamingSettingToSimplified.IsOn = SettingService.GetValue<bool>(SettingConstants.Roaming.TO_SIMPLIFIED, true);
            RoamingSettingToSimplified.Loaded += new RoutedEventHandler((sender, e) =>
            {
                RoamingSettingToSimplified.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingService.SetValue(SettingConstants.Roaming.TO_SIMPLIFIED, RoamingSettingToSimplified.IsOn);
                });
            });

        }

        private void RoamingSettingSetDefault_Click(object sender, RoutedEventArgs e)
        {
            SettingService.SetValue<string>(SettingConstants.Roaming.CUSTOM_SERVER_URL, ApiHelper.ROMAING_PROXY_URL);
            RoamingSettingCustomServer.Text = ApiHelper.ROMAING_PROXY_URL;
            NotificationShowExtensions.ShowMessageToast("保存成功");
        }

        private void RoamingSettingCustomServer_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var text = sender.Text;
            if (text.Length == 0 || !text.Contains("."))
            {
                NotificationShowExtensions.ShowMessageToast("输入服务器链接有误");
                sender.Text = SettingService.GetValue<string>(SettingConstants.Roaming.CUSTOM_SERVER_URL, ApiHelper.ROMAING_PROXY_URL);
                return;
            }
            if (!text.Contains("http"))
            {
                text = "https://" + text;
            }
            SettingService.SetValue<string>(SettingConstants.Roaming.CUSTOM_SERVER_URL, text);
            sender.Text = text;
            NotificationShowExtensions.ShowMessageToast("保存成功");
        }
    }
}
