using BiliLite.Extensions;
using BiliLite.Extensions.Notifications;
using BiliLite.Models;
using BiliLite.Models.Common;
using BiliLite.Models.Exceptions;
using BiliLite.Models.Requests.Api;
using BiliLite.Services;
using BiliLite.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Windows.Input;

namespace BiliLite.Modules.User
{
    public class LoginVM : IModules
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        public Account account;
        Timer smsTimer;
        Timer qrTimer;
        AccountApi accountApi;
        public LoginVM()
        {
            account = new Account();
            accountApi = new AccountApi();
            Countries = new ObservableCollection<CountryItemModel>();
            LoginTypeCommand = new RelayCommand<int>(ChangeLoginType);
            SendSMSCommand = new RelayCommand(SendSMSCode);
            SendPwdLoginSMSCommand = new RelayCommand(SendPwdLoginVerifySMS);
            RefreshQRCommand = new RelayCommand(RefreshQRCode);
            smsTimer = new Timer(1000);
            smsTimer.Elapsed += SmsTimer_Elapsed;
        }
        public event EventHandler<Uri> OpenWebView;
        public event EventHandler<bool> SetWebViewVisibility;
        public event EventHandler CloseDialog;
        public ICommand LoginTypeCommand { get; private set; }
        public ICommand SendSMSCommand { get; private set; }
        public ICommand SendPwdLoginSMSCommand { get; private set; }
        public ICommand RefreshQRCommand { get; private set; }

        private int loginType = 1;
        /// <summary>
        /// 登录类型
        /// 0=账号密码，1=短信登录，2=二维码登录
        /// </summary>
        public int LoginType
        {
            get { return loginType; }
            set { loginType = value; DoPropertyChanged("LoginType"); }
        }

        private string title = "短信登录";
        public string Title
        {
            get { return title; }
            set { title = value; DoPropertyChanged("Title"); }
        }

        private bool _primaryButtonEnable = true;

        public bool PrimaryButtonEnable
        {
            get { return _primaryButtonEnable; }
            set { _primaryButtonEnable = value; DoPropertyChanged("PrimaryButtonEnable"); }
        }



        public void ChangeLoginType(int type)
        {
            LoginType = type;

            switch (type)
            {
                case 0:
                    Title = "密码登录";
                    if (qrTimer != null)
                    {
                        qrTimer.Stop();
                        qrTimer.Dispose();
                    }
                    NotificationShowExtensions.ShowMessageToast("为了您的账号安全,建议扫描二维码登录");
                    break;
                case 1:
                    Title = "短信登录";
                    if (qrTimer != null)
                    {
                        qrTimer.Stop();
                        qrTimer.Dispose();
                    }
                    break;
                case 2:
                    Title = "二维码登录";
                    GetQRAuthInfo();
                    break;
                case 3:
                    Title = "短信验证";
                    if (qrTimer != null)
                    {
                        qrTimer.Stop();
                        qrTimer.Dispose();
                    }
                    break;
                default:
                    break;
            }
        }

        public void DoLogin()
        {
            switch (loginType)
            {
                case 0:
                    DoPasswordLogin();
                    break;
                case 1:
                    DoSMSLogin();
                    break;
                case 3:
                    CompletePasswordLoginCheck();
                    break;
                default:
                    break;
            }
        }

        public async void ValidateLogin(JObject jObject)
        {
            try
            {
                if (jObject["access_token"] != null)
                {
                    var appKey = SettingConstants.Account.DefaultLoginAppKeySecret;
                    var m = await account.SaveLogin(
                        jObject["access_token"].ToString(),
                        jObject["refresh_token"].ToString(),
                        jObject["expires_in"].ToInt32(),
                        Convert.ToInt64(jObject["mid"].ToString()),
                        null, null,
                        appKey);

                    if (m)
                    {
                        CloseDialog?.Invoke(this, null);
                        NotificationShowExtensions.ShowMessageToast("登录成功");
                    }
                    else
                    {
                        PrimaryButtonEnable = true;
                        SetWebViewVisibility?.Invoke(this, false);
                        NotificationShowExtensions.ShowMessageToast("登录失败,请重试");
                    }
                    // await UserManage.LoginSucess(jObject["access_token"].ToString());
                }
                else
                {
                    PrimaryButtonEnable = true;
                    SetWebViewVisibility?.Invoke(this, false);
                    NotificationShowExtensions.ShowMessageToast("登录失败,请重试");
                }

            }
            catch (Exception ex)
            {
                _logger.Log("登录二次验证失败", LogType.Error, ex);
            }
        }

        #region 短信登录
        private ObservableCollection<CountryItemModel> _countries;
        public ObservableCollection<CountryItemModel> Countries
        {
            get { return _countries; }
            set { _countries = value; DoPropertyChanged("Countries"); }
        }
        private CountryItemModel currentCountry;
        public CountryItemModel CurrentCountry
        {
            get { return currentCountry; }
            set { currentCountry = value; DoPropertyChanged("CurrentCountry"); }
        }
        private string phone;
        public string Phone
        {
            get { return phone; }
            set { phone = value; DoPropertyChanged("Phone"); }
        }

        private string code;
        public string Code
        {
            get { return code; }
            set { code = value; DoPropertyChanged("Code"); }
        }

        private bool enableSendSMS = true;
        public bool EnableSendSMS
        {
            get { return enableSendSMS; }
            set { enableSendSMS = value; DoPropertyChanged("EnableSendSMS"); }
        }


        private int _SMSCountDown = 60;
        public int SMSCountDown
        {
            get { return _SMSCountDown; }
            set { _SMSCountDown = value; DoPropertyChanged("SMSCountDown"); }
        }


        public async Task LoadCountry()
        {
            try
            {

                var results = await accountApi.Country().Request();
                if (results.status)
                {
                    var data = await results.GetJson<ApiDataModel<JObject>>();
                    if (data.success)
                    {
                        var ls = JsonConvert.DeserializeObject<ObservableCollection<CountryItemModel>>(data.data["list"].ToString());
                        if (ls != null && ls.Count > 0)
                        {
                            Countries = ls;
                            CurrentCountry = Countries.First();
                        }
                    }
                    else
                    {
                        throw new CustomizedErrorWithDataException(data.message, data);
                    }
                }
                else
                {
                    throw new CustomizedErrorWithDataException(results.message, results);
                }
            }
            catch (CustomizedErrorWithDataException ex)
            {
                _logger.Error($"{ex.Message}|{ex.DataText}", ex);
                NotificationShowExtensions.ShowMessageToast(ex.Message);
            }
            catch (Exception ex)
            {
                var handel = HandelError<LoginVM>(ex);
                NotificationShowExtensions.ShowMessageToast(handel.message);
            }
        }

        private async void SmsTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (SMSCountDown <= 1)
                {
                    EnableSendSMS = true;
                    smsTimer.Stop();
                }
                else
                {
                    SMSCountDown--;
                }
            });
        }
        string sessionId = "";
        string captchaKey = "";
        public async void SendSMSCode()
        {
            if (CurrentCountry == null)
            {
                NotificationShowExtensions.ShowMessageToast("请选择国家/地区");
                return;
            }
            if (Phone.Length == 0)
            {
                NotificationShowExtensions.ShowMessageToast("请输入手机号");
                return;
            }

            try
            {
                var appKey = SettingConstants.Account.DefaultLoginAppKeySecret;
                sessionId = Guid.NewGuid().ToString().Replace("-", "");
                var results = await accountApi.SendSMS(CurrentCountry.country_code, Phone, sessionId, appKey).Request();
                if (!results.status)
                {
                    throw new CustomizedErrorWithDataException(results.message, results);
                }

                var data = await results.GetJson<ApiDataModel<SMSResultModel>>();
                if (!data.success)
                {
                    throw new CustomizedErrorWithDataException(data.message, data);
                }

                if (!string.IsNullOrEmpty(data.data.recaptcha_url))
                {
                    var uri = new Uri(data.data.recaptcha_url);
                    SetWebViewVisibility?.Invoke(this, true);
                    OpenWebView?.Invoke(this,
                        new Uri("ms-appx-web:///Assets/GeeTest/bili_gt.html" + uri.Query + "&app=uwp"));
                }
                else
                {
                    captchaKey = data.data.captcha_key;
                    EnableSendSMS = false;
                    //验证码发送成功，倒计时
                    SMSCountDown = 60;
                    smsTimer.Start();
                }
            }
            catch (CustomizedErrorWithDataException ex)
            {
                _logger.Error($"{ex.Message}|{ex.DataText}", ex);
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                EnableSendSMS = true;
            }
            catch (Exception ex)
            {
                _logger.Error($"{ex.Message}", ex);
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                EnableSendSMS = true;
            }
        }

        public async void SendSMSCodeWithCaptcha(string seccode = "", string validate = "", string challenge = "", string recaptcha_token = "")
        {

            try
            {
                var appKey = SettingConstants.Account.DefaultLoginAppKeySecret;
                var request = accountApi.SendSMSWithCaptcha(CurrentCountry.country_code, Phone, sessionId,
                    seccode, validate, challenge, recaptcha_token, appKey);
                var results = await request.Request();
                if (!results.status)
                {
                    throw new CustomizedErrorWithDataException(results.message, results);
                }

                var data = await results.GetJson<ApiDataModel<SMSResultModel>>();
                if (!data.success)
                {
                    throw new CustomizedErrorWithDataException(data.message, data);
                }

                if (!string.IsNullOrEmpty(data.data.recaptcha_url))
                {
                    var uri = new Uri(data.data.recaptcha_url);
                    SetWebViewVisibility?.Invoke(this, true);
                    OpenWebView?.Invoke(this,
                        new Uri("ms-appx-web:///Assets/GeeTest/bili_gt.html" + uri.Query + "&app=uwp"));
                }
                else
                {
                    captchaKey = data.data.captcha_key;
                    //验证码发送成功，倒计时
                    EnableSendSMS = false;
                    SMSCountDown = 60;
                    smsTimer.Start();
                }
            }
            catch (CustomizedErrorWithDataException ex)
            {
                _logger.Error($"SMS登录错误：{ex.Message}|{ex.DataText}", ex);
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                EnableSendSMS = true;
            }
            catch (Exception ex)
            {
                _logger.Error($"SMS登录错误：{ex.Message}", ex);
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                EnableSendSMS = true;
            }
        }

        private async void DoSMSLogin()
        {
            if (CurrentCountry == null)
            {
                NotificationShowExtensions.ShowMessageToast("请选择国家/地区");
                return;
            }
            if (Phone.Length == 0)
            {
                NotificationShowExtensions.ShowMessageToast("请输入手机号");
                return;
            }
            if (Code.Length == 0)
            {
                NotificationShowExtensions.ShowMessageToast("请输入验证码");
                return;
            }
            try
            {
                var appKey = SettingConstants.Account.DefaultLoginAppKeySecret;
                var results = await accountApi.SMSLogin(CurrentCountry.country_code, Phone, Code, sessionId, captchaKey, appKey).Request();
                if (results.status)
                {
                    SettingService.SetValue(SettingConstants.Account.IS_WEB_LOGIN, false);
                    var data = await results.GetData<LoginResultModel>();
                    var result = await HandelLoginResult(data.code, data.message, data.data, appKey);
                    HandleResult(result);
                }
                else
                {
                    throw new CustomizedErrorWithDataException(results.message, results);
                }

            }
            catch (CustomizedErrorWithDataException ex)
            {
                _logger.Error($"SMS登录错误：{ex.Message}|{ex.DataText}", ex);
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                EnableSendSMS = true;
            }
            catch (Exception ex)
            {
                _logger.Error($"SMS登录错误：{ex.Message}", ex);
                NotificationShowExtensions.ShowMessageToast(ex.Message);
                EnableSendSMS = true;
            }
        }


        #endregion

        #region 密码登录

        private string userName;

        public string UserName
        {
            get { return userName; }
            set { userName = value; DoPropertyChanged("UserName"); }
        }

        private string password;
        public string Password
        {
            get { return password; }
            set { password = value; DoPropertyChanged("Password"); }
        }

        private async void DoPasswordLogin()
        {
            if (UserName.Length == 0)
            {
                NotificationShowExtensions.ShowMessageToast("请输入用户名");
                return;
            }
            if (Password.Length == 0)
            {
                NotificationShowExtensions.ShowMessageToast("请输入密码");
                return;
            }
            PrimaryButtonEnable = false;

            try
            {
                var appKey = SettingConstants.Account.DefaultLoginAppKeySecret;
                var pwd = await account.EncryptedPassword(Password);
                var results = await accountApi.LoginV3(UserName, pwd, appKey).Request();
                if (results.status)
                {
                    SettingService.SetValue(SettingConstants.Account.IS_WEB_LOGIN, false);
                    var data = await results.GetData<LoginResultModel>();
                    var result = await HandelLoginResult(data.code, data.message, data.data, appKey);

                    HandleResult(result);
                }
                else
                {
                    throw new CustomizedErrorWithDataException(results.message, results);
                }
            }
            catch (CustomizedErrorWithDataException ex)
            {
                _logger.Error($"密码登录错误：{ex.Message}|{ex.DataText}", ex);
                NotificationShowExtensions.ShowMessageToast(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error($"密码登录错误：{ex.Message}", ex);
                NotificationShowExtensions.ShowMessageToast(ex.Message);
            }
            finally
            {
                PrimaryButtonEnable = true;
            }
        }

        private async void CompletePasswordLoginCheck()
        {
            if (Code.Length == 0)
            {
                NotificationShowExtensions.ShowMessageToast("请输入验证码");
                return;
            }
            PrimaryButtonEnable = false;
            try
            {
                var req = await accountApi.SubmitPwdLoginSMSCheck(Code, gee_tmp_token, gee_request_id, captchaKey).Request();
                if (!req.status)
                {
                    throw new CustomizedErrorWithDataException(req.message, req);
                }

                var obj = req.GetJObject();
                if (obj["code"].ToInt32() != 0)
                {
                    throw new CustomizedErrorWithDataException(obj["message"].ToString(), req);
                }

                var code = obj["data"]["code"].ToString();
                var result = await PasswordLoginFetchCookie(code);
                HandleResult(result);
            }
            catch (CustomizedErrorWithDataException ex)
            {
                _logger.Error($"{ex.Message}|{ex.DataText}", ex);
                NotificationShowExtensions.ShowMessageToast(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error($"{ex.Message}", ex);
                NotificationShowExtensions.ShowMessageToast(ex.Message);
            }
            finally
            {
                PrimaryButtonEnable = true;
                EnableSendSMS = true;
            }
        }

        private async Task<LoginCallbackModel> PasswordLoginFetchCookie(string code)
        {
            try
            {
                var req = await accountApi.PwdLoginExchangeCookie(code).Request();
                if (req.status)
                {
                    var obj = req.GetJObject();
                    if (obj["code"].ToInt32() == 0)
                    {
                        var refresh_token = obj["data"]["refresh_token"].ToString();
                        account.LoginByCookie(req.cookies, refresh_token);
                        return new LoginCallbackModel()
                        {
                            status = LoginStatus.Success,
                            message = "登录成功"
                        };
                    }
                    else
                    {
                        return new LoginCallbackModel()
                        {
                            status = LoginStatus.Fail,
                            message = obj["message"].ToString()
                        };
                    }
                }
                else
                {
                    return new LoginCallbackModel()
                    {
                        status = LoginStatus.Fail,
                        message = req.message
                    };
                }
            }
            catch (Exception ex)
            {
                return new LoginCallbackModel()
                {
                    status = LoginStatus.Fail,
                    message = ex.Message
                };
            }
            finally
            {
                PrimaryButtonEnable = true;
                EnableSendSMS = true;
            }
        }
        #endregion

        #region 二维码登录

        private bool qrLoadding;

        public bool QRLoadding
        {
            get { return qrLoadding; }
            set { qrLoadding = value; DoPropertyChanged("QRLoadding"); }
        }

        private Microsoft.UI.Xaml.Media.ImageSource qrImageSource;

        public Microsoft.UI.Xaml.Media.ImageSource QRImageSource
        {
            get { return qrImageSource; }
            set { qrImageSource = value; DoPropertyChanged("QRImageSource"); }
        }


        QRAuthInfo qrAuthInfo;
        private async void GetQRAuthInfo()
        {
            try
            {
                QRLoadding = true;
                if (qrTimer != null)
                {
                    qrTimer.Stop();
                    qrTimer.Dispose();
                }
                var result = await account.GetQRAuthInfoTV(ApiHelper.IPadOsKey);
                if (result.success)
                {
                    qrAuthInfo = result.data;

                    var qrCodeService = App.ServiceProvider.GetRequiredService<IQrCodeService>();
                    var img = await qrCodeService.GenerateQrCode(qrAuthInfo.url);

                    QRImageSource = img;
                    qrTimer = new Timer();
                    qrTimer.Interval = 3000;
                    qrTimer.Elapsed += QrTimer_Elapsed;
                    qrTimer.Start();
                }
                else
                {
                    NotificationShowExtensions.ShowMessageToast(result.message);
                }

            }
            catch (Exception ex)
            {
                _logger.Log("读取和加载登录二维码失败", LogType.Error, ex);
                NotificationShowExtensions.ShowMessageToast("加载二维码失败");
            }
            finally
            {
                QRLoadding = false;
            }

        }

        private void RefreshQRCode()
        {
            if (QRLoadding)
                return;
            GetQRAuthInfo();
        }


        private async void QrTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                var result = await account.PollQRAuthInfoTV(qrAuthInfo.auth_code, ApiHelper.IPadOsKey);
                if (result.status == LoginStatus.Success)
                {
                    qrTimer.Stop();
                    CloseDialog?.Invoke(this, null);
                }
            });
        }

        #endregion

        private GeetestRequestModel gee_req;
        private string gee_tmp_token;
        private string gee_request_id;

        private string hideTel = "";
        public string HideTel
        {
            get { return hideTel; }
            set { hideTel = value; DoPropertyChanged("HideTel"); }
        }

        public void HandleGeetestSuccess(string seccode, string validate, string challenge, string recaptcha_token)
        {
            if (gee_req != null)
            {
                // 验证完成后的 gee_challenge 值可能与之前获取的不同，此处只做提示
                if (gee_req.gee_challenge != challenge)
                {
                    NotificationShowExtensions.ShowMessageToast("验证码失效");
                }
                gee_req.gee_challenge = challenge;
                gee_req.gee_validate = validate;
                gee_req.gee_seccode = seccode;
            }
            switch (LoginType)
            {
                //切换到短信验证界面并发送验证码
                case 0:
                    FetchHideTelNumber();
                    ChangeLoginType(3);
                    SendPwdLoginVerifySMS();
                    break;
                //发送短信
                case 1:
                    SendSMSCodeWithCaptcha(seccode, validate, challenge, recaptcha_token);
                    break;
                default:
                    break;
            }
        }

        private async void FetchHideTelNumber()
        {
            try
            {
                var req = await accountApi.FetchHideTel(gee_tmp_token).Request();
                if (req.status)
                {
                    var obj = req.GetJObject();
                    if (obj["code"].ToInt32() == 0)
                    {
                        HideTel = obj["data"]["account_info"]["hide_tel"].ToString();
                    }
                    else
                    {
                        NotificationShowExtensions.ShowMessageToast(obj["message"].ToString());
                        return;
                    }
                }
                else
                {
                    NotificationShowExtensions.ShowMessageToast(req.message);
                }
            }
            catch (Exception ex)
            {
                _logger.Log("获取验证手机号失败", LogType.Error, ex);
            }
        }

        private async void SendPwdLoginVerifySMS()
        {
            try
            {
                var req = await accountApi.SendVerifySMS(gee_tmp_token, gee_req.recaptcha_token, gee_req.gee_challenge, gee_req.gee_gt, gee_req.gee_validate, gee_req.gee_seccode).Request();
                if (req.status)
                {
                    var obj = req.GetJObject();
                    if (obj["code"].ToInt32() == 0)
                    {
                        captchaKey = obj["data"]["captcha_key"].ToString();
                        EnableSendSMS = false;
                        //验证码发送成功，倒计时
                        SMSCountDown = 60;
                        smsTimer.Start();
                    }
                    else
                    {
                        NotificationShowExtensions.ShowMessageToast(obj["message"].ToString());
                        return;
                    }
                }
                else
                {
                    NotificationShowExtensions.ShowMessageToast(req.message);
                }
            }
            catch (Exception ex)
            {
                _logger.Log("发送短信验证码失败", LogType.Error, ex);
            }
        }

        private async Task<LoginCallbackModel> HandelLoginResult(int code, string message, LoginResultModel result, ApiKeyInfo appkey)
        {
            if (code == 0)
            {
                if (result.status == 0)
                {
                    await account.SaveLogin(result.token_info.access_token, result.token_info.refresh_token, result.token_info.expires_in, result.token_info.mid, result.sso, result.cookie_info, appkey);
                    return new LoginCallbackModel()
                    {
                        status = LoginStatus.Success,
                        message = "登录成功"
                    };
                }
                if (result.status == 1 || result.status == 2)
                {
                    var query = HttpUtility.ParseQueryString(new Uri(result.url).Query);
                    gee_tmp_token = query.Get("tmp_token");
                    gee_request_id = query.Get("request_id");
                    gee_req = await StartGeetestCaptcha();
                    return new LoginCallbackModel()
                    {
                        status = LoginStatus.NeedCaptcha,
                        message = "本次登录需要安全验证",
                        url = $"http://fake/?gee_gt={gee_req.gee_gt}&gee_challenge={gee_req.gee_challenge}"
                    };
                }
                return new LoginCallbackModel()
                {
                    status = LoginStatus.Fail,
                    message = result.message
                };
            }
            else if (code == -105)
            {
                return new LoginCallbackModel()
                {
                    status = LoginStatus.NeedCaptcha,
                    url = result.url,
                    message = "登录需要验证码"
                };
            }
            else
            {
                return new LoginCallbackModel()
                {
                    status = LoginStatus.Fail,
                    message = message
                };
            }

        }
        private void HandleResult(LoginCallbackModel result)
        {
            switch (result.status)
            {
                case LoginStatus.Success:
                    CloseDialog?.Invoke(this, null);
                    break;
                case LoginStatus.Fail:
                case LoginStatus.Error:
                    PrimaryButtonEnable = true;
                    NotificationShowExtensions.ShowMessageToast(result.message);
                    break;
                case LoginStatus.NeedCaptcha:
                    var uri = new Uri(result.url);
                    SetWebViewVisibility?.Invoke(this, true);
                    var query = HttpUtility.ParseQueryString(uri.Query);
                    gee_req = new GeetestRequestModel()
                    {
                        gee_challenge = query.Get("gee_challenge"),
                        gee_gt = query.Get("gee_gt"),
                        recaptcha_token = query.Get("recaptcha_token"),
                    };
                    // TODO: 密码登录验证需要获取tmp_code
                    gee_tmp_token = "";
                    //验证码重定向
                    //源码:https://github.com/xiaoyaocz/some_web
                    OpenWebView?.Invoke(this, new Uri("ms-appx-web:///Assets/GeeTest/bili_gt.html" + uri.Query + "&app=uwp"));
                    break;
                case LoginStatus.NeedValidate:
                    SetWebViewVisibility?.Invoke(this, true);
                    OpenWebView?.Invoke(this, new Uri(result.url));
                    break;
                default:
                    break;
            }
        }

        private async Task<GeetestRequestModel> StartGeetestCaptcha()
        {
            try
            {
                var req = await accountApi.GeetestCaptcha().Request();
                var obj = req.GetJObject();
                if (req.status && obj["code"].ToInt32() == 0)
                {
                    var data = JsonConvert.DeserializeObject<GeetestRequestModel>(obj["data"].ToString());
                    return data;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.Log("读取极验验证码失败", LogType.Error, ex);
                return null;
            }
        }
    }

    public class CountryItemModel
    {
        public int id { get; set; }
        public string country_code { get; set; }
        public string countryCode { get { return country_code; } }
        public string cname { get; set; }
    }
    public class SMSResultModel
    {
        public bool is_new { get; set; }
        public string captcha_key { get; set; }

        public string recaptcha_url { get; set; }
    }

    public class LoginResultModel
    {
        public int status { get; set; }
        public string message { get; set; }
        public List<string> sso { get; set; }
        public string url { get; set; }
        public LoginResultTokenModel token_info { get; set; }
        public LoginCookieInfo cookie_info { get; set; }
    }
    public class LoginResultTokenModel
    {
        public long mid { get; set; }
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
    }

    public class GeetestRequestModel
    {
        public string recaptcha_token { get; set; }
        public string gee_challenge { get; set; }
        public string gee_gt { get; set; }
        public string gee_validate { get; set; }
        public string gee_seccode { get; set; }
    }
}