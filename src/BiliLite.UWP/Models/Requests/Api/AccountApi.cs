using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Services;
using System;
using BiliLite.Models.Common.User;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLite.Models.Requests.Api
{
    public class AccountApi : BaseApi
    {
        private readonly CookieService m_cookieService;

        public AccountApi()
        {
            m_cookieService = App.ServiceProvider.GetRequiredService<CookieService>();
        }

        /// <summary>
        /// 读取登录密码加密信息
        /// </summary>
        /// <returns></returns>
        public ApiModel GetKey()
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://passport.bilibili.com/x/passport-login/web/key"
            };
            return api;
        }

        /// <summary>
        /// 登录API V2
        /// </summary>
        /// <returns></returns>
        public ApiModel LoginV2(string username, string password, string captcha = "", ApiKeyInfo appKey = null)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = "https://passport.bilibili.com/api/oauth2/login",
                body = $"username={Uri.EscapeDataString(username)}&password={Uri.EscapeDataString(password)}{(captcha == "" ? "" : "&captcha=" + captcha)}&" + ApiHelper.MustParameter(appKey)
            };
            api.body += ApiHelper.GetSign(api.body, appKey);
            return api;
        }

        /// <summary>
        /// 登录API V3
        /// </summary>
        /// <returns></returns>
        public ApiModel LoginV3(string username, string password, ApiKeyInfo appKey)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = "https://passport.bilibili.com/x/passport-login/oauth2/login",
                body = $"actionKey=appkey&channel=bili&device=phone&permission=ALL&subid=1&username={Uri.EscapeDataString(username)}&password={Uri.EscapeDataString(password)}&" + ApiHelper.MustParameter(appKey),
                headers = ApiHelper.MustHeader(appKey),
            };
            api.body += ApiHelper.GetSign(api.body, appKey);
            return api;
        }

        /// <summary>s
        /// 获取登录国家地区
        /// </summary>
        /// <returns></returns>
        public ApiModel Country()
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://passport.bilibili.com/x/passport-login/country",
                parameter = ApiHelper.MustParameter(AppKey)
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        public ApiModel SendSMS(string cid, string phone, string sessionId, ApiKeyInfo appKey)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = "https://passport.bilibili.com/x/passport-login/sms/send",
                body = $"actionKey=appkey&cid={cid}&tel={phone}&login_session_id={sessionId}&" + ApiHelper.MustParameter(appKey),
                headers = ApiHelper.MustHeader(appKey),
            };
            api.body += ApiHelper.GetSign(api.body, appKey);
            return api;
        }

        public ApiModel SendSMSWithCaptcha(string cid, string phone, string session_id, string seccode = "", string validate = "", string challenge = "", string recaptchaToken = "", ApiKeyInfo appKey = null)
        {
            if (seccode.Contains("|"))
            {
                seccode = seccode.UrlEncode();
            }
            var buvid = ApiHelper.GetBuvid();
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = "https://passport.bilibili.com/x/passport-login/sms/send",
                body = $"buvid={buvid}&actionKey=appkey&cid={cid}&tel={phone}&login_session_id={session_id}&gee_seccode={seccode}&gee_validate={validate}&gee_challenge={challenge}&recaptcha_token={recaptchaToken}&" + ApiHelper.MustParameter(appKey),
                headers = ApiHelper.MustHeader(appKey),
            };
            api.body += ApiHelper.GetSign(api.body, appKey);
            return api;
        }

        public ApiModel SMSLogin(string cid, string phone, string code, string sessionId, string captchaKey, ApiKeyInfo appKey)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = "https://passport.bilibili.com/x/passport-login/login/sms",
                body = $"actionKey=appkey&cid={cid}&tel={phone}&login_session_id={sessionId}&captcha_key={captchaKey}&code={code}&" + ApiHelper.MustParameter(appKey),
                headers = ApiHelper.MustHeader(appKey),
            };
            api.body += ApiHelper.GetSign(api.body, appKey);
            return api;
        }

        /// <summary>
        /// SSO
        /// </summary>
        /// <param name="accessKey"></param>
        /// <returns></returns>
        public ApiModel SSO(string accessKey)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://passport.bilibili.com/api/login/sso",
                parameter = ApiHelper.MustParameter(AppKey, false) + $"&access_key={accessKey}",
                headers = ApiHelper.GetDefaultHeaders()
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        /// <summary>
        /// 读取验证码
        /// </summary>
        /// <returns></returns>
        public ApiModel Captcha()
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://passport.bilibili.com/captcha",
                headers = ApiHelper.GetDefaultHeaders(),
                parameter = $"ts={TimeExtensions.GetTimestampS()}"
            };
            return api;
        }

        /// <summary>
        /// 读取极验验证码
        /// </summary>
        /// <returns></returns>
        public ApiModel GeetestCaptcha()
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = "https://passport.bilibili.com/x/safecenter/captcha/pre"
            };
            return api;
        }

        /// <summary>
        /// 获取带星号的手机号
        /// </summary>
        /// <returns></returns>
        public ApiModel FetchHideTel(string tmp_token)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://api.bilibili.com/x/safecenter/user/info",
                parameter = $"tmp_code={tmp_token}"
            };
            return api;
        }

        /// <summary>
        /// 发送验证短信
        /// </summary>
        /// <returns></returns>
        public ApiModel SendVerifySMS(string tmp_token, string recaptcha_token, string gee_challenge, string gee_gt, string geetest_validate, string geetest_seccode)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = "https://passport.bilibili.com/x/safecenter/common/sms/send",
                body = $"sms_type=loginTelCheck&tmp_code={tmp_token}&recaptcha_token={recaptcha_token}&gee_challenge={gee_challenge}&gee_gt={gee_gt}&gee_validate={geetest_validate}&gee_seccode={geetest_seccode}"
            };
            return api;
        }

        /// <summary>
        /// 提交短信验证码
        /// </summary>
        /// <returns></returns>
        public ApiModel SubmitPwdLoginSMSCheck(string code, string tmp_token, string request_id, string captcha_key)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = "https://passport.bilibili.com/x/safecenter/login/tel/verify",
                body = $"type=loginTelCheck&code={code}&tmp_code={tmp_token}&request_id={request_id}&captcha_key={captcha_key}"
            };
            return api;
        }

        /// <summary>
        /// 交换获取cookie
        /// </summary>
        /// <returns></returns>
        public ApiModel PwdLoginExchangeCookie(string code)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = "https://passport.bilibili.com/x/passport-login/web/exchange_cookie",
                body = $"code={code}"
            };
            return api;
        }

        /// <summary>
        /// 个人资料
        /// </summary>
        /// <returns></returns>
        public ApiModel UserProfile()
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://app.bilibili.com/x/v2/account/myinfo",
                parameter = ApiHelper.MustParameter(AppKey, true)
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }


        /// <summary>
        /// 个人资料
        /// </summary>
        /// <returns></returns>
        public ApiModel MineProfile()
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://app.bilibili.com/x/v2/account/mine",
                parameter = ApiHelper.MustParameter(AppKey, true)
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        /// <summary>
        /// 个人空间
        /// </summary>
        /// <returns></returns>
        public ApiModel Space(string mid)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "http://app.biliapi.net/x/v2/space",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&vmid={mid}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        public ApiModel History(int pn = 1, int ps = 24)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/history",
                parameter = ApiHelper.MustParameter(AppKey, true) + $"&pn={pn}&ps={ps}"
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        public ApiModel HistoryWbi(HistoryCursor cursor)
        {
            if (cursor == null)
            {
                cursor = new HistoryCursor()
                {
                    Max = 0,
                    ViewAt = 0,
                };
            }
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/web-interface/history/cursor",
                parameter = $"max={cursor.Max}&view_at={cursor.ViewAt}&business=",
                need_cookie = true,
            };
            return api;
        }

        public ApiModel SearchHistory(string keyword, int page)
        {
            var kw = keyword.UrlEncode();
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/web-interface/history/search",
                parameter = $"pn={page}&ps=20&keyword={kw}&business=all",
                need_cookie = true,
            };
            return api;
        }

        public ApiModel DelHistory(string id)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = $"{ApiHelper.API_BASE_URL}/x/v2/history/delete",
                body = ApiHelper.MustParameter(AppKey, true) + $"&kid={id}"
            };
            api.body += ApiHelper.GetSign(api.body, AppKey);
            return api;
        }

        /// <summary>
        /// tv版二维码登录获取二维码及AuthCode
        /// </summary>
        /// <param name="mid"></param>
        /// <returns></returns>
        public ApiModel QRLoginAuthCodeTV(string local_id, ApiKeyInfo appkey)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = "https://passport.bilibili.com/x/passport-tv-login/qrcode/auth_code",
                body = $"appkey={appkey.Appkey}&local_id={local_id}&ts={TimeExtensions.GetTimestampS()}&mobi_app=ios",
                headers = ApiHelper.MustHeader(appkey),
            };
            api.body += ApiHelper.GetSign(api.body, appkey);
            return api;
        }

        /// <summary>
        /// tv版二维码登录轮询
        /// </summary>
        /// <param name="auth_code"></param>
        /// <returns></returns>
        public ApiModel QRLoginPollTV(string auth_code, string local_id, ApiKeyInfo appkey)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = "https://passport.bilibili.com/x/passport-tv-login/qrcode/poll",
                body = $"appkey={appkey.Appkey}&auth_code={auth_code}&local_id={local_id}&ts={TimeExtensions.GetTimestampS()}&mobi_app=ios",
                headers = ApiHelper.MustHeader(appkey),
            };
            api.body += ApiHelper.GetSign(api.body, appkey);
            return api;
        }

        /// <summary>
        /// web二维码登录获取二维码及AuthCode
        /// </summary>
        /// <returns></returns>
        public ApiModel QRLoginAuthCode()
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://passport.bilibili.com/x/passport-login/web/qrcode/generate",
            };
            return api;
        }

        /// <summary>
        /// web版二维码登录轮询
        /// </summary>
        /// <param name="auth_code"></param>
        /// <returns></returns>
        public ApiModel QRLoginPoll(string auth_code)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://passport.bilibili.com/x/passport-login/web/qrcode/poll",
                parameter = $"qrcode_key={auth_code}"
            };
            return api;
        }

        /// <summary>
        /// web版登录获取到的Cookie转app令牌
        /// </summary>
        /// <returns></returns>
        public ApiModel GetCookieToAccessKey(ApiKeyInfo appKey)
        {
            var apiBody = "api=http://link.acg.tv/forum.php";
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://passport.bilibili.com/login/app/third",
                parameter = $"appkey={appKey.Appkey}&{apiBody}",
                need_cookie = true,
            };
            api.parameter += ApiHelper.GetSign(apiBody, appKey);
            return api;
        }

        /// <summary>
        /// web版登录获取到的Cookie转app令牌
        /// </summary>
        /// <returns></returns>
        public ApiModel GetCookieToAccessKey(string url)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = url,
                need_cookie = true,
                need_redirect = true,
            };
            return api;
        }

        /// <summary>
        /// 读取oauth2信息
        /// </summary>
        /// <returns></returns>
        public ApiModel GetOAuth2Info()
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://passport.bilibili.com/api/oauth2/info",
                parameter = ApiHelper.MustParameter(AppKey) + "&access_token=" + SettingService.Account.AccessKey
            };
            api.parameter += ApiHelper.GetSign(api.parameter, AppKey);
            return api;
        }

        /// <summary>
        /// 刷新Token
        /// </summary>
        /// <returns></returns>
        public ApiModel RefreshToken(ApiKeyInfo appKey)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = "https://passport.bilibili.com/api/oauth2/refreshToken",
                body = ApiHelper.MustParameter(appKey) + $"&access_token={SettingService.Account.AccessKey}&refresh_token={SettingService.GetValue(SettingConstants.Account.REFRESH_KEY, "")}"
            };
            api.body += ApiHelper.GetSign(api.body, appKey);
            return api;
        }

        /// <summary>
        /// 检查Cookie是否需要刷新
        /// </summary>
        /// <returns></returns>
        public ApiModel CheckCookies()
        {
            var csrf = m_cookieService.GetCSRFToken();
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://passport.bilibili.com/x/passport-login/web/cookie/info",
                parameter = $"csrf={csrf}",
                need_cookie = true,
            };
            return api;
        }

        /// <summary>
        /// 刷新CSRF
        /// </summary>
        /// <returns></returns>
        public ApiModel RefreshCsrf(string correspondPath)
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"https://www.bilibili.com/correspond/1/{correspondPath}",
                need_cookie = true,
            };
            return api;
        }

        /// <summary>
        /// 刷新Cookie
        /// </summary>
        /// <returns></returns>
        public ApiModel RefreshCookie(string refreshCsrf,string refreshToken)
        {
            var csrf = m_cookieService.GetCSRFToken();
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = $"https://passport.bilibili.com/x/passport-login/web/cookie/refresh",
                body = $"csrf={csrf}&refresh_csrf={refreshCsrf}&source=main_web&refresh_token={refreshToken}",
                need_cookie = true,
            };
            return api;
        }

        /// <summary>
        /// 确认更新Cookie
        /// </summary>
        /// <returns></returns>
        public ApiModel ConfirmRefreshCookie(string refreshToken)
        {
            var csrf = m_cookieService.GetCSRFToken();
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = $"https://passport.bilibili.com/x/passport-login/web/confirm/refresh",
                body = $"csrf={csrf}&refresh_token={refreshToken}",
                need_cookie = true,
            };
            return api;
        }

        public ApiModel Nav()
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://api.bilibili.com/x/web-interface/nav"
            };
            return api;
        }

        public ApiModel Buvid()
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://api.bilibili.com/x/web-frontend/getbuvid"
            };
            return api;
        }

        public ApiModel Buvid4()
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://api.bilibili.com/x/frontend/finger/spi"
            };
            return api;
        }

        public ApiModel BNut()
        {
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = "https://www.bilibili.com/",
                need_cookie = true,
            };
            return api;
        }

        public ApiModel BiliTicket()
        {
            var csrf = m_cookieService.GetCSRFToken();
            var ts = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
            var hexSign = ApiHelper.GetHMACSHA256("XgwSnGZ1p", $"ts{ts}");
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = "https://api.bilibili.com/bapis/bilibili.api.ticket.v1.Ticket/GenWebTicket",
                parameter= $"key_id=ec02&hexsign={hexSign}&context[ts]={ts}&csrf={csrf}",
                need_cookie = true,
            };
            return api;
        }

        public ApiModel CaptchaRegister(string voucher)
        {
            var csrf = m_cookieService.GetCSRFToken();
            var api = new ApiModel()
            {
                method = HttpMethods.Post,
                baseUrl = "https://api.bilibili.com/x/gaia-vgate/v1/register",
                body = $"csrf={csrf}&v_voucher={voucher}",
                need_cookie = true,
            };
            return api;
        }
    }
}
