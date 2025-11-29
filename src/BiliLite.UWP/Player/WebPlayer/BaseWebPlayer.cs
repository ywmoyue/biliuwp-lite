using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Services;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;
using BiliLite.Models.Common.Player;
using BiliLite.Player.WebPlayer.Models;
using NSDanmaku;
using System.Threading;

namespace BiliLite.Player.WebPlayer;

public abstract class BaseWebPlayer : Grid, IDisposable
{
    private bool m_hasLoaded = false;
    private bool m_webViewLoaded = false;
    private Grid m_gridElement;
    private const string BaseUrl = "https://www.bilibili.com";
    private const string DevBaseUrl = "http://www.bilibili.com";
    private double m_rate = 1;
    private TaskCompletionSource<byte[]> m_captureTaskCompletionSource;
    private readonly SemaphoreSlim m_captureLock = new SemaphoreSlim(1, 1);
    private string m_localVideoLibsPath;
    private string m_localOldVideoLibsPath;

    private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

    public BaseWebPlayer()
    {
        m_gridElement = this;
        m_gridElement.Background = new SolidColorBrush(Colors.Black);
    }

    public abstract string PlayerView { get; }

    public abstract RealPlayerType Type { get; }

    public PlayEngine PlayEngine => Type == RealPlayerType.ShakaPlayer ? PlayEngine.ShakaPlayer : PlayEngine.Mpegts;

    public WebView2 WebViewElement { get; set; }

    public double Volume { get; set; } = 1;

    public Grid GridElement => m_gridElement;

    public event EventHandler<double> PositionChanged;

    public event EventHandler<ShakaPlayerLoadedData> PlayerLoaded;

    public event EventHandler Ended;

    public event EventHandler<WebPlayerStatsUpdatedData> StatsUpdated;

    private async Task InitWebView2()
    {
        WebViewElement = new WebView2();
        WebViewElement.Background = new SolidColorBrush(Colors.Black);
        // 由于加载白屏，需要设置WebViewElement高度为0，loaded事件后再恢复默认高度
        // TODO： 这个做法效果不大，需要设置一个遮罩在loaded事件后隐藏该遮罩
        WebViewElement.Height = 0;
        m_gridElement.Children.Add(WebViewElement);

        var tempFolder = ApplicationData.Current.TemporaryFolder;
        var dataFolder = ApplicationData.Current.LocalFolder;
        var installFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
        var assetsFolder = await installFolder.GetFolderAsync("Assets");
        var shakaAssetsFolder = await assetsFolder.GetFolderAsync("ShakaPlayer");
        await WebViewElement.EnsureCoreWebView2Async();
        // 挂载下载目录
        if (m_localVideoLibsPath != null)
        {
            _logger.Info($"挂载下载目录：{m_localVideoLibsPath},{m_localOldVideoLibsPath}");
            try
            {
                WebViewElement.CoreWebView2.SetVirtualHostNameToFolderMapping("videolibs.bililte.service",
                    m_localVideoLibsPath,
                    CoreWebView2HostResourceAccessKind.Allow);
                WebViewElement.CoreWebView2.SetVirtualHostNameToFolderMapping("oldvideolibs.bililte.service",
                    m_localOldVideoLibsPath,
                    CoreWebView2HostResourceAccessKind.Allow);
            }
            catch (Exception ex)
            {
                _logger.Error($"挂载下载目录失败", ex);
                throw new Exception("挂载下载目录失败");
            }
        }

        // 挂载临时目录
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
            if (@event.Data is int or long)
            {
                PositionChanged?.Invoke(this, @event.Data.ToDouble());
            }

            PositionChanged?.Invoke(this, (double)@event.Data);
        }
        else if (@event.Event == ShakaPlayerEventLists.LOADED)
        {
            WebViewElement.Height = double.NaN;
            var data = JsonConvert.DeserializeObject<ShakaPlayerLoadedData>(
                JsonConvert.SerializeObject(@event.Data));
            PlayerLoaded?.Invoke(this, data);
            m_hasLoaded = true;
            SetSyncThreshold();
            SetVolume(Volume);
            SetRate(m_rate);
        }
        else if (@event.Event == ShakaPlayerEventLists.ENDED)
        {
            Ended?.Invoke(this, EventArgs.Empty);
        }
        else if (@event.Event == ShakaPlayerEventLists.STATS)
        {
            var data = JsonConvert.DeserializeObject<WebPlayerStatsUpdatedData>(
                JsonConvert.SerializeObject(@event.Data));
            StatsUpdated?.Invoke(this, data);
        }
        else if (@event.Event == ShakaPlayerEventLists.VOLUME_CHANGED)
        {
            if (@event.Data is int or long)
            {
                Volume = @event.Data.ToDouble();
            }

            Volume = (double)@event.Data;
        }
        else if (@event.Event == ShakaPlayerEventLists.CAPTURE_IMAGE_DATA)
        {
            var data = JsonConvert.DeserializeObject<WebPlayerCaptureVideoImageData>(
                JsonConvert.SerializeObject(@event.Data));
            ReceiveCaptureVideoFrame(data);
        }
    }

    private string GetPlayerUrl(string playDataStr)
    {
        var url = "";
        if (!SettingService.GetValue(SettingConstants.Player.WEB_PLAYER_ENABLE_DEV_MODE,
                false))
        {
            url = $"{BaseUrl}/index.html?view={PlayerView}&playData={playDataStr.ToBase64()}";
        }
        else
        {
            url = $"{DevBaseUrl}/index.html?view={PlayerView}&playData={playDataStr.ToBase64()}";
        }

        return url;
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
                contentType = mode == "Flv" ? "video/x-flv" : "application/vnd.apple.mpegurl",
            },
            audio = new
            {
                url = "",
            }
        };
        var json = JsonConvert.SerializeObject(playData);
        await LoadCore(json);
    }

    public async Task LoadUrl(string videoUrl, string audioUrl, bool isLocal = false)
    {
        if (!m_webViewLoaded)
        {
            await Task.Delay(100);
        }

        if (isLocal)
        {
            var folder = await DownloadHelper.GetDownloadFolder();
            m_localVideoLibsPath = folder.Path;
            var oldFolder = await DownloadHelper.GetDownloadOldFolder();
            m_localOldVideoLibsPath = oldFolder.Path;

            videoUrl = videoUrl
                .Replace(folder.Path, "https://videolibs.bililte.service")
                .Replace(oldFolder.Path, "https://oldvideolibs.bililte.service")
                .Replace("\\", "/");

            audioUrl = audioUrl
                .Replace(folder.Path, "https://videolibs.bililte.service")
                .Replace(oldFolder.Path, "https://oldvideolibs.bililte.service")
                .Replace("\\", "/");
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
        await LoadCore(json);
    }

    private async Task LoadCore(string playDataStr)
    {
        if (WebViewElement == null)
            await InitWebView2();
        WebViewElement.Source = new Uri(GetPlayerUrl(playDataStr));
    }

    public async Task SetVolume(double volume)
    {
        if (!m_hasLoaded)
        {
            Volume = volume;
            return;
        }
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

    public async Task SetSyncThreshold()
    {
        var diff1 = SettingService.GetValue(
            SettingConstants.Player.WEB_PLAYER_AV_POSITION_SYNC_THRESHOLD_1,
            SettingConstants.Player.DEFAULT_WEB_PLAYER_AV_POSITION_SYNC_THRESHOLD_1);
        var diff2 = SettingService.GetValue(
            SettingConstants.Player.WEB_PLAYER_AV_POSITION_SYNC_THRESHOLD_2,
            SettingConstants.Player.DEFAULT_WEB_PLAYER_AV_POSITION_SYNC_THRESHOLD_2);
        string script = $"window.setCheckAVSyncDiff({diff1},{diff2})";
        await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
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
        m_rate = speed;
        if (!m_hasLoaded) return;
        var script = $"window.setRate({speed})";
        await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
    }

    public void Stop()
    {
        WebViewElement?.Close();
        WebViewElement = null;
    }

    public async Task<byte[]> CaptureVideo()
    {
        if (!m_hasLoaded) return null;

        await m_captureLock.WaitAsync();

        try
        {
            m_captureTaskCompletionSource = new TaskCompletionSource<byte[]>();

            var script = $"window.captureVideo()";
            await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);

            return await m_captureTaskCompletionSource.Task;
        }
        finally
        {
            m_captureLock.Release();
        }
    }

    private void ReceiveCaptureVideoFrame(WebPlayerCaptureVideoImageData data)
    {
        if (m_captureTaskCompletionSource == null) return;
        try
        {
            // 将Base64字符串转换为字节数组
            if (!string.IsNullOrEmpty(data.ImageData))
            {
                // 删除Base64前缀（如"data:image/png;base64,"）
                var base64Data = data.ImageData.Split(',')[1];
                var bytes = Convert.FromBase64String(base64Data);
                m_captureTaskCompletionSource.TrySetResult(bytes);
            }
            else
            {
                m_captureTaskCompletionSource.TrySetResult(null);
            }
        }
        catch (Exception ex)
        {
            m_captureTaskCompletionSource.TrySetException(ex);
        }
    }

    public void Dispose()
    {
        Stop();
    }

    public async Task FlipVertical()
    {
        if (!m_hasLoaded) return;
        var script = $"window.flipVertical()";
        await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
    }

    public async Task FlipHorizontal()
    {
        if (!m_hasLoaded) return;
        var script = $"window.flipHorizontal()";
        await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
    }

    public async Task Zoom(double scaleFactor)
    {
        if (!m_hasLoaded) return;
        var script = $"window.zoom({scaleFactor})";
        await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
    }

    public async Task Move(double x, double y)
    {
        if (!m_hasLoaded) return;
        var script = $"window.move({x},{y})";
        await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
    }

    public async Task ResetTransforms()
    {
        if (!m_hasLoaded) return;
        var script = $"window.resetTransforms()";
        await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
    }

    public async Task TogglePictureInPicture()
    {
        if (!m_hasLoaded) return;
        var script = $"window.togglePictureInPicture()";
        await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
    }

    public void OpenDevMode()
    {
        try
        {
            WebViewElement.CoreWebView2.Settings.AreDevToolsEnabled = true;
            WebViewElement.CoreWebView2.OpenDevToolsWindow();
        }
        catch (Exception ex)
        {

        }
    }
}