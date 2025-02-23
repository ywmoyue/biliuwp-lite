using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models.Common;
using BiliLite.Modules.User;
using BiliLite.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace BiliLite.Dialogs
{
    public sealed partial class LoginDialog : ContentDialog
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();

        JSBridge.biliapp _biliapp = new JSBridge.biliapp();
        JSBridge.secure _secure = new JSBridge.secure();
        private LoginVM loginVM;
        public LoginDialog()
        {
            this.InitializeComponent();
            this.Loaded += SMSLoginDialog_Loaded;
            loginVM = new LoginVM();
            loginVM.OpenWebView += LoginVM_OpenWebView;
            loginVM.CloseDialog += LoginVM_CloseDialog;
            loginVM.SetWebViewVisibility += LoginVM_SetWebViewVisibility;
            _biliapp.CloseBrowserEvent += _biliapp_CloseBrowserEvent;
            _biliapp.ValidateLoginEvent += _biliapp_ValidateLoginEvent;
        }

        private void LoginVM_CloseDialog(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void LoginVM_SetWebViewVisibility(object sender, bool e)
        {
            webView.Visibility = e ? Visibility.Visible : Visibility.Collapsed;
        }

        private void _biliapp_CloseBrowserEvent(object sender, string e)
        {
            this.Hide();
        }
        private void _biliapp_ValidateLoginEvent(object sender, string e)
        {
            loginVM.ValidateLogin(JObject.Parse(e));

        }

        private async void LoginVM_OpenWebView(object sender, Uri e)
        {
            await InitWebView2();
            var templateText = await FileIO.ReadTextAsync(
                await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/GeeTest/bili_gt.cshtml")));

            var result = templateText.Replace("@Model.Url", e.AbsoluteUri);

            //webView.Source = e;
            webView.NavigateToString(result);
        }

        private void SMSLoginDialog_Loaded(object sender, RoutedEventArgs e)
        {
            _ = loginVM.LoadCountry();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            loginVM.DoLogin();
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (webView.Visibility == Visibility.Visible)
            {
                webView.Visibility = Visibility.Collapsed;
                args.Cancel = true;
                return;
            }
        }


        private async Task InitWebView2()
        {
            await webView.EnsureCoreWebView2Async();
        }


        private void txt_Password_GotFocus(object sender, RoutedEventArgs e)
        {
            hide.Visibility = Visibility.Visible;
        }

        private void txt_Password_LostFocus(object sender, RoutedEventArgs e)
        {
            hide.Visibility = Visibility.Collapsed;
        }

        private async void WebView_OnNavigationStarting(WebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        {
            if (args.Uri.Contains("access_key="))
            {
                var access = Regex.Match(args.Uri, "access_key=(.*?)&").Groups[1].Value;
                var mid = Regex.Match(args.Uri, "mid=(.*?)&").Groups[1].Value;
                var appKey = SettingConstants.Account.DefaultLoginAppKeySecret;
                await loginVM.account.SaveLogin(access, "", 0, long.Parse(mid), null, null, appKey);
                this.Hide();
                return;
            }
            if (args.Uri.Contains("geetest.result"))
            {
                var success = (Regex.Match(args.Uri, @"success=(\d)&").Groups[1].Value).ToInt32();
                if (success == 0)
                {
                    //验证失败
                    webView.Visibility = Visibility.Collapsed;
                    NotificationShowExtensions.ShowMessageToast("验证失败");
                }
                else if (success == 1)
                {
                    webView.Visibility = Visibility.Collapsed;
                    //验证成功
                    var challenge = Regex.Match(args.Uri, "geetest_challenge=(.*?)&").Groups[1].Value;
                    var validate = Regex.Match(args.Uri, "geetest_validate=(.*?)&").Groups[1].Value;
                    var seccode = Regex.Match(args.Uri, "geetest_seccode=(.*?)&").Groups[1].Value;
                    var recaptcha_token = Regex.Match(args.Uri, "recaptcha_token=(.*?)&").Groups[1].Value;
                    loginVM.HandleGeetestSuccess(seccode, validate, challenge, recaptcha_token);
                }
                else if (success == 2)
                {
                    //关闭验证码
                    IsPrimaryButtonEnabled = true;

                    webView.Visibility = Visibility.Collapsed;
                }
                return;
            }
            try
            {
                //this.webView.AddWebAllowedObject("biliapp", _biliapp);
                //this.webView.AddWebAllowedObject("secure", _secure);
            }
            catch (Exception ex)
            {
                logger.Log("注入JS对象失败", LogType.Error, ex);
            }
        }

        private async void WebView_OnNavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            if (sender.Source.AbsoluteUri == "https://passport.bilibili.com/ajax/miniLogin/redirect" || sender.Source.AbsoluteUri == "https://www.bilibili.com/")
            {
                var results = await $"https://passport.bilibili.com/login/app/third?appkey=&api=http%3A%2F%2Flink.acg.tv%2Fforum.php&sign=67ec798004373253d60114caaad89a8c".GetString();
                var obj = JObject.Parse(results);
                if (obj["code"].ToInt32() == 0)
                {
                    webView.Source = new Uri(obj["data"]["confirm_uri"].ToString());
                }
                else
                {
                    NotificationShowExtensions.ShowMessageToast("登录失败，请重试");
                }
                return;
            }
        }
    }
}
