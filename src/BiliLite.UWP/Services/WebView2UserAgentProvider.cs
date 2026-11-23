using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BiliLite.Services
{
    public static class WebView2UserAgentProvider
    {
        private static readonly SemaphoreSlim m_initializeLock = new SemaphoreSlim(1, 1);
        private static readonly ILogger m_logger = GlobalLogger.FromCurrentType();
        private static bool m_initialized;
        private static string m_desktopUserAgent = FallbackDesktopUserAgent;

        public const string FallbackDesktopUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/148.0.0.0 Safari/537.36";

        public static string DesktopUserAgent => m_desktopUserAgent;

        public static async Task InitializeAsync()
        {
            if (m_initialized)
            {
                return;
            }

            await m_initializeLock.WaitAsync();
            try
            {
                if (m_initialized)
                {
                    return;
                }

                try
                {
                    var webView = new WebView2();
                    await webView.EnsureCoreWebView2Async();

                    var userAgent = webView.CoreWebView2?.Settings?.UserAgent;
                    if (!string.IsNullOrWhiteSpace(userAgent))
                    {
                        m_desktopUserAgent = userAgent;
                    }
                }
                catch (Exception ex)
                {
                    m_logger.Warn("初始化 WebView2 User-Agent 失败，使用默认值", ex);
                }

                m_initialized = true;
            }
            finally
            {
                m_initializeLock.Release();
            }
        }
    }
}