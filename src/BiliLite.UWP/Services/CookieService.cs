using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Web.Http.Filters;
using BiliLite.Extensions;
using BiliLite.Models;
using BiliLite.Models.Common;
using BiliLite.Models.Requests.Api;
using Newtonsoft.Json;

namespace BiliLite.Services
{
    public class CookieService
    {
        private List<HttpCookieItem> m_cookies;

        public CookieService()
        {
            var cookiesStr = SettingService.GetValue(SettingConstants.Account.BILIBILI_COOKIES, "");
            m_cookies = new List<HttpCookieItem>();
            if (string.IsNullOrEmpty(cookiesStr))
            {
                // 兼容旧版本
                m_cookies = GetOldVersionCookies();
                return;
            }
            var cookies = JsonConvert.DeserializeObject<List<HttpCookieItem>>(cookiesStr);
            if (cookies.FirstOrDefault()?.Expires > DateTimeOffset.Now)
            {
                m_cookies = cookies;
            }
        }

        public List<HttpCookieItem> Cookies
        {
            get => m_cookies;
            set
            {
                SaveCookies(value);
                m_cookies = value;
            }
        }

        public string BiliTicket { get; set; }

        private List<HttpCookieItem> GetOldVersionCookies()
        {
            var filter = new HttpBaseProtocolFilter();
            var cookies = filter.CookieManager.GetCookies(new Uri(Constants.GET_COOKIE_DOMAIN));
            if (cookies == null || cookies.Count == 0) return null;
            return cookies.Select(x => new HttpCookieItem()
            {
                Domain = x.Domain,
                Expires = x.Expires,
                HttpOnly = x.HttpOnly,
                Name = x.Name,
                Secure = x.Secure,
                Value = x.Value
            }).ToList();
        }

        private void ClearOldVersionCookies()
        {
            var domains = new string[] {
                "http://bilibili.com",
                "http://biligame.com",
                "http://bigfun.cn",
                "http://bigfunapp.cn",
                "http://dreamcast.hk",
                Constants.GET_COOKIE_DOMAIN,
            };
            //删除Cookie
            var httpBaseProtocolFilter = new HttpBaseProtocolFilter();
            foreach (var domain in domains)
            {
                var cookies = httpBaseProtocolFilter.CookieManager.GetCookies(new Uri(domain));
                foreach (var item in cookies)
                {
                    httpBaseProtocolFilter.CookieManager.DeleteCookie(item);
                }
            }
        }

        private void SaveCookies(List<HttpCookieItem> cookies)
        {
            if (cookies == null)
            {
                return;
            }

            var cookiesStr = JsonConvert.SerializeObject(cookies);
            SettingService.SetValue(SettingConstants.Account.BILIBILI_COOKIES, cookiesStr);
        }

        public string GetCSRFToken()
        {
            //没有Cookie
            if (Cookies == null || Cookies.Count == 0)
            {
                throw new Exception("未登录");
            }

            var csrf = Cookies.FirstOrDefault(x => x.Name == "bili_jct")?.Value;

            if (string.IsNullOrEmpty(csrf))
            {
                throw new Exception("未登录");
            }

            return csrf;
        }

        public async Task CheckCookieKeys()
        {
            BiliTicket = SettingService.GetValue("BiliTicket", "");
            if (string.IsNullOrEmpty(BiliTicket))
            {
                var api = new AccountApi().BiliTicket();
                var result = await api.Request();
                var data  = result.GetJObject();
                BiliTicket = data["data"]["ticket"].ToString();
            }
            if (!Cookies.Any(x => x.Name == "buvid3"))
            {
                var api = new AccountApi().Buvid();
                var result = await api.Request();
                var data = result.GetJObject();
                m_cookies.Add(new HttpCookieItem()
                {
                    Name = "buvid3",
                    Value = data["data"]["buvid"].ToString()
                });
                SaveCookies(m_cookies);
            }
            if (!Cookies.Any(x => x.Name == "buvid4"))
            {
                var api = new AccountApi().Buvid4();
                var result = await api.Request();
                var data = result.GetJObject();
                m_cookies.Add(new HttpCookieItem()
                {
                    Name = "buvid4",
                    Value = data["data"]["b_4"].ToString()
                });
                var buvid3 = m_cookies.FirstOrDefault(x => x.Name == "buvid3");
                buvid3.Value = data["data"]["b_3"].ToString();
                SaveCookies(m_cookies);
            }
            if (!Cookies.Any(x => x.Name == "b_nut"))
            {
                var api = new AccountApi().BNut();
                var result = await api.Request();
                foreach (var httpCookieItem in result.cookies)
                {
                    if (httpCookieItem.Name == "b_nut")
                    {
                        m_cookies.Add(httpCookieItem);
                    }
                    if (httpCookieItem.Name == "buvid3 ")
                    {
                        var buvid3 = m_cookies.FirstOrDefault(x => x.Name == "buvid3");
                        buvid3.Value = httpCookieItem.Value;
                    }
                }
                SaveCookies(m_cookies);
            }
        }

        public void ClearCookies()
        {
            Cookies = new List<HttpCookieItem>();
            ClearOldVersionCookies();
        }
    }
}
