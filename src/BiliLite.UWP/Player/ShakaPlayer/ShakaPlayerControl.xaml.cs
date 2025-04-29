using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using BiliLite.Player.ShakaPlayer.Models;
using Newtonsoft.Json;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Player.ShakaPlayer
{
    public sealed partial class ShakaPlayerControl : UserControl
    {
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
            await WebViewElement.EnsureCoreWebView2Async();
            WebViewElement.CoreWebView2.SetVirtualHostNameToFolderMapping("videolibs.bililte.service", "C:\\Users\\muyan\\Videos\\哔哩哔哩下载",
                CoreWebView2HostResourceAccessKind.Allow);
            WebViewElement.CoreWebView2.SetVirtualHostNameToFolderMapping("temp.bililte.service", tempFolder.Path,
                CoreWebView2HostResourceAccessKind.Allow);
            WebViewElement.CoreWebView2.SetVirtualHostNameToFolderMapping("www.bilibili.com", Path.Combine(dataFolder.Path, "shakaPlayer"),
                CoreWebView2HostResourceAccessKind.Allow);

            WebViewElement.CoreWebView2.Settings.AreDevToolsEnabled = true;
            WebViewElement.CoreWebView2.OpenDevToolsWindow();

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
            }
        }

        public async Task LoadUrl(string mpdUrl)
        {
            WebViewElement.Source = new Uri($"https://www.bilibili.com/index.html?mpdUrl={mpdUrl}");
        }

        public async Task Pause()
        {
            var script = "window.pause()";
            await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
        }

        public async Task Resume()
        {
            var script = "window.resume()";
            await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
        }

        public async Task Seek(double position)
        {
            var script = $"window.seek({position})";
            await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
        }

        public async Task SetRate(double speed)
        {
            var script = $"window.setRate({speed})";
            await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
        }
    }
}
