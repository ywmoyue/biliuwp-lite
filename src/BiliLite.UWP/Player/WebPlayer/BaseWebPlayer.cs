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
using Windows.UI.Xaml;
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
    // 播放器就绪(LOADED)之前收到的进度请求先暂存，就绪后立刻补上，
    // 否则恢复历史进度/切集续播的 seek 会被直接丢弃，导致从 0 秒开始播放。
    private double m_pendingSeekPosition = double.NaN;
    private Grid m_gridElement;
    private const string BaseUrl = "https://www.bilibili.com";
    private const string DevBaseUrl = "http://www.bilibili.com";
    private double m_rate = 1;
    private TaskCompletionSource<byte[]> m_captureTaskCompletionSource;
    private readonly SemaphoreSlim m_captureLock = new SemaphoreSlim(1, 1);
    private string m_localVideoLibsPath;
    private string m_localOldVideoLibsPath;
    private Border m_loadingMask;

    private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

    public BaseWebPlayer()
    {
        m_gridElement = this;
        m_gridElement.Background = new SolidColorBrush(Colors.Black);
        m_loadingMask = new Border
        {
            Background = new SolidColorBrush(Colors.Black),
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Visibility = Visibility.Collapsed,
            IsHitTestVisible = false,
        };
        m_gridElement.Children.Add(m_loadingMask);
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
        // UWP WebView2 不暴露 DefaultBackgroundColor/CoreWebView2Controller，使用环境变量设置透明底色。
        Environment.SetEnvironmentVariable("WEBVIEW2_DEFAULT_BACKGROUND_COLOR", "00FFFFFF");
        WebViewElement = new WebView2();
        WebViewElement.Background = new SolidColorBrush(Colors.Transparent);
        // 由于加载白屏，需要设置WebViewElement高度为0，loaded事件后再恢复默认高度
        WebViewElement.Height = 0;
        ShowLoadingMask();
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
            HideLoadingMask();
            var data = JsonConvert.DeserializeObject<ShakaPlayerLoadedData>(
                JsonConvert.SerializeObject(@event.Data));
            // 必须先置位再抛出事件：PlayerLoaded 的订阅者（DashShakaSubPlayer → VideoPlayer → 播放状态机）
            // 会在回调内同步完成“离开 Loading”的状态切换，播放器控件随即调用 Seek 恢复历史进度。
            // 若此时 m_hasLoaded 仍为 false，Seek 会被当成“未就绪”直接丢弃，
            // 播放器就只能从 0 秒开始播放。
            m_hasLoaded = true;
            ApplyPendingSeek();
            PlayerLoaded?.Invoke(this, data);
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

        var tempFolder = ApplicationData.Current.TemporaryFolder;

        if (isLocal)
        {
            var folder = await DownloadHelper.GetDownloadFolder();
            m_localVideoLibsPath = folder.Path;
            var oldFolder = await DownloadHelper.GetDownloadOldFolder();
            m_localOldVideoLibsPath = oldFolder.Path;

            videoUrl = videoUrl
                .Replace(folder.Path, "https://videolibs.bililte.service")
                .Replace(oldFolder.Path, "https://oldvideolibs.bililte.service")
                .Replace(tempFolder.Path, "https://temp.bililte.service")
                .Replace("\\", "/");

            audioUrl = audioUrl
                .Replace(folder.Path, "https://videolibs.bililte.service")
                .Replace(oldFolder.Path, "https://oldvideolibs.bililte.service")
                .Replace(tempFolder.Path, "https://temp.bililte.service")
                .Replace("\\", "/");
        }

        // 如果没有设置 isLocal，但 audioUrl 指向临时目录，也需要做映射，保证 WebView 能访问临时音频文件
        if (!string.IsNullOrWhiteSpace(audioUrl) && audioUrl.Contains(tempFolder.Path))
        {
            audioUrl = audioUrl.Replace(tempFolder.Path, "https://temp.bililte.service").Replace("\\", "/");
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
        // 每次加载都会导航到新页面，旧页面里的 window.seek/window.resume 等脚本接口随之失效。
        // 先复位就绪标记，让新页面就绪前到达的进度请求进入队列，而不是对尚未初始化的页面执行后丢失。
        m_hasLoaded = false;
        m_pendingSeekPosition = double.NaN;
        ShowLoadingMask();
        WebViewElement.Source = new Uri(GetPlayerUrl(playDataStr));
    }

    /// <summary>
    /// 播放器就绪后补上加载期间排队的进度请求。
    /// </summary>
    private void ApplyPendingSeek()
    {
        var position = m_pendingSeekPosition;
        m_pendingSeekPosition = double.NaN;
        if (double.IsNaN(position))
        {
            return;
        }

        _logger.Info($"WebPlayer.ApplyPendingSeek: 播放器就绪，补上等待中的进度 position={position}");
        _ = Seek(position);
    }

    private void ShowLoadingMask()
    {
        if (m_loadingMask != null)
        {
            m_loadingMask.Visibility = Visibility.Visible;
        }
    }

    private void HideLoadingMask()
    {
        if (m_loadingMask != null)
        {
            m_loadingMask.Visibility = Visibility.Collapsed;
        }
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
        if (!m_hasLoaded || WebViewElement?.CoreWebView2 == null)
        {
            // 页面还没加载完成时 window.seek 尚不存在，此时执行也不会生效，
            // 所以先记下来，等 LOADED 之后再补上，避免恢复历史进度丢失。
            m_pendingSeekPosition = position;
            _logger.Info($"WebPlayer.Seek: 播放器未就绪，暂存进度 position={position}");
            return;
        }

        var script = $"window.seek({position})";
        try
        {
            await WebViewElement.CoreWebView2.ExecuteScriptAsync(script);
            _logger.Debug($"WebPlayer.Seek: position={position}");
        }
        catch (Exception ex)
        {
            _logger.Warn($"WebPlayer.Seek 执行失败: position={position}, {ex.Message}");
        }
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
        // WebView 已销毁，脚本接口不再可用：复位就绪标记，避免后续 Seek/Resume 落到空引用上。
        m_hasLoaded = false;
        m_pendingSeekPosition = double.NaN;
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
