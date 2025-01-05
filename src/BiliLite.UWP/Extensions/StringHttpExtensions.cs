using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;
using System.Net.Http;
using BiliLite.Models.Common;
using BiliLite.Services;
using Flurl.Http;
using BiliLite.Models.Responses;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLite.Extensions
{
    /// <summary>
    /// 网络请求方法封装
    /// </summary>
    public static class StringHttpExtensions
    {
        private static readonly ILogger logger = GlobalLogger.FromCurrentType();

        /// <summary>
        /// 发送一个获取重定向值的get请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static async Task<HttpResults> GetRedirectHttpResultsAsync(this string url, IDictionary<string, string> headers = null,
            IDictionary<string, string> cookies = null)
        {
            Debug.WriteLine("GET:" + url);
            var biliRequestBuilder = new BiliRequestBuilder(url)
                .SetHeaders(headers)
                .SetCookies(cookies)
                .SetNeedRedirect();
            var biliRequest = biliRequestBuilder.Build();
            var httpResult = await biliRequest.Send();
            return httpResult;
        }

        /// <summary>
        /// 发送get请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="cookies"></param>
        /// <returns></returns>
        public static async Task<HttpResults> GetHttpResultsAsync(this string url, IDictionary<string, string> headers = null,
            IDictionary<string, string> cookies = null)
        {
            Debug.WriteLine("GET:" + url);
            var biliRequestBuilder = new BiliRequestBuilder(url)
                .SetHeaders(headers)
                .SetCookies(cookies);
            var biliRequest = biliRequestBuilder.Build();
            var httpResult = await biliRequest.Send();
            return httpResult;
        }


        /// <summary>
        /// 发送一个获取重定向值的get请求,且带上Cookie
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static async Task<HttpResults> GetRedirectHttpResultsWithWebCookie(this string url, IDictionary<string, string> headers = null)
        {
            try
            {
                var cookies = await GetCookies();
                return await url.GetRedirectHttpResultsAsync(headers, cookies);
            }
            catch (Exception ex)
            {
                logger.Log("GET请求失败" + url, LogType.Error, ex);
                return new HttpResults()
                {
                    code = ex.HResult,
                    status = false,
                    message = "网络请求出现错误(GET)"
                };
            }
        }

        /// <summary>
        /// 发送get请求,且带上Cookie
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="cookies"></param>
        /// <returns></returns>
        public static async Task<HttpResults> GetHttpResultsWithWebCookie(this string url, IDictionary<string, string> headers = null, IDictionary<string, string> extraCookies = null)
        {
            try
            {
                var cookies = await GetCookies();

                if (extraCookies != null)
                {
                    foreach (var kvp in extraCookies.ToList())
                    {
                        cookies[kvp.Key] = kvp.Value;
                    }
                }

                return await url.GetHttpResultsAsync(headers, cookies);
            }
            catch (Exception ex)
            {
                logger.Log("GET请求失败" + url, LogType.Error, ex);
                return new HttpResults()
                {
                    code = ex.HResult,
                    status = false,
                    message = "网络请求出现错误(GET)"
                };
            }
        }

        /// <summary>
        /// Get请求，返回Stream
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static async Task<Stream> GetStream(this string url, IDictionary<string, string> headers = null)
        {
            Debug.WriteLine("GET:" + url);
            try
            {
                var stream = await url.WithHeaders(headers).GetAsync().ReceiveStream();
                return stream;
            }
            catch (Exception ex)
            {
                logger.Log("GET请求Stream失败" + url, LogType.Error, ex);
                return null;
            }
        }

        /// <summary>
        /// Get请求，返回buffer
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static async Task<IBuffer> GetBuffer(this string url, IDictionary<string, string> headers = null)
        {
            Debug.WriteLine("GET:" + url);
            try
            {
                var response = await url.WithHeaders(headers).GetAsync();
                var bytes = await response.GetBytesAsync();
                var stream = new MemoryStream(bytes, 0, bytes.Length, true, true);
                var buffer = stream.GetWindowsRuntimeBuffer();
                return buffer;
            }
            catch (Exception ex)
            {
                logger.Log("GET请求Buffer失败" + url, LogType.Error, ex);
                return null;
            }
        }

        /// <summary>
        /// Get请求，返回字符串
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public static async Task<string> GetString(this string url, IDictionary<string, string> headers = null, IDictionary<string, string> cookie = null)
        {
            Debug.WriteLine("GET:" + url);
            try
            {
                var result = await url.WithHeaders(headers).GetAsync().ReceiveString();
                return result;
            }
            catch (Exception ex)
            {
                logger.Log("GET请求String失败" + url, LogType.Error, ex);
                return null;
            }
        }

        /// <summary>
        /// 发送一个POST请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="body"></param>
        /// <param name="headers"></param>
        /// <param name="cookie"></param>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public static async Task<HttpResults> PostHttpResultsAsync(this string url, string body, Dictionary<string, object> formBody, IDictionary<string, string> headers = null, IDictionary<string, string> cookies = null)
        {
            Debug.WriteLine("POST:" + url + "\r\nBODY:" + body);
            var biliRequestBuilder = new BiliRequestBuilder(url)
                .SetHeaders(headers)
                .SetCookies(cookies)
                .SetPostBody(body, formBody);
            var biliRequest = biliRequestBuilder.Build();
            var httpResult = await biliRequest.Send();
            return httpResult;
        }

        public static async Task<HttpResults> PostHttpResultsWithCookie(this string url, string body, Dictionary<string, object> formBody, IDictionary<string, string> headers = null, IDictionary<string, string> extraCookies = null)
        {
            try
            {
                var cookieService = App.ServiceProvider.GetRequiredService<CookieService>();
                var cookies = cookieService.Cookies;
                //没有Cookie
                if (cookies == null || cookies.Count == 0)
                {
                    //访问一遍bilibili.com
                    var getCookieResult = await Constants.BILIBILI_DOMAIN.GetHttpResultsAsync();
                    cookieService.Cookies = getCookieResult.cookies;
                }
                cookies = cookieService.Cookies;
                var cookiesCollection = cookies.ToDictionary(x => x.Name, x => x.Value);

                if (extraCookies != null)
                {
                    foreach (var kvp in extraCookies)
                    {
                        cookiesCollection.Add(kvp.Key, kvp.Value);
                    }
                }

                return await url.PostHttpResultsAsync(body, formBody, headers, cookiesCollection);
            }
            catch (Exception ex)
            {
                logger.Log("GET请求失败" + url, LogType.Error, ex);
                return new HttpResults()
                {
                    code = ex.HResult,
                    status = false,
                    message = "网络请求出现错误(POST)"
                };
            }
        }

        public static async Task<bool> CheckVideoUrlValidAsync(this string url,string userAgent,string referer)
        {
            try
            {
                logger.Debug($"url:{url},referer:{referer},User-Agent:{userAgent}");
                // 使用 Flurl 发送 GET 请求
                var response = await url
                    .WithHeader("referer", referer)
                    .WithHeader("User-Agent", userAgent)
                    .WithTimeout(TimeSpan.FromSeconds(15)) // 设置超时时间
                    .GetAsync(completionOption: HttpCompletionOption.ResponseHeadersRead); // 只读取响应头

                // 如果状态码是 2xx，则认为地址有效
                if (response.StatusCode >= 200 && response.StatusCode < 300)
                {
                    // 立即取消请求
                    response.Dispose();
                    return true;
                }

                // 如果状态码不是 2xx，则认为地址无效
                return false;
            }
            catch (FlurlHttpException ex)
            {
                // 捕获 Flurl 的 HTTP 异常
                if (ex.StatusCode.HasValue)
                {
                    // 如果有状态码，返回 false
                    return false;
                }

                // 其他异常情况（如超时、网络错误等）
                return false;
            }
            catch (Exception)
            {
                // 其他异常情况
                return false;
            }
        }

        private static async Task<Dictionary<string, string>> GetCookies()
        {
            var cookieService = App.ServiceProvider.GetRequiredService<CookieService>();
            var cookies = cookieService.Cookies;
            //没有Cookie
            if (cookies == null || cookies.Count == 0)
            {
                //访问一遍bilibili.com拿Cookie
                var getCookieResult = await Constants.BILIBILI_DOMAIN.GetHttpResultsAsync();
                cookieService.Cookies = getCookieResult.cookies;
            }
            cookies = cookieService.Cookies;
            var cookiesCollection = cookies.ToDictionary(x => x.Name, x => x.Value);
            return cookiesCollection;
        }
    }
}
