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
using NSDanmaku;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Player.ShakaPlayer
{
    public sealed partial class ShakaPlayerControl : UserControl, IDisposable
    {
        private bool m_hasLoaded = false;
        private bool m_webViewLoaded = false;

        public ShakaPlayerControl()
        {
            this.InitializeComponent();
        }

        public double Volume { get; set; } = 1;

        public event EventHandler<double> PositionChanged;

        public event EventHandler<ShakaPlayerLoadedData> PlayerLoaded;

        public event EventHandler Ended;

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
            // TODO: 挂载下载目录
            //WebViewElement.CoreWebView2.SetVirtualHostNameToFolderMapping("videolibs.bililte.service", "",
            //    CoreWebView2HostResourceAccessKind.Allow);
            WebViewElement.CoreWebView2.SetVirtualHostNameToFolderMapping("temp.bililte.service", tempFolder.Path,
                CoreWebView2HostResourceAccessKind.Allow);

            if (!SettingService.GetValue(SettingConstants.Player.WEB_PLAYER_ENABLE_DEV_MODE,
                    false))
            {
                WebViewElement.CoreWebView2.SetVirtualHostNameToFolderMapping("www.bilibili.com",
                    shakaAssetsFolder.Path,
                    CoreWebView2HostResourceAccessKind.Allow);
            }

            if (SettingService.GetValue(SettingConstants.Player.WEB_PLAYER_ENABLE_DEBUG_MODE,
                    false))
            {
                WebViewElement.CoreWebView2.Settings.AreDevToolsEnabled = true;
                WebViewElement.CoreWebView2.OpenDevToolsWindow();
            }

            WebViewElement.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
            m_webViewLoaded = true;
        }

        private void CoreWebView2_WebMessageReceived(CoreWebView2 sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            var json = args.WebMessageAsJson;
            if (json == null) return;
            var @event = JsonConvert.DeserializeObject<BaseShakaPlayerEventMessage>(json);
            if (@event.Event == ShakaPlayerEventLists.POSITION_CHANGED)
            {
                if (@event.Data is int)
                {
                    PositionChanged?.Invoke(this, @event.Data.ToDouble());
                }

                PositionChanged?.Invoke(this, (double)@event.Data);
            }
            else if (@event.Event == ShakaPlayerEventLists.LOADED)
            {
                var data = JsonConvert.DeserializeObject<ShakaPlayerLoadedData>(
                    JsonConvert.SerializeObject(@event.Data));
                PlayerLoaded?.Invoke(this, data);
                m_hasLoaded = true;
            }
            else if (@event.Event == ShakaPlayerEventLists.ENDED)
            {
                Ended?.Invoke(this, EventArgs.Empty);
            }
            else if (@event.Event == ShakaPlayerEventLists.VOLUME_CHANGED)
            {
                if (@event.Data is int)
                {
                    Volume = @event.Data.ToDouble();
                }

                Volume = (double)@event.Data;
            }
        }

        public async Task LoadLiveUrl(string videoUrl, string mode)
        {
            if (!m_webViewLoaded)
            {
                await Task.Delay(100);
            }
            var playData = new
            {
                video = new
                {
                    url = videoUrl,
                    contentType = mode=="Flv"?"video/x-flv": "application/vnd.apple.mpegurl",
                },
                audio = new
                {
                    url = "",
                }
            };
            var json = JsonConvert.SerializeObject(playData);
            LoadCore(json);
        }

        public async Task LoadUrl(string videoUrl, string audioUrl)
        {
            if (!m_webViewLoaded)
            {
                await Task.Delay(100);
            }
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
            LoadCore(json);
        }

        private void LoadCore(string playDataStr)
        {
            if (!SettingService.GetValue(SettingConstants.Player.WEB_PLAYER_ENABLE_DEV_MODE,
                    false))
            {
                WebViewElement.Source = new Uri($"https://www.bilibili.com/index.html?playData={playDataStr.ToBase64()}");
            }
            else
            {
                WebViewElement.Source = new Uri($"http://www.bilibili.com/index.html?playData={playDataStr.ToBase64()}");
            }
        }

        public async Task SetVolume(double volume)
        {
            if (!m_hasLoaded) return;
            var script = $"window.setVolume({volume})";
            await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
        }

        public async Task<double> GetVolume()
        {
            if (!m_hasLoaded) return 1;
            string script = "window.getVolume()";
            string result = await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);

            if (double.TryParse(result, out double volume))
            {
                return volume;
            }
            return 1;
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

        private void ShakaPlayerControl_OnUnloaded(object sender, RoutedEventArgs e)
        {
            Dispose();
        }
    }
}
