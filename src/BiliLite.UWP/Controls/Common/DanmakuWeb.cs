using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Windows.Storage;
using BiliLite.Models.Common.Danmaku;
using System.Collections;
using BiliLite.Extensions;
using NSDanmaku.Model;
using Atelier39;

namespace BiliLite.Controls.Common
{
    public class DanmakuWeb : Grid
    {
        private Grid m_gridElement;
        private bool m_hasLoaded = false;

        public DanmakuWeb()
        {
            m_gridElement = this;
            m_gridElement.Background = new SolidColorBrush(Colors.Transparent);
            Loaded += DanmakuWeb_Loaded;
        }

        private async void DanmakuWeb_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await InitWebView2();
        }

        public WebView2 WebViewElement { get; set; }
        public Grid GridElement => m_gridElement;

        public event EventHandler<DanmakuHoverData> BulletHover;
        public event EventHandler DanmakuLoaded;

        public async Task InitWebView2()
        {
            Environment.SetEnvironmentVariable("WEBVIEW2_DEFAULT_BACKGROUND_COLOR", "0");
            WebViewElement = new WebView2();
            WebViewElement.Background = new SolidColorBrush(Colors.Transparent);
            m_gridElement.Children.Add(WebViewElement);

            //var tempFolder = ApplicationData.Current.TemporaryFolder;
            //var installFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            //var assetsFolder = await installFolder.GetFolderAsync("Assets");
            //var danmakuAssetsFolder = await assetsFolder.GetFolderAsync("DanmakuPlayer");

            await WebViewElement.EnsureCoreWebView2Async();
            //WebViewElement.CoreWebView2.SetVirtualHostNameToFolderMapping(
            //    "danmaku.bililite.service",
            //    danmakuAssetsFolder.Path,
            //    CoreWebView2HostResourceAccessKind.Allow);

            WebViewElement.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            WebViewElement.CoreWebView2.Navigate("http://www.bilibili.com/index.html?view=DanmakuPlayerView");

            WebViewElement.CoreWebView2.OpenDevToolsWindow();
            WebViewElement.CoreWebView2.Settings.AreDevToolsEnabled = true;
        }

        private void CoreWebView2_WebMessageReceived(CoreWebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            var json = args.WebMessageAsJson;
            if (json == null) return;

            try
            {
                var @event = JsonConvert.DeserializeObject<BaseDanmakuEventMessage>(json);

                if (@event.Event == DanmakuEventLists.BULLET_HOVER)
                {
                    var data = JsonConvert.DeserializeObject<DanmakuHoverData>(
                    JsonConvert.SerializeObject(@event.Data));

                    BulletHover?.Invoke(this, data);
                }
                else if (@event.Event == DanmakuEventLists.DANMAKU_LOADED)
                {
                    m_hasLoaded = true;
                    DanmakuLoaded?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                // 处理异常
            }
        }

        public async Task LoadDanmaku(List<DanmakuItem> danmakuList, DanmakuConfig config)
        {
            if (!m_hasLoaded)
            {
                await Task.Delay(100);
            }

            var playData = new
            {
                danmakuConfig = config,
                comments = danmakuList.Select(d => new
                {
                    duration = 20000,
                    id = d.GetHashCode(),
                    start = d.StartMs,
                    txt = d.Text,
                    style = new
                    {
                        color = d.TextColor.ToString(),
                        fontSize = $"{d.BaseFontSize}px",
                        border = "none",
                        borderRadius = "0px",
                        padding = "2px 5px",
                        backgroundColor = "transparent"
                    },
                    mode = GetModeString(d.Mode)
                }).ToList()
            };
            var base64 = JsonConvert.SerializeObject(playData.comments).ToBase64();

            var script = $"window.danmaku.updateComments('{base64}')";
            await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
        }

        public async Task SendComment(DanmakuItem danmakuItem)
        {
            if (!m_hasLoaded) return;

            var comment = new
            {
                duration = 20000,
                id = danmakuItem.GetHashCode(),
                start = danmakuItem.StartMs,
                txt = danmakuItem.Text,
                style = new
                {
                    color = danmakuItem.TextColor.ToString(),
                    fontSize = $"{danmakuItem.BaseFontSize}px",
                    border = "none",
                    borderRadius = "0px",
                    padding = "2px 5px",
                    backgroundColor = "transparent"
                },
                mode = GetModeString(danmakuItem.Mode)
            };

            var script = $"window.danmaku.sendComment({JsonConvert.SerializeObject(comment)})";
            await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
        }

        public async Task Clear()
        {
            if (!m_hasLoaded) return;
            var script = "window.danmaku.clear()";
            await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
        }

        public async Task Pause()
        {
            if (!m_hasLoaded) return;
            var script = "window.danmaku.pause()";
            await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
        }

        public async Task Resume()
        {
            if (!m_hasLoaded) return;
            var script = "window.danmaku.play()";
            await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
        }

        public async Task HideMode(string mode)
        {
            if (!m_hasLoaded) return;
            var script = $"window.danmaku.hide('{mode}')";
            await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
        }

        public async Task ShowMode(string mode)
        {
            if (!m_hasLoaded) return;
            var script = $"window.danmaku.show('{mode}')";
            await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
        }

        public async Task SetArea(double start, double end, int? lines = null)
        {
            if (!m_hasLoaded) return;
            var area = new { start, end, lines };
            var script = $"window.danmaku.setArea({JsonConvert.SerializeObject(area)})";
            await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
        }

        public async Task SetOpacity(double opacity)
        {
            if (!m_hasLoaded) return;
            var script = $"window.danmaku.setOpacity({opacity})";
            await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
        }

        public async Task SetFontSize(double size, double? channelSize = null)
        {
            if (!m_hasLoaded) return;
            var script = channelSize.HasValue ?
                $"window.danmaku.setFontSize({size}, {channelSize.Value})" :
                $"window.danmaku.setFontSize({size})";
            await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
        }

        public async Task SetPlayRate(string mode, double rate)
        {
            if (!m_hasLoaded) return;
            var script = $"window.danmaku.setPlayRate('{mode}', {rate})";
            await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
        }

        private string GetModeString(Atelier39.DanmakuMode mode)
        {
            return mode switch
            {
                Atelier39.DanmakuMode.Top => "top",
                Atelier39.DanmakuMode.Bottom => "bottom",
                Atelier39.DanmakuMode.Rolling => "scroll",
                _ => "scroll"
            };
        }
    }

    public class BaseDanmakuEventMessage
    {
        public string Event { get; set; }
        public object Data { get; set; }
    }

    public class DanmakuHoverData
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string Mode { get; set; }
    }

    public static class DanmakuEventLists
    {
        public const string BULLET_HOVER = "bullet_hover";
        public const string DANMAKU_LOADED = "danmaku_loaded";
    }

    public class DanmakuConfig
    {
        public double AreaStart { get; set; } = 0;
        public double AreaEnd { get; set; } = 1;
        public int? Lines { get; set; }
        public int ChannelSize { get; set; } = 40;
        public bool MouseControl { get; set; } = false;
        public bool MouseControlPause { get; set; } = false;
        public bool DefaultOff { get; set; } = false;
        public bool ChaseEffect { get; set; } = true;
    }

    public enum DanmakuMode
    {
        Scroll,
        Top,
        Bottom
    }
}
