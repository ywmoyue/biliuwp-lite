using Microsoft.Web.WebView2.Core;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Player.ShakaPlayer.Models;
using BiliLite.Services;
using Newtonsoft.Json;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Player.ShakaPlayer
{
    public sealed partial class ShakaPlayerControl : UserControl, IDisposable
    {
        private bool m_hasLoaded = false;

        public ShakaPlayerControl()
        {
            this.InitializeComponent();
        }

        public event EventHandler<double> PositionChanged;

        public event EventHandler<ShakaPlayerLoadedData> PlayerLoaded;

        private async void ShakaPlayerControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            await InitWebView2();
        }

        private async Task InitWebView2()
        {
            var tempFolder = ApplicationData.Current.TemporaryFolder;
            var dataFolder = ApplicationData.Current.LocalFolder;
            var installFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var assetsFolder = await installFolder.GetFolderAsync("Assets");
            var shakaAssetsFolder = await assetsFolder.GetFolderAsync("ShakaPlayer");
            await WebViewElement.EnsureCoreWebView2Async();
            WebViewElement.CoreWebView2.SetVirtualHostNameToFolderMapping("videolibs.bililte.service", "C:\\Users\\muyan\\Videos\\哔哩哔哩下载",
                CoreWebView2HostResourceAccessKind.Allow);
            WebViewElement.CoreWebView2.SetVirtualHostNameToFolderMapping("temp.bililte.service", tempFolder.Path,
                CoreWebView2HostResourceAccessKind.Allow);
            WebViewElement.CoreWebView2.SetVirtualHostNameToFolderMapping("www.bilibili.com", shakaAssetsFolder.Path,
                CoreWebView2HostResourceAccessKind.Allow);

            if (SettingService.GetValue(SettingConstants.Player.SHAKA_PLAYER_ENABLE_DEBUG_MODE,
                    false))
            {
                WebViewElement.CoreWebView2.Settings.AreDevToolsEnabled = true;
                WebViewElement.CoreWebView2.OpenDevToolsWindow();
            }

            WebViewElement.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
        }

        private void CoreWebView2_WebMessageReceived(CoreWebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            var json = args.WebMessageAsJson;
            if (json == null) return;
            var @event = JsonConvert.DeserializeObject<BaseShakaPlayerEventMessage>(json);
            if (@event.Event == ShakaPlayerEventLists.POSITION_CHANGED)
            {
                PositionChanged?.Invoke(this, (double)@event.Data);
            }else if (@event.Event == ShakaPlayerEventLists.LOADED)
            {
                var data = JsonConvert.DeserializeObject<ShakaPlayerLoadedData>(
                    JsonConvert.SerializeObject(@event.Data));
                PlayerLoaded?.Invoke(this, data);
                m_hasLoaded = true;
            }
        }

        public async Task LoadUrl(string videoUrl, string audioUrl)
        {
            var playData = new
            {
                video = new
                {
                    url = videoUrl,
                },
                audio = new
                {
                    url = audioUrl,
                }
            };
            var json = JsonConvert.SerializeObject(playData);
            WebViewElement.Source = new Uri($"https://www.bilibili.com/index.html?playData={json.ToBase64()}");
        }

        public async Task Pause()
        {
            if (!m_hasLoaded) return;
            var script = "window.pause()";
            await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
        }

        public async Task Resume()
        {
            if (!m_hasLoaded) return;
            var script = "window.resume()";
            await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
        }

        public async Task Seek(double position)
        {
            if (!m_hasLoaded) return;
            var script = $"window.seek({position})";
            await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
        }

        public async Task SetRate(double speed)
        {
            if (!m_hasLoaded) return;
            var script = $"window.setRate({speed})";
            await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
        }

        public void Dispose()
        {
            WebViewElement.Close();
        }
    }
}
