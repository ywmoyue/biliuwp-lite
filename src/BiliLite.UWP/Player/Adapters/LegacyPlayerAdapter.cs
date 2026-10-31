using BiliLite.Controls;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Player;
using BiliLite.Models.Common.Video;
using BiliLite.Models.Common.Video.PlayUrlInfos;
using BiliLite.Player.WebPlayer;
using BiliLite.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using LegacyPlayerControl = BiliLite.Controls.Player;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace BiliLite.Player.Adapters
{
    /// <summary>
    /// Legacy 播放控件兼容门面，仅供历史路径维护，不作为业务入口。
    /// </summary>
    public class LegacyPlayerAdapter : INotifyPropertyChanged
    {
        private readonly LegacyPlayerControl m_legacyPlayer;

        public event PropertyChangedEventHandler PropertyChanged;

        public LegacyPlayerAdapter(LegacyPlayerControl legacyPlayer)
        {
            m_legacyPlayer = legacyPlayer ?? throw new ArgumentNullException(nameof(legacyPlayer));
            m_legacyPlayer.PropertyChanged += LegacyPlayer_PropertyChanged;
        }

        private void LegacyPlayer_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        public event EventHandler<PlayState> PlayStateChanged
        {
            add => m_legacyPlayer.PlayStateChanged += value;
            remove => m_legacyPlayer.PlayStateChanged -= value;
        }

        public event EventHandler PlayMediaOpened
        {
            add => m_legacyPlayer.PlayMediaOpened += value;
            remove => m_legacyPlayer.PlayMediaOpened -= value;
        }

        public event EventHandler PlayMediaEnded
        {
            add => m_legacyPlayer.PlayMediaEnded += value;
            remove => m_legacyPlayer.PlayMediaEnded -= value;
        }

        public event EventHandler<string> PlayMediaError
        {
            add => m_legacyPlayer.PlayMediaError += value;
            remove => m_legacyPlayer.PlayMediaError -= value;
        }

        public event EventHandler<ChangePlayerEngine> ChangeEngine
        {
            add => m_legacyPlayer.ChangeEngine += value;
            remove => m_legacyPlayer.ChangeEngine -= value;
        }

        public event EventHandler StatsUpdated
        {
            add => m_legacyPlayer.StatsUpdated += value;
            remove => m_legacyPlayer.StatsUpdated -= value;
        }

        public PlayState PlayState => m_legacyPlayer.PlayState;

        public double Position => m_legacyPlayer.Position;

        public double Duration => m_legacyPlayer.Duration;

        public bool Buffering => m_legacyPlayer.Buffering;

        public double BufferCache => m_legacyPlayer.BufferCache;

        public double Volume
        {
            get => m_legacyPlayer.Volume;
            set => m_legacyPlayer.Volume = value;
        }

        public bool IsMuted
        {
            get => Volume <= 0;
            set
            {
                if (value)
                {
                    Volume = 0;
                }
                else if (Volume <= 0)
                {
                    Volume = 1;
                }
            }
        }

        public bool Opening => m_legacyPlayer.Opening;

        public bool ShowShakaPlayer => m_legacyPlayer.ShowShakaPlayer;

        public RealPlayerType RealPlayerType
        {
            get => m_legacyPlayer.RealPlayerType;
            set => m_legacyPlayer.RealPlayerType = value;
        }

        public BaseWebPlayer WebPlayer => m_legacyPlayer.WebPlayer;

        public VideoPlayHistoryHelper.ABPlayHistoryEntry ABPlay
        {
            get => m_legacyPlayer.ABPlay;
            set => m_legacyPlayer.ABPlay = value;
        }

        public void SetRatioMode(int mode) => m_legacyPlayer.SetRatioMode(mode);

        public void SetVideoEnable(bool enable) => m_legacyPlayer.SetVideoEnable(enable);

        public void SetPosition(double position) => m_legacyPlayer.SetPosition(position);

        public void SetRate(double value) => m_legacyPlayer.SetRate(value);

        public void Play() => m_legacyPlayer.Play();

        public void Pause() => m_legacyPlayer.Pause();

        public void ClosePlay() => m_legacyPlayer.ClosePlay();

        public Task<bool> CheckPlayUrl() => m_legacyPlayer.CheckPlayUrl();

        public void OpenDevMode() => m_legacyPlayer.OpenDevMode();

        public string MediaInfo => m_legacyPlayer.MediaInfo;

        public string GetMediaInfo() => m_legacyPlayer.GetMediaInfo();

        public async Task CaptureVideoImageToFileAsync(StorageFile saveFile, DisplayInformation displayInformation)
        {
            if (ShowShakaPlayer)
            {
                var imageData = await WebPlayer.CaptureVideo();
                await FileIO.WriteBytesAsync(saveFile, imageData);
                return;
            }

            RenderTargetBitmap bitmap = new RenderTargetBitmap();
            await bitmap.RenderAsync(m_legacyPlayer);
            var pixelBuffer = await bitmap.GetPixelsAsync();
            using var fileStream = await saveFile.OpenAsync(FileAccessMode.ReadWrite);
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, fileStream);
            encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Ignore,
                 (uint)bitmap.PixelWidth,
                 (uint)bitmap.PixelHeight,
                 displayInformation.LogicalDpi,
                 displayInformation.LogicalDpi,
                 pixelBuffer.ToArray());
            await encoder.FlushAsync();
        }

        /// <summary>
        /// 对点播清晰度切换提供统一播放入口，减少 UI 层对具体引擎 API 的分支依赖。
        /// </summary>
        public async Task<PlayerOpenResult> PlayByQualityAsync(BiliPlayUrlInfo quality, double positon = 0, bool isLocal = false, string epId = "")
        {
            if (quality == null)
            {
                return new PlayerOpenResult { result = false, message = "quality is null" };
            }

            var realPlayInfo = RealPlayInfoFactory.CreateFromPlayUrlInfo(
                quality,
                playInfo: null,
                isLocal: isLocal,
                userAgent: quality.UserAgent,
                referer: quality.Referer,
                preferredPlayerType: RealPlayerType);

            return await PlayByRealPlayInfoAsync(realPlayInfo, positon, epId);
        }

        /// <summary>
        /// 统一执行入口：所有点播类型最终都应通过 RealPlayInfo 进入此方法。
        /// </summary>
        public async Task<PlayerOpenResult> PlayByRealPlayInfoAsync(RealPlayInfo realPlayInfo, double positon = 0, string epId = "")
        {
            if (realPlayInfo == null)
            {
                return new PlayerOpenResult { result = false, message = "realPlayInfo is null" };
            }

            switch (realPlayInfo.PlayMediaType)
            {
                case PlayMediaType.Dash:
                    return await PlayDashByRealPlayInfoAsync(realPlayInfo, positon);
                case PlayMediaType.Single:
                    if (realPlayInfo.SingleIsFlv)
                    {
                        return await PlaySingleFlvByRealPlayInfoAsync(realPlayInfo, positon, epId);
                    }

                    return await m_legacyPlayer.PlayerSingleMp4UseNativeAsync(realPlayInfo.SingleUrl, positon: positon, isLocal: realPlayInfo.IsLocal);
                case PlayMediaType.MultiFlv:
                    return await m_legacyPlayer.PlayVideoUseSYEngine(realPlayInfo.MultiFlvUrls,
                        realPlayInfo.UserAgent,
                        realPlayInfo.Referer,
                        positon: positon,
                        epId: epId,
                        isLocal: realPlayInfo.IsLocal);
                default:
                    return new PlayerOpenResult { result = false, message = "unsupported play media type" };
            }
        }

        public async Task<PlayerOpenResult> PlayByChangeEngineAsync(ChangePlayerEngine engine, BiliPlayUrlInfo quality, double positon, string epId = "")
        {
            if (!engine.need_change)
            {
                return new PlayerOpenResult { result = false, message = engine.message };
            }

            if (quality == null)
            {
                return new PlayerOpenResult { result = false, message = "quality is null" };
            }

            if (engine.play_type == PlayMediaType.Dash && engine.change_engine == PlayEngine.FFmpegInteropMSS)
            {
                var realPlayInfo = RealPlayInfoFactory.CreateFromPlayUrlInfo(
                    quality,
                    preferredPlayerType: RealPlayerType.FFmpegInterop,
                    fallbackPlayerTypes: new[] { RealPlayerType.FFmpegInterop });
                return await PlayByRealPlayInfoAsync(realPlayInfo, positon, epId);
            }

            if (engine.play_type == PlayMediaType.Single && engine.change_engine == PlayEngine.SYEngine)
            {
                var realPlayInfo = RealPlayInfoFactory.CreateFromPlayUrlInfo(
                    quality,
                    preferredPlayerType: RealPlayerType.Native,
                    fallbackPlayerTypes: new[] { RealPlayerType.Native });
                return await PlayByRealPlayInfoAsync(realPlayInfo, positon, epId);
            }

            return new PlayerOpenResult { result = false, message = "unsupported change engine" };
        }

        private List<Func<PlayerOpenParam, Task<PlayerOpenResult>>> GetDashPlayerStrategies(RealPlayerType preferredType)
        {
            var strategies = new List<Func<PlayerOpenParam, Task<PlayerOpenResult>>>();
            switch (preferredType)
            {
                case RealPlayerType.Native:
                    strategies.Add(param => m_legacyPlayer.PlayerDashUseNative(param));
                    strategies.Add(param => m_legacyPlayer.PlayDashUseFFmpegInterop(param));
                    strategies.Add(param => m_legacyPlayer.PlayerDashUseShaka(param));
                    break;
                case RealPlayerType.FFmpegInterop:
                    strategies.Add(param => m_legacyPlayer.PlayDashUseFFmpegInterop(param));
                    strategies.Add(param => m_legacyPlayer.PlayerDashUseNative(param));
                    strategies.Add(param => m_legacyPlayer.PlayerDashUseShaka(param));
                    break;
                case RealPlayerType.ShakaPlayer:
                    strategies.Add(param => m_legacyPlayer.PlayerDashUseShaka(param));
                    strategies.Add(param => m_legacyPlayer.PlayerDashUseNative(param));
                    break;
            }

            return strategies;
        }

        private async Task<PlayerOpenResult> PlayDashByRealPlayInfoAsync(RealPlayInfo realPlayInfo, double positon)
        {
            if (realPlayInfo?.DashInfo == null)
            {
                return new PlayerOpenResult { result = false, message = "dash play info is null" };
            }

            var playerOpenParam = new PlayerOpenParam
            {
                DashInfo = realPlayInfo.DashInfo,
                UserAgent = realPlayInfo.UserAgent,
                Referer = realPlayInfo.Referer,
                Positon = positon,
                IsLocal = realPlayInfo.IsLocal,
            };

            var strategies = GetDashPlayerStrategies(realPlayInfo.PreferredPlayerType);
            if (realPlayInfo.FallbackPlayerTypes != null && realPlayInfo.FallbackPlayerTypes.Count > 0)
            {
                strategies = realPlayInfo.FallbackPlayerTypes
                    .SelectMany(GetDashPlayerStrategies)
                    .Distinct()
                    .ToList();
            }

            foreach (var strategy in strategies)
            {
                var result = await strategy(playerOpenParam);
                if (result.result)
                {
                    return result;
                }
            }

            return new PlayerOpenResult { result = false };
        }

        private async Task<PlayerOpenResult> PlaySingleFlvByRealPlayInfoAsync(RealPlayInfo realPlayInfo, double positon, string epId)
        {
            if (string.IsNullOrWhiteSpace(realPlayInfo?.SingleUrl))
            {
                return new PlayerOpenResult { result = false, message = "single flv url is empty" };
            }

            var chain = realPlayInfo.FallbackPlayerTypes?.Count > 0
                ? realPlayInfo.FallbackPlayerTypes
                : new List<RealPlayerType> { RealPlayerType.FFmpegInterop, RealPlayerType.Native };

            foreach (var playerType in chain)
            {
                PlayerOpenResult result;
                if (playerType == RealPlayerType.FFmpegInterop)
                {
                    result = await m_legacyPlayer.PlaySingleFlvUseFFmpegInterop(realPlayInfo.SingleUrl,
                        realPlayInfo.UserAgent,
                        realPlayInfo.Referer,
                        positon: positon);
                }
                else
                {
                    // 旧播放器 SYEngine 路径在新回退链中临时映射为 Native 槽位。
                    result = await m_legacyPlayer.PlaySingleFlvUseSYEngine(realPlayInfo.SingleUrl,
                        realPlayInfo.UserAgent,
                        realPlayInfo.Referer,
                        positon: positon,
                        epId: epId);
                }

                if (result.result)
                {
                    return result;
                }
            }

            return new PlayerOpenResult { result = false };
        }

        public void Dispose()
        {
            m_legacyPlayer.PropertyChanged -= LegacyPlayer_PropertyChanged;
            m_legacyPlayer.Dispose();
        }
    }
}
