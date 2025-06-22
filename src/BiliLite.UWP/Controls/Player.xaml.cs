using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Video.PlayUrlInfos;
using BiliLite.Services;
using FFmpegInteropX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Editing;
using Windows.Media.Playback;
using Windows.Media.Streaming.Adaptive;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Web.Http;
using BiliLite.Models.Common.Player;
using Newtonsoft.Json;
using BiliLite.Models;
using BiliLite.Player.ShakaPlayer.Extensions;
using BiliLite.Player.WebPlayer;
using BiliLite.Player.WebPlayer.Models;
using PropertyChanged;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Controls
{
    //TODO 写得太复杂了，需要重写
    public sealed partial class Player : UserControl, IDisposable, INotifyPropertyChanged
    {
        #region Fields

        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        private string m_referer;
        private string m_userAgent;
        private BiliDashPlayUrlInfo m_dashInfo;
        private PlayEngine m_currentEngine;

        private FFmpegMediaSource m_ffmpegMssVideo;
        private MediaPlayer m_playerVideo;
        //音视频分离
        private FFmpegMediaSource m_ffmpegMssAudio;
        private MediaPlayer m_playerAudio;
        private MediaTimelineController m_mediaTimelineController;

        //多段FLV
        private List<FFmpegMediaSource> m_ffmpegMssItems;
        private MediaPlaybackList m_mediaPlaybackList;
        private BaseWebPlayer m_webPlayer;
        private WebPlayerStatsUpdatedData m_webPlayerStatsData;

        #endregion

        #region Constructors
        public Player()
        {
            InitializeComponent();

            // We don't have ARM64 support of SYEngine.
            if (RuntimeInformation.ProcessArchitecture != Architecture.Arm64)
            {
                SYEngine.Core.ForceNetworkMode = true;
                SYEngine.Core.ForceSoftwareDecode = !SettingService.GetValue<bool>(SettingConstants.Player.HARDWARE_DECODING, false);
            }
            //_ffmpegConfig.StreamBufferSize = 655360;//1024 * 30;

        }

        #endregion

        #region Properties

        public PlayState PlayState { get; set; }
        public PlayMediaType PlayMediaType { get; set; }
        public VideoPlayHistoryHelper.ABPlayHistoryEntry ABPlay { get; set; }

        [DoNotNotify]
        public BaseWebPlayer WebPlayer => m_webPlayer;

        /// <summary>
        /// 进度
        /// </summary>
        public double Position
        {
            get { return (double)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register("Position", typeof(double), typeof(Player), new PropertyMetadata(0.0, OnPositionChanged));

        /// <summary>
        /// 时长
        /// </summary>
        public double Duration
        {
            get { return (double)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }

        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(double), typeof(Player), new PropertyMetadata(0.0));

        /// <summary>
        /// 音量0-1
        /// </summary>
        public double Volume
        {
            get
            {
                return (double)GetValue(VolumeProperty);
            }
            set
            {
                if (value > 1)
                {
                    value = 1;
                }
                if (value < 0)
                {
                    value = 0;
                }
                SetValue(VolumeProperty, value);
            }
        }

        public static readonly DependencyProperty VolumeProperty =
            DependencyProperty.Register("Volume", typeof(double), typeof(Player), new PropertyMetadata(1.0, OnVolumeChanged));

        /// <summary>
        /// 是否缓冲中
        /// </summary>
        public bool Buffering
        {
            get { return (bool)GetValue(BufferingProperty); }
            set { SetValue(BufferingProperty, value); }
        }

        public static readonly DependencyProperty BufferingProperty =
            DependencyProperty.Register("Buffering", typeof(bool), typeof(Player), new PropertyMetadata(false));

        /// <summary>
        /// 缓冲进度,0-100
        /// </summary>
        public double BufferCache
        {
            get { return (double)GetValue(BufferCacheProperty); }
            set { SetValue(BufferCacheProperty, value); }
        }

        public static readonly DependencyProperty BufferCacheProperty =
            DependencyProperty.Register("BufferCache", typeof(double), typeof(Player), new PropertyMetadata(1d));

        /// <summary>
        /// 播放速度
        /// </summary>
        public double Rate { get; set; } = 1.0;

        /// <summary>
        /// 媒体信息
        /// </summary>
        public string MediaInfo
        {
            get { return (string)GetValue(MediaInfoProperty); }
            set { SetValue(MediaInfoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MediaInfo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MediaInfoProperty =
            DependencyProperty.Register("MediaInfo", typeof(string), typeof(Player), new PropertyMetadata(""));
        public bool Opening { get; set; }

        public RealPlayerType RealPlayerType { get; set; }

        [DependsOn(nameof(RealPlayerType))]
        public bool ShowMediaPlayer => RealPlayerType is RealPlayerType.Native or RealPlayerType.FFmpegInterop;

        [DependsOn(nameof(RealPlayerType))]
        public bool ShowShakaPlayer => RealPlayerType is RealPlayerType.ShakaPlayer or RealPlayerType.Mpegts;

        #endregion

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 播放状态变更
        /// </summary>
        public event EventHandler<PlayState> PlayStateChanged;

        /// <summary>
        /// 媒体加载完成
        /// </summary>
        public event EventHandler PlayMediaOpened;

        /// <summary>
        /// 播放完成
        /// </summary>
        public event EventHandler PlayMediaEnded;

        /// <summary>
        /// 播放错误
        /// </summary>
        public event EventHandler<string> PlayMediaError;

        /// <summary>
        /// 更改播放引擎
        /// </summary>
        public event EventHandler<ChangePlayerEngine> ChangeEngine;

        public event EventHandler StatsUpdated; 

        #endregion

        #region Private Methods

        private void DoPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private static void OnPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as Player;
            if (sender.ABPlay != null && sender.ABPlay.PointB != 0 && (double)e.NewValue > sender.ABPlay.PointB)
            {
                sender.Position = sender.ABPlay.PointA;
                return;
            }
            if (Math.Abs((double)e.NewValue - (double)e.OldValue) > 1)
            {
                if (sender.PlayState == PlayState.Playing || sender.PlayState == PlayState.Pause)
                {
                    sender.SetPosition((double)e.NewValue);
                }
            }
        }

        private static void OnVolumeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sender = d as Player;
            var value = (double)e.NewValue;
            if (value > 1)
            {
                value = 1;
            }
            else if (value < 0)
            {
                value = 0;
            }

            if (sender.ShowMediaPlayer)
                sender.SetVolume(value);
            else sender.m_webPlayer.SetVolume(value);
        }

        private async Task<AdaptiveMediaSource> CreateAdaptiveMediaSource(BiliDashPlayUrlInfo dashInfo, string userAgent, string referer)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                if (userAgent != null && userAgent.Length > 0)
                {
                    httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
                }
                if (referer != null && referer.Length > 0)
                {
                    httpClient.DefaultRequestHeaders.Add("Referer", referer);
                }
                var mpdStr = "";
                if (dashInfo.Audio != null)
                {
                    mpdStr = $@"<MPD xmlns=""urn:mpeg:DASH:schema:MPD:2011""  profiles=""urn:mpeg:dash:profile:isoff-on-demand:2011"" type=""static"">
                  <Period  start=""PT0S"">
                    <AdaptationSet>
                      <ContentComponent contentType=""video"" id=""1"" />
                      <Representation bandwidth=""{dashInfo.Video.BandWidth}"" codecs=""{dashInfo.Video.Codecs}"" height=""{dashInfo.Video.Height}"" id=""{dashInfo.Video.ID}"" mimeType=""{dashInfo.Video.MimeType}"" width=""{dashInfo.Video.Width}"" startWithSap=""{dashInfo.Video.StartWithSap}"">
                        <BaseURL></BaseURL>
                        <SegmentBase indexRange=""{dashInfo.Video.SegmentBaseIndexRange}"">
                          <Initialization range=""{dashInfo.Video.SegmentBaseInitialization}"" />
                        </SegmentBase>
                      </Representation>
                    </AdaptationSet>
                    <AdaptationSet>
                      <ContentComponent contentType=""audio"" id=""2"" />
                      <Representation bandwidth=""{dashInfo.Audio.BandWidth}"" codecs=""{dashInfo.Audio.Codecs}"" id=""{dashInfo.Audio.ID}"" mimeType=""{dashInfo.Audio.MimeType}"" >
                        <BaseURL></BaseURL>
                        <SegmentBase indexRange=""{dashInfo.Audio.SegmentBaseIndexRange}"">
                          <Initialization range=""{dashInfo.Audio.SegmentBaseInitialization}"" />
                        </SegmentBase>
                      </Representation>
                    </AdaptationSet>
                  </Period>
                </MPD>";
                }
                else
                {
                    mpdStr = $@"<MPD xmlns=""urn:mpeg:DASH:schema:MPD:2011""  profiles=""urn:mpeg:dash:profile:isoff-on-demand:2011"" type=""static"">
                  <Period  start=""PT0S"">
                    <AdaptationSet>
                      <ContentComponent contentType=""video"" id=""1"" />
                      <Representation bandwidth=""{dashInfo.Video.BandWidth}"" codecs=""{dashInfo.Video.Codecs}"" height=""{dashInfo.Video.Height}"" id=""{dashInfo.Video.ID}"" mimeType=""{dashInfo.Video.MimeType}"" width=""{dashInfo.Video.Width}"" startWithSap=""{dashInfo.Video.StartWithSap}"">
                        <BaseURL></BaseURL>
                        <SegmentBase indexRange=""{dashInfo.Video.SegmentBaseIndexRange}"">
                          <Initialization range=""{dashInfo.Video.SegmentBaseInitialization}"" />
                        </SegmentBase>
                      </Representation>
                    </AdaptationSet>
                  </Period>
                </MPD>";
                }



                var stream = new MemoryStream(Encoding.UTF8.GetBytes(mpdStr)).AsInputStream();
                var soure = await AdaptiveMediaSource.CreateFromStreamAsync(stream, new Uri(dashInfo.Video.Url), "application/dash+xml", httpClient);
                soure.MediaSource.AdvancedSettings.AllSegmentsIndependent = true;

                var s = soure.Status;
                soure.MediaSource.DownloadRequested += (sender, args) =>
                {
                    if (args.ResourceContentType == "audio/mp4")
                    {
                        args.Result.ResourceUri = new Uri(dashInfo.Audio.Url);
                    }
                };
                return soure.MediaSource;
            }
            catch (Exception)
            {
                return null;
            }

        }
        private MediaSourceConfig CreateFFmpegInteropConfig(string userAgent, string referer)
        {

            var passthrough = SettingService.GetValue<bool>(SettingConstants.Player.HARDWARE_DECODING, true);
            var _ffmpegConfig = new MediaSourceConfig();
            if (userAgent != null && userAgent.Length > 0)
            {
                _ffmpegConfig.FFmpegOptions.Add("user_agent", userAgent);
            }
            if (referer != null && referer.Length > 0)
            {
                _ffmpegConfig.FFmpegOptions.Add("referer", referer);
                _ffmpegConfig.FFmpegOptions.Add("headers", $"Referer: {referer}");
            }

            _ffmpegConfig.FFmpegOptions.Add("allowed_extensions","ALL");
            _ffmpegConfig.FFmpegOptions.Add("reconnect", "1");
            _ffmpegConfig.FFmpegOptions.Add("reconnect_streamed", "1");
            _ffmpegConfig.FFmpegOptions.Add("reconnect_on_network_error", "1");
            //_ffmpegConfig.BufferTime
            var ffmpegOptionsStr =
                SettingService.GetValue(SettingConstants.Player.FFMPEG_INTEROP_X_OPTIONS, "");
            var ffmpegOptions = new Dictionary<string, string>();
            try
            {
                if (!string.IsNullOrEmpty(ffmpegOptionsStr))
                {
                    var options = ffmpegOptionsStr.Split(",");
                    foreach (var optionStr in options)
                    {
                        var option = optionStr.Split(':').Select(x => x.Trim()).ToList();
                        ffmpegOptions.Add(option[0], option[1]);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Warn("解析额外参数错误", ex);
            }

            if (ffmpegOptions.Any())
            {
                foreach (var keyValuePair in ffmpegOptions)
                {
                    _ffmpegConfig.FFmpegOptions.Add(keyValuePair.Key,keyValuePair.Value);
                }
            }
            return _ffmpegConfig;
        }

        private SYEngine.PlaylistNetworkConfigs CreatePlaylistNetworkConfigs(string userAgent, string referer, string epId = "")
        {

            SYEngine.PlaylistNetworkConfigs config = new SYEngine.PlaylistNetworkConfigs();
            config.DownloadRetryOnFail = true;
            config.HttpCookie = string.Empty;
            config.UniqueId = string.Empty;
            config.HttpReferer = string.Empty;
            config.HttpUserAgent = string.Empty;
            if (userAgent != null && userAgent.Length > 0)
            {
                config.HttpUserAgent = userAgent;
            }
            if (referer != null && referer.Length > 0)
            {
                config.HttpReferer = referer;
            }
            return config;
        }

        //private void vlcVideoView_Initialized(object sender, LibVLCSharp.Platforms.UWP.InitializedEventArgs e)
        //{
        //    //_libVLC = new LibVLCSharp.Shared.LibVLC(enableDebugLogs: true, e.SwapChainOptions);
        //    ////LibVLC.SetUserAgent("Mozilla/5.0", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/86.0.4240.198 Safari/537.36");

        //    //_libVLC.Log += LibVLC_Log;
        //    //_vlcMediaPlayer = new LibVLCSharp.Shared.MediaPlayer(_libVLC);
        //    //vlcVideoView.MediaPlayer = _vlcMediaPlayer;
        //}

        //private void LibVLC_Log(object sender, LibVLCSharp.Shared.LogEventArgs e)
        //{
        //    Debug.WriteLine(e.FormattedLog);
        //}

        private void ShakaPlayer_OnStatsUpdated(object sender, WebPlayerStatsUpdatedData e)
        {
            m_webPlayerStatsData = e;
            StatsUpdated?.Invoke(this, null);
        }

        private async Task OnPlayerMediaOpened(Action specificPlayerAction = null)
        {
            Opening = false;
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                specificPlayerAction?.Invoke();
                PlayMediaOpened?.Invoke(this, EventArgs.Empty);
            });
        }

        private async Task OnPlayerMediaEnded()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (PlayState == PlayState.End)
                {
                    PlayStateChanged?.Invoke(this, PlayState);
                    PlayMediaEnded?.Invoke(this, EventArgs.Empty);
                    return;
                }
                //加个判断，是否真的播放完成了
                if ((int)Position < (int)Duration) return;
                PlayState = PlayState.End;
                Position = 0;
                PlayStateChanged?.Invoke(this, PlayState);
                PlayMediaEnded?.Invoke(this, EventArgs.Empty);
            });
        }

        private async Task OnPlayerPositionChanged(TimeSpan position)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    Position = position.TotalSeconds;
                }
                catch (Exception)
                {
                }
            });
        }

        private async Task OnPlayerMediaFailed(ChangePlayerEngine changeEngineEventArgs)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                PlayState = PlayState.Error;
                PlayStateChanged?.Invoke(this, PlayState);
                ChangeEngine?.Invoke(this, changeEngineEventArgs);
            });
        }

        private async Task OnPlayerBufferingStarted()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Buffering = true;
            });
        }

        private async Task OnPlayerBufferingProgressChanged(MediaPlaybackSession session)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Buffering = true;
                BufferCache = session.BufferingProgress;
            });
        }

        private async Task OnPlayerBufferingEnded()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Buffering = false;
            });
        }

        private void HookVideoPlayerEvent(ChangePlayerEngine playFailedChangeEngineEventArgs, bool playFailedNeedCheckPlayerHasValue = false, Action specificPlayerMediaOpenAction = null)
        {
            //播放开始
            m_playerVideo.MediaOpened += async (e, arg) =>
            {
                _logger.Trace($"MediaOpened");
                await OnPlayerMediaOpened(specificPlayerMediaOpenAction);
            };
            //播放完成
            m_playerVideo.MediaEnded += async (e, arg) =>
            {
                _logger.Trace($"MediaEnded");
                await OnPlayerMediaEnded();
            };
            //播放错误
            m_playerVideo.MediaFailed += async (e, arg) =>
            {
                _logger.Trace($"MediaFailed: {JsonConvert.SerializeObject(arg)}");
                if (playFailedNeedCheckPlayerHasValue && m_playerVideo?.Source == null)
                {
                    return;
                }
                playFailedChangeEngineEventArgs.message = arg.ErrorMessage;
                await OnPlayerMediaFailed(playFailedChangeEngineEventArgs);
            };
            //缓冲开始
            m_playerVideo.PlaybackSession.BufferingStarted += async (e, arg) =>
            {
                _logger.Trace($"BufferingStarted: {JsonConvert.SerializeObject(arg)}");
                await OnPlayerBufferingStarted();
            };
            //缓冲进行中
            m_playerVideo.PlaybackSession.BufferingProgressChanged += async (e, arg) =>
            {
                _logger.Trace($"BufferingProgressChanged");
                await OnPlayerBufferingProgressChanged(e);
            };
            //缓冲结束
            m_playerVideo.PlaybackSession.BufferingEnded += async (e, arg) =>
            {
                _logger.Trace($"BufferingEnded");
                await OnPlayerBufferingEnded();
            };
            if (m_mediaTimelineController != null)
            {
                //进度变更
                m_mediaTimelineController.PositionChanged += async (e, arg) =>
                {
                    await OnPlayerPositionChanged(e.Position);
                };
            }
            else
            {
                //进度变更
                m_playerVideo.PlaybackSession.PositionChanged += async (e, arg) =>
                {
                    await OnPlayerPositionChanged(e.Position);
                };
            }

            if (m_playerAudio != null)
            {
                //播放开始
                m_playerAudio.MediaOpened += async (e, arg) =>
                {
                    _logger.Trace($"m_playerAudioMediaOpened");
                };
                //播放完成
                m_playerAudio.MediaEnded += async (e, arg) =>
                {
                    _logger.Trace($"m_playerAudioMediaEnded");
                };
                //m_playerAudio.PlaybackSession.PositionChanged += (e, arg) =>
                //{
                //    _logger.Trace(
                //        $"audio:{m_playerAudio.Position},video:{m_playerVideo.Position},controller:{m_mediaTimelineController.Position},offset:{m_playerAudio.TimelineControllerPositionOffset}");
                //};
                //播放错误
                m_playerAudio.MediaFailed += async (e, arg) =>
                {
                    _logger.Trace($"m_playerAudioMediaFailed: {JsonConvert.SerializeObject(arg)}");
                };
                //缓冲开始
                m_playerAudio.PlaybackSession.BufferingStarted += async (e, arg) =>
                {
                    _logger.Trace($"m_playerAudioBufferingStarted: {JsonConvert.SerializeObject(arg)}");
                };
                //缓冲进行中
                m_playerAudio.PlaybackSession.BufferingProgressChanged += async (e, arg) =>
                {
                    _logger.Trace($"m_playerAudioBufferingProgressChanged");
                };
                //缓冲结束
                m_playerAudio.PlaybackSession.BufferingEnded += async (e, arg) =>
                {
                    _logger.Trace($"m_playerAudioBufferingEnded: {JsonConvert.SerializeObject(arg)}");
                };
            }
        }

        private async void ShakaPlayer_OnPositionChanged(object sender, double e)
        {
            await OnPlayerPositionChanged(TimeSpan.FromSeconds(e));
        }

        private async void ShakaPlayer_OnPlayerLoaded(object sender, ShakaPlayerLoadedData e)
        {
            Duration = e.Duration;
            Opening = false;
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                PlayMediaOpened?.Invoke(this, EventArgs.Empty);

                Buffering = false;
            });
        }

        private async void ShakaPlayer_OnEnded(object sender, EventArgs e)
        {
            await OnPlayerMediaEnded();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 使用AdaptiveMediaSource播放视频
        /// </summary>
        /// <returns></returns>
        public async Task<PlayerOpenResult> PlayerDashUseNative(BiliDashPlayUrlInfo dashInfo, string userAgent, string referer, double positon = 0)
        {
            try
            {
                mediaPlayerVideo.Visibility = Visibility.Visible;
                //vlcVideoView.Visibility = Visibility.Collapsed;
                m_dashInfo = dashInfo;
                m_referer = referer;
                m_userAgent = userAgent;

                Opening = true;
                m_currentEngine = PlayEngine.Native;
                PlayMediaType = PlayMediaType.Dash;
                //加载中
                PlayState = PlayState.Loading;
                PlayStateChanged?.Invoke(this, PlayState);
                ClosePlay();


                //设置播放器
                m_playerVideo = new MediaPlayer();
                //_playerVideo.Source = MediaSource.CreateFromUri(new Uri(videoUrl.baseUrl));
                var mediaSource = await CreateAdaptiveMediaSource(dashInfo, userAgent, referer);
                if (mediaSource == null)
                {
                    return new PlayerOpenResult()
                    {
                        result = false,
                        message = "创建MediaSource失败"
                    };
                }
                
                m_playerVideo.Source = MediaSource.CreateFromAdaptiveMediaSource(mediaSource);
                Buffering = true;

                var playFailedChangeEngineArgs = new ChangePlayerEngine()
                {
                    change_engine = PlayEngine.FFmpegInteropMSS,
                    current_mode = PlayEngine.Native,
                    need_change = true,
                    play_type = PlayMediaType.Dash
                };

                HookVideoPlayerEvent(playFailedChangeEngineArgs, specificPlayerMediaOpenAction: () =>
                {
                    Buffering = false;
                    Duration = m_playerVideo.PlaybackSession.NaturalDuration.TotalSeconds;
                });

                PlayState = PlayState.Pause;
                PlayStateChanged?.Invoke(this, PlayState);
                //设置音量
                m_playerVideo.Volume = Volume;
                //设置速率
                m_playerVideo.PlaybackSession.PlaybackRate = Rate;

                //绑定MediaPlayer
                mediaPlayerVideo.SetMediaPlayer(m_playerVideo);

                return new PlayerOpenResult()
                {
                    result = true
                };
            }
            catch (Exception ex)
            {
                //PlayMediaError?.Invoke(this, "视频加载时出错:" + ex.Message);
                return new PlayerOpenResult()
                {
                    result = false,
                    message = ex.Message,
                    detail_message = ex.StackTrace
                };
            }
        }

        public async Task<PlayerOpenResult> PlayDashUseWebPlayer(BaseWebPlayer webPlayer, BiliPlayUrlInfo quality, string userAgent, string referer, double positon = 0)
        {
            try
            {
                m_webPlayer = webPlayer;
                mediaPlayerVideo.Visibility = Visibility.Visible;
                //vlcVideoView.Visibility = Visibility.Collapsed;
                m_dashInfo = quality.DashInfo;
                m_referer = referer;
                m_userAgent = userAgent;

                Opening = true;
                m_currentEngine = webPlayer.PlayEngine;
                PlayMediaType = PlayMediaType.Dash;
                //加载中
                PlayState = PlayState.Loading;
                PlayStateChanged?.Invoke(this, PlayState);
                ClosePlay();


                var mpdModel = new GenerateMPDModel()
                {
                    AudioBandwidth = quality.DashInfo.Audio.BandWidth.ToString(),
                    AudioCodec = quality.DashInfo.Audio.Codecs,
                    AudioID = quality.DashInfo.Audio.ID.ToString(),
                    AudioUrl = quality.DashInfo.Audio.Url,
                    Duration = quality.DashInfo.Duration,
                    DurationMS = quality.Timelength,
                    VideoBandwidth = quality.DashInfo.Video.BandWidth.ToString(),
                    VideoCodec = quality.DashInfo.Video.Codecs,
                    VideoID = quality.DashInfo.Video.ID.ToString(),
                    VideoFrameRate = quality.DashInfo.Video.FrameRate.ToString(),
                    VideoHeight = quality.DashInfo.Video.Height,
                    VideoWidth = quality.DashInfo.Video.Width,
                    VideoUrl = quality.DashInfo.Video.Url,
                };
                var mpdUrl = await mpdModel.CreateMpdFiles();

                //设置播放器
                await webPlayer.LoadUrl(quality.DashInfo.Video.Url, quality.DashInfo.Audio.Url);

                Buffering = true;

                PlayState = PlayState.Pause;
                PlayStateChanged?.Invoke(this, PlayState);
                //设置音量
                webPlayer.SetVolume(Volume);
                //设置速率
                webPlayer.SetRate(Rate);

                return new PlayerOpenResult()
                {
                    result = true
                };
            }
            catch (Exception ex)
            {
                //PlayMediaError?.Invoke(this, "视频加载时出错:" + ex.Message);
                return new PlayerOpenResult()
                {
                    result = false,
                    message = ex.Message,
                    detail_message = ex.StackTrace
                };
            }
        }

        public async Task<PlayerOpenResult> PlayerDashUseShaka(BiliPlayUrlInfo quality, string userAgent, string referer, double positon = 0)
        {
            RealPlayerType = RealPlayerType.ShakaPlayer;
            return await PlayDashUseWebPlayer(ShakaPlayer, quality, userAgent, referer, positon);
        }

        /// <summary>
        /// 使用eMediaSource播放视频
        /// </summary>
        /// <param name="videoUrl"></param>
        /// <param name="audioUrl"></param>
        /// <param name="positon"></param>
        /// <param name="needConfig"></param>
        /// <returns></returns>
        public async Task<PlayerOpenResult> PlayerSingleMp4UseNativeAsync(string url, double positon = 0, bool needConfig = true, bool isLocal = false)
        {
            try
            {
                mediaPlayerVideo.Visibility = Visibility.Visible;
                //vlcVideoView.Visibility = Visibility.Collapsed;

                Opening = true;
                m_currentEngine = PlayEngine.Native;
                PlayMediaType = PlayMediaType.Dash;
                //加载中
                PlayState = PlayState.Loading;
                PlayStateChanged?.Invoke(this, PlayState);
                ClosePlay();

                //设置播放器
                m_playerVideo = new MediaPlayer();
                if (isLocal)
                {
                    m_playerVideo.Source = MediaSource.CreateFromStorageFile(await StorageFile.GetFileFromPathAsync(url));
                }
                else
                {
                    m_playerVideo.Source = MediaSource.CreateFromUri(new Uri(url));
                }

                var playFailedChangeEngineArgs = new ChangePlayerEngine()
                {
                    change_engine = PlayEngine.FFmpegInteropMSS,
                    current_mode = PlayEngine.Native,
                    need_change = true,
                    play_type = PlayMediaType.Dash
                };

                HookVideoPlayerEvent(playFailedChangeEngineArgs, specificPlayerMediaOpenAction: () =>
                {
                    Duration = m_playerVideo.PlaybackSession.NaturalDuration.TotalSeconds;
                });

                PlayState = PlayState.Pause;
                PlayStateChanged?.Invoke(this, PlayState);
                //设置音量
                m_playerVideo.Volume = Volume;
                //设置速率
                m_playerVideo.PlaybackSession.PlaybackRate = Rate;

                //绑定MediaPlayer
                mediaPlayerVideo.SetMediaPlayer(m_playerVideo);

                return new PlayerOpenResult()
                {
                    result = true
                };
            }
            catch (Exception ex)
            {
                //PlayMediaError?.Invoke(this, "视频加载时出错:" + ex.Message);
                return new PlayerOpenResult()
                {
                    result = false,
                    message = ex.Message,
                    detail_message = ex.StackTrace
                };
            }
        }

        /// <summary>
        /// 使用FFmpegInterop解码播放音视频分离视频
        /// </summary>
        /// <param name="videoUrl"></param>
        /// <param name="audioUrl"></param>
        /// <param name="positon"></param>
        /// <param name="needConfig"></param>
        /// <returns></returns>
        public async Task<PlayerOpenResult> PlayDashUseFFmpegInterop(BiliDashPlayUrlInfo dashPlayUrlInfo, string userAgent, string referer, double positon = 0, bool needConfig = true, bool isLocal = false)
        {
            try
            {
                mediaPlayerVideo.Visibility = Visibility.Visible;
                //vlcVideoView.Visibility = Visibility.Collapsed;
                Opening = true;
                m_dashInfo = dashPlayUrlInfo;
                m_referer = referer;
                m_userAgent = userAgent;

                m_currentEngine = PlayEngine.FFmpegInteropMSS;

                PlayMediaType = PlayMediaType.Dash;
                //加载中
                PlayState = PlayState.Loading;
                PlayStateChanged?.Invoke(this, PlayState);
                //关闭正在播放的视频
                ClosePlay();
                var _ffmpegConfig = CreateFFmpegInteropConfig(userAgent, referer);
                
                if (isLocal)
                {

                    var videoFile = await StorageFile.GetFileFromPathAsync(dashPlayUrlInfo.Video.Url);
                    m_ffmpegMssVideo = await FFmpegMediaSource.CreateFromStreamAsync(await videoFile.OpenAsync(FileAccessMode.Read), _ffmpegConfig);
                    if (dashPlayUrlInfo.Audio != null)
                    {
                        var audioFile = await StorageFile.GetFileFromPathAsync(dashPlayUrlInfo.Audio.Url);
                        m_ffmpegMssAudio = await FFmpegMediaSource.CreateFromStreamAsync(await audioFile.OpenAsync(FileAccessMode.Read), _ffmpegConfig);
                    }
                }
                else
                {
                    m_ffmpegMssVideo = await FFmpegMediaSource.CreateFromUriAsync(dashPlayUrlInfo.Video.Url, _ffmpegConfig);
                    if (dashPlayUrlInfo.Audio != null)
                    {
                        m_ffmpegMssAudio = await FFmpegMediaSource.CreateFromUriAsync(dashPlayUrlInfo.Audio.Url, _ffmpegConfig);
                    }

                }


                //设置时长
                Duration = m_ffmpegMssVideo.Duration.TotalSeconds;
                //设置视频
                m_playerVideo = new MediaPlayer();
                m_playerVideo.Source = m_ffmpegMssVideo.CreateMediaPlaybackItem();
                //设置音频
                if (dashPlayUrlInfo.Audio != null)
                {
                    m_playerAudio = new MediaPlayer();
                    m_playerAudio.Source = m_ffmpegMssAudio.CreateMediaPlaybackItem();
                }

                //设置时间线控制器
                m_mediaTimelineController = new MediaTimelineController();
                m_playerVideo.CommandManager.IsEnabled = false;
                m_playerVideo.TimelineController = m_mediaTimelineController;
                if (dashPlayUrlInfo.Audio != null)
                {
                    m_playerAudio.CommandManager.IsEnabled = true;
                    m_playerAudio.TimelineController = m_mediaTimelineController;
                }

                var playFailedChangeEngineArgs = new ChangePlayerEngine()
                {
                    need_change = false,
                };

                HookVideoPlayerEvent(playFailedChangeEngineArgs);

                if (dashPlayUrlInfo.Audio != null)
                {
                    m_playerAudio.PlaybackSession.BufferingStarted += async (e, arg) =>
                    {
                        await OnPlayerBufferingStarted();
                    };
                    m_playerAudio.PlaybackSession.BufferingProgressChanged += async (e, arg) =>
                    {
                        await OnPlayerBufferingProgressChanged(e);
                    };
                    m_playerAudio.PlaybackSession.BufferingEnded += async (e, arg) =>
                    {
                        await OnPlayerBufferingEnded();
                    };
                    //设置音量
                    m_playerAudio.Volume = Volume;
                    mediaPlayerAudio.SetMediaPlayer(m_playerAudio);
                }
                else
                {
                    m_playerVideo.Volume = Volume;
                }
                PlayState = PlayState.Pause;
                PlayStateChanged?.Invoke(this, PlayState);

                //绑定MediaPlayer
                mediaPlayerVideo.SetMediaPlayer(m_playerVideo);

                //设置速率
                m_mediaTimelineController.ClockRate = Rate;

                return new PlayerOpenResult()
                {
                    result = true
                };
            }
            catch (Exception ex)
            {
                //PlayMediaError?.Invoke(this, "视频加载时出错:" + ex.Message);
                return new PlayerOpenResult()
                {
                    result = false,
                    message = ex.Message,
                    detail_message = ex.StackTrace
                };
            }
        }

        /// <summary>
        /// 使用FFmpeg解码播放单Dash视频
        /// </summary>
        /// <param name="url"></param>
        /// <param name="positon"></param>
        /// <param name="needConfig"></param>
        /// <returns></returns>
        public async Task<PlayerOpenResult> PlayDashUrlUseFFmpegInterop(string url, string userAgent, string referer, double positon = 0, bool needConfig = true)
        {

            try
            {
                mediaPlayerVideo.Visibility = Visibility.Visible;
                Opening = true;
                m_currentEngine = PlayEngine.FFmpegInteropMSS;

                PlayMediaType = PlayMediaType.Single;
                //加载中
                PlayState = PlayState.Loading;
                PlayStateChanged?.Invoke(this, PlayState);
                //关闭正在播放的视频
                ClosePlay();

                var _ffmpegConfig = CreateFFmpegInteropConfig(userAgent, referer);
                m_ffmpegMssVideo = await FFmpegMediaSource.CreateFromUriAsync(url, _ffmpegConfig);


                //设置时长
                Duration = m_ffmpegMssVideo.Duration.TotalSeconds;
                //设置播放器
                m_playerVideo = new MediaPlayer();
                var mediaSource = m_ffmpegMssVideo.CreateMediaPlaybackItem();
                m_playerVideo.Source = mediaSource;

                var playFailedChangeEngineArgs = new ChangePlayerEngine()
                {
                    change_engine = PlayEngine.SYEngine,
                    current_mode = PlayEngine.FFmpegInteropMSS,
                    need_change = true,
                    play_type = PlayMediaType.Single
                };

                HookVideoPlayerEvent(playFailedChangeEngineArgs);

                PlayState = PlayState.Pause;
                PlayStateChanged?.Invoke(this, PlayState);
                //设置音量
                m_playerVideo.Volume = Volume;
                //设置速率
                m_playerVideo.PlaybackSession.PlaybackRate = Rate;
                //绑定MediaPlayer
                mediaPlayerVideo.SetMediaPlayer(m_playerVideo);
                return new PlayerOpenResult()
                {
                    result = true
                };
            }
            catch (Exception ex)
            {
                //PlayMediaError?.Invoke(this, "视频加载时出错:" + ex.Message);
                return new PlayerOpenResult()
                {
                    result = false,
                    message = ex.Message,
                    detail_message = ex.StackTrace
                };
            }
        }

        /// <summary>
        /// 使用FFmpeg解码播放单FLV视频
        /// </summary>
        /// <param name="url"></param>
        /// <param name="positon"></param>
        /// <param name="needConfig"></param>
        /// <returns></returns>
        public async Task<PlayerOpenResult> PlaySingleFlvUseFFmpegInterop(string url, string userAgent, string referer, double positon = 0, bool needConfig = true)
        {

            try
            {
                mediaPlayerVideo.Visibility = Visibility.Visible;
                //vlcVideoView.Visibility = Visibility.Collapsed;
                Opening = true;
                m_currentEngine = PlayEngine.FFmpegInteropMSS;

                PlayMediaType = PlayMediaType.Single;
                //加载中
                PlayState = PlayState.Loading;
                PlayStateChanged?.Invoke(this, PlayState);
                //关闭正在播放的视频
                ClosePlay();

                var _ffmpegConfig = CreateFFmpegInteropConfig(userAgent, referer);
                m_ffmpegMssVideo = await FFmpegMediaSource.CreateFromUriAsync(url, _ffmpegConfig);


                //设置时长
                Duration = m_ffmpegMssVideo.Duration.TotalSeconds;
                //设置播放器
                m_playerVideo = new MediaPlayer();
                var mediaSource = m_ffmpegMssVideo.CreateMediaPlaybackItem();
                m_playerVideo.Source = mediaSource;

                var playFailedChangeEngineArgs = new ChangePlayerEngine()
                {
                    change_engine = PlayEngine.SYEngine,
                    current_mode = PlayEngine.FFmpegInteropMSS,
                    need_change = true,
                    play_type = PlayMediaType.Single
                };

                HookVideoPlayerEvent(playFailedChangeEngineArgs);

                PlayState = PlayState.Pause;
                PlayStateChanged?.Invoke(this, PlayState);
                //设置音量
                m_playerVideo.Volume = Volume;
                //设置速率
                m_playerVideo.PlaybackSession.PlaybackRate = Rate;
                //绑定MediaPlayer
                mediaPlayerVideo.SetMediaPlayer(m_playerVideo);
                return new PlayerOpenResult()
                {
                    result = true
                };
            }
            catch (Exception ex)
            {
                //PlayMediaError?.Invoke(this, "视频加载时出错:" + ex.Message);
                return new PlayerOpenResult()
                {
                    result = false,
                    message = ex.Message,
                    detail_message = ex.StackTrace
                };
            }
        }

        /// <summary>
        /// 使用SYEngine解码播放FLV视频
        /// </summary>
        /// <param name="url"></param>
        /// <param name="positon"></param>
        /// <param name="needConfig"></param>
        /// <param name="epId"></param>
        /// <returns></returns>
        public async Task<PlayerOpenResult> PlaySingleFlvUseSYEngine(string url, string userAgent, string referer, double positon = 0, bool needConfig = true, string epId = "")
        {

            try
            {
                mediaPlayerVideo.Visibility = Visibility.Visible;
                //vlcVideoView.Visibility = Visibility.Collapsed;
                Opening = true;
                m_currentEngine = PlayEngine.SYEngine;
                PlayMediaType = PlayMediaType.Single;
                //加载中
                PlayState = PlayState.Loading;
                PlayStateChanged?.Invoke(this, PlayState);
                //关闭正在播放的视频
                ClosePlay();
                var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);
                if (needConfig)
                {
                    playList.NetworkConfigs = CreatePlaylistNetworkConfigs(userAgent, referer, epId);
                }
                playList.Append(url, 0, 0);
                //设置播放器
                m_playerVideo = new MediaPlayer();
                m_playerVideo.Source = null;
                var mediaSource = await playList.SaveAndGetFileUriAsync();
                m_playerVideo.Source = MediaSource.CreateFromUri(mediaSource);

                var playFailedChangeEngineArgs = new ChangePlayerEngine()
                {
                    need_change = false,
                    play_type = PlayMediaType.Single,
                };

                HookVideoPlayerEvent(playFailedChangeEngineArgs, specificPlayerMediaOpenAction: () =>
                {
                    Duration = m_playerVideo.PlaybackSession.NaturalDuration.TotalSeconds;
                });

                PlayState = PlayState.Pause;
                PlayStateChanged?.Invoke(this, PlayState);
                //设置音量
                m_playerVideo.Volume = Volume;
                //设置速率
                m_playerVideo.PlaybackSession.PlaybackRate = Rate;
                //绑定MediaPlayer
                mediaPlayerVideo.SetMediaPlayer(m_playerVideo);
                return new PlayerOpenResult()
                {
                    result = true
                };
            }
            catch (Exception ex)
            {
                //PlayMediaError?.Invoke(this, "视频加载时出错:" + ex.Message);
                return new PlayerOpenResult()
                {
                    result = false,
                    message = ex.Message,
                    detail_message = ex.StackTrace
                };
            }
        }

        /// <summary>
        /// 使用SYEngine解码播放多段FLV视频
        /// </summary>
        /// <param name="url"></param>
        /// <param name="positon"></param>
        /// <param name="needConfig"></param>
        /// <param name="epId"></param>
        /// <returns></returns>
        public async Task<PlayerOpenResult> PlayVideoUseSYEngine(List<BiliFlvPlayUrlInfo> urls, string userAgent, string referer, double positon = 0, bool needConfig = true, string epId = "", bool isLocal = false)
        {
            m_currentEngine = PlayEngine.SYEngine;
            PlayMediaType = PlayMediaType.MultiFlv;
            try
            {
                mediaPlayerVideo.Visibility = Visibility.Visible;
                //vlcVideoView.Visibility = Visibility.Collapsed;
                Opening = false;
                //加载中
                PlayState = PlayState.Loading;
                PlayStateChanged?.Invoke(this, PlayState);
                ClosePlay();
                var playList = new SYEngine.Playlist(SYEngine.PlaylistTypes.NetworkHttp);
                if (needConfig)
                {
                    playList.NetworkConfigs = CreatePlaylistNetworkConfigs(userAgent, referer, epId);
                }
                foreach (var item in urls)
                {
                    playList.Append(item.Url, 0, item.Length / 1000);
                }
                //设置时长
                Duration = urls.Sum(x => x.Length / 1000);
                //设置播放器
                m_playerVideo = new MediaPlayer();
                m_playerVideo.Source = null;

                if (isLocal)
                {
                    MediaComposition composition = new MediaComposition();
                    foreach (var item in urls)
                    {
                        var file = await StorageFile.GetFileFromPathAsync(item.Url);
                        var clip = await MediaClip.CreateFromFileAsync(file);
                        composition.Clips.Add(clip);
                    }
                    m_playerVideo.Source = MediaSource.CreateFromMediaStreamSource(composition.GenerateMediaStreamSource());
                }
                else
                {
                    var mediaSource = await playList.SaveAndGetFileUriAsync();

                    m_playerVideo.Source = MediaSource.CreateFromUri(mediaSource);

                }

                var playFailedChangeEngineArgs = new ChangePlayerEngine()
                {
                    need_change = false,
                    play_type = PlayMediaType.MultiFlv,
                };

                HookVideoPlayerEvent(playFailedChangeEngineArgs);

                PlayState = PlayState.Pause;
                PlayStateChanged?.Invoke(this, PlayState);
                //设置音量
                m_playerVideo.Volume = Volume;
                //设置速率
                m_playerVideo.PlaybackSession.PlaybackRate = Rate;
                //绑定MediaPlayer
                mediaPlayerVideo.SetMediaPlayer(m_playerVideo);

                return new PlayerOpenResult()
                {
                    result = true
                };
            }
            catch (Exception ex)
            {
                return new PlayerOpenResult()
                {
                    result = false,
                    message = ex.Message,
                    detail_message = ex.StackTrace
                };
            }
        }

        public void SetRatioMode(int mode)
        {
            if (ShowShakaPlayer) return;
            switch (mode)
            {
                case 0:
                    mediaPlayerVideo.Width = double.NaN;
                    mediaPlayerVideo.Height = double.NaN;
                    mediaPlayerVideo.Stretch = Stretch.Uniform;
                    break;
                case 1:
                    mediaPlayerVideo.Width = double.NaN;
                    mediaPlayerVideo.Height = double.NaN;
                    mediaPlayerVideo.Stretch = Stretch.UniformToFill;
                    break;
                case 2:
                    mediaPlayerVideo.Stretch = Stretch.Fill;
                    mediaPlayerVideo.Height = ActualHeight;
                    mediaPlayerVideo.Width = ActualHeight * 16 / 9;
                    break;
                case 3:
                    mediaPlayerVideo.Stretch = Stretch.Fill;
                    mediaPlayerVideo.Height = ActualHeight;
                    mediaPlayerVideo.Width = ActualHeight * 4 / 3;
                    break;
                case 4:
                    if (m_dashInfo != null)
                    {
                        if (((double)ActualWidth / (double)ActualHeight) <= ((double)m_dashInfo.Video.Width / (double)m_dashInfo.Video.Height))
                        {
                            /// 原视频长宽比大于等于窗口长宽比
                            mediaPlayerVideo.Stretch = Stretch.Fill;
                            mediaPlayerVideo.Width = ActualWidth;
                            mediaPlayerVideo.Height = ActualWidth * (double)m_dashInfo.Video.Height / (double)m_dashInfo.Video.Width;
                        }
                        else {
                            /// 原视频长宽比小于窗口长宽比
                            mediaPlayerVideo.Stretch = Stretch.Fill;
                            mediaPlayerVideo.Width = ActualHeight * (double)m_dashInfo.Video.Width / (double)m_dashInfo.Video.Height;
                            mediaPlayerVideo.Height = ActualHeight;
                        }
                    }
                    else {
                        /// 未获取到DASH视频信息则不缩放
                        mediaPlayerVideo.Stretch = Stretch.None;
                        mediaPlayerVideo.Width = double.NaN;
                        mediaPlayerVideo.Height = double.NaN;
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 设置进度
        /// </summary>
        public void SetPosition(double position)
        {
            if (ShowShakaPlayer)
            {
                m_webPlayer.Seek(position);
                return;
            }
            if (m_mediaTimelineController != null)
            {
                m_mediaTimelineController.Position = TimeSpan.FromSeconds(position);
            }
            else
            {
                m_playerVideo.PlaybackSession.Position = TimeSpan.FromSeconds(position);
            }
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            try
            {
                if (ShowShakaPlayer)
                {
                    m_webPlayer.Pause();
                    PlayState = PlayState.Pause;
                    PlayStateChanged?.Invoke(this, PlayState);
                    return;
                }
                if (m_mediaTimelineController != null)
                {
                    if (m_mediaTimelineController.State == MediaTimelineControllerState.Running)
                    {
                        m_mediaTimelineController.Pause();
                        PlayState = PlayState.Pause;
                    }
                }
                else if (m_playerVideo != null)
                {
                    if (m_playerVideo.PlaybackSession.CanPause)
                    {
                        m_playerVideo.Pause();
                        PlayState = PlayState.Pause;
                    }
                }

                PlayStateChanged?.Invoke(this, PlayState);
            }
            catch (Exception ex)
            {
                _logger.Log("暂停出现错误", LogType.Error, ex);
            }

        }

        /// <summary>
        /// 播放
        /// </summary>
        public void Play()
        {
            if (Position == 0 && Duration == 0) return;
            if (ShowShakaPlayer)
            {
                m_webPlayer.Resume();
                PlayState = PlayState.Playing;
                PlayStateChanged?.Invoke(this, PlayState);
                return;
            }
            if (m_mediaTimelineController != null)
            {
                if (m_mediaTimelineController.State == MediaTimelineControllerState.Paused)
                {
                    m_mediaTimelineController.Resume();
                    PlayState = PlayState.Playing;
                }
                else
                {
                    m_mediaTimelineController.Start();
                    PlayState = PlayState.Playing;
                }
            }

            else
            {
                m_playerVideo.Play();
                PlayState = PlayState.Playing;
            }

            PlayStateChanged?.Invoke(this, PlayState);
        }

        public async Task<bool> CheckPlayUrl()
        {
            if (m_dashInfo == null) return true;
            if (!m_dashInfo.Video.Url.ToLower().StartsWith("http")) return true;
            return await m_dashInfo.Video.Url.CheckVideoUrlValidAsync(m_userAgent, m_referer);
        }

        /// <summary>
        /// 设置播放速度
        /// </summary>
        /// <param name="value"></param>
        public void SetRate(double value)
        {
            Rate = value;
            if (ShowShakaPlayer)
            {
                m_webPlayer.SetRate(value);
                return;
            }
            if (m_mediaTimelineController != null)
            {
                m_mediaTimelineController.ClockRate = value;
            }
            else
            {
                if (m_playerVideo != null)
                {
                    m_playerVideo.PlaybackSession.PlaybackRate = value;
                }
            }

        }

        /// <summary>
        /// 停止播放
        /// </summary>
        public void ClosePlay()
        {
            //全部设置为NULL
            if (mediaPlayerVideo.MediaPlayer != null)
            {
                mediaPlayerVideo.SetMediaPlayer(null);

            }
            if (mediaPlayerAudio.MediaPlayer != null)
            {
                mediaPlayerVideo.SetMediaPlayer(null);
            }
            if (m_ffmpegMssVideo != null)
            {
                m_ffmpegMssVideo.Dispose();
                m_ffmpegMssVideo = null;
            }
            if (m_ffmpegMssAudio != null)
            {
                m_ffmpegMssAudio.Dispose();
                m_ffmpegMssAudio = null;
            }
            if (m_playerVideo != null)
            {
                m_playerVideo.Source = null;
                m_playerVideo.Dispose();
                m_playerVideo = null;
            }
            if (m_playerAudio != null)
            {
                m_playerAudio.Source = null;
                m_playerAudio.Dispose();
                m_playerAudio = null;
            }
            if (m_mediaPlaybackList != null)
            {
                m_mediaPlaybackList.Items.Clear();
                m_mediaPlaybackList = null;
            }
            if (m_mediaTimelineController != null)
            {
                m_mediaTimelineController = null;
            }
            if (m_ffmpegMssItems != null)
            {
                m_ffmpegMssItems.Clear();
                m_ffmpegMssItems = null;
            }


            PlayState = PlayState.End;
            //进度设置为0
            Position = 0;
            Duration = 0;
        }

        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="volume"></param>
        public void SetVolume(double volume)
        {

            if (mediaPlayerAudio.MediaPlayer != null)
            {
                mediaPlayerAudio.MediaPlayer.Volume = volume;
            }
            if (mediaPlayerVideo.MediaPlayer != null)
            {
                mediaPlayerVideo.MediaPlayer.Volume = volume;
            }
        }

        public string GetMediaInfo()
        {
            try
            {
                var info = "";
                switch (PlayMediaType)
                {
                    case PlayMediaType.Single:
                        info += $"Type: single_video\r\n";
                        break;
                    case PlayMediaType.MultiFlv:
                        info += $"Type: multi_video\r\n";
                        break;
                    case PlayMediaType.Dash:
                        info += $"Type: dash\r\n";
                        break;
                    default:
                        break;
                }
                info += $"Engine: {m_currentEngine.ToString()}\r\n";
                if (m_ffmpegMssVideo != null)
                {
                    info += $"Resolution: {m_ffmpegMssVideo.CurrentVideoStream.PixelHeight} x {m_ffmpegMssVideo.CurrentVideoStream.PixelWidth}\r\n";
                    info += $"Video Codec: {m_ffmpegMssVideo.CurrentVideoStream.CodecName}\r\n";
                    info += $"Video Bitrate: {m_ffmpegMssVideo.CurrentVideoStream.Bitrate}\r\n";
                    info += $"Average Frame: {((double)m_ffmpegMssVideo.CurrentVideoStream.FramesPerSecond).ToString("0.0")}\r\n";
                    if (PlayMediaType == PlayMediaType.Dash)
                    {
                        info += $"Audio Codec: {m_ffmpegMssAudio.AudioStreams[0].CodecName}\r\n";
                        info += $"Audio Bitrate: {m_ffmpegMssAudio.AudioStreams[0].Bitrate}";
                    }
                    else
                    {
                        info += $"Audio Codec: {m_ffmpegMssVideo.AudioStreams[0].CodecName}\r\n";
                        info += $"Audio Bitrate: {m_ffmpegMssVideo.AudioStreams[0].Bitrate}";
                    }
                }
                else
                {
                    //info += $"Resolution: {_playerVideo.PlaybackSession.NaturalVideoHeight} x {_playerVideo.PlaybackSession.NaturalVideoWidth}\r\n";
                    if (m_dashInfo != null && m_dashInfo.Audio != null)
                    {
                        if (ShowMediaPlayer)
                        {
                            info += $"Resolution: {m_dashInfo.Video.Width} x {m_dashInfo.Video.Height}\r\n";
                            info +=
                                $"View Resolution: {(int)mediaPlayerVideo.Width} x {(int)mediaPlayerVideo.Height}\r\n";
                        }

                        info += $"Video Codec: {m_dashInfo.Video.Codecs}\r\n";
                        info += $"Video DataRate: {(m_dashInfo.Video.BandWidth / 1024).ToString("0.0")}Kbps\r\n";
                        info += $"Average Frame: {m_dashInfo.Video.FrameRate}\r\n";
                        info += $"Audio Codec: {m_dashInfo.Audio.Codecs}\r\n";
                        info += $"Audio DataRate: {(m_dashInfo.Audio.BandWidth / 1024).ToString("0.0")}Kbps\r\n";
                        info += $"Video Host: {m_dashInfo.Video.Host}\r\n";
                        info += $"Audio Host: {m_dashInfo.Audio.Host}\r\n";
                        //info += $"Video Url: {m_dashInfo.Video.Url}\r\n";
                        //info += $"Audio Url: {m_dashInfo.Audio.Url}\r\n";
                    }
                    else if (m_playerVideo != null)
                    {
                        info += $"Resolution: {m_playerVideo.PlaybackSession.NaturalVideoWidth} x {m_playerVideo.PlaybackSession.NaturalVideoHeight}\r\n";
                    }
                }

                if (m_webPlayerStatsData != null)
                {
                    info += $"Resolution: {m_webPlayerStatsData.Width} x {m_webPlayerStatsData.Height}\r\n";
                    info += $"DecodedFrames: {m_webPlayerStatsData.DecodedFrames}\r\n";
                    info += $"DroppedFrames: {m_webPlayerStatsData.DroppedFrames}\r\n";
                }
                //MediaInfo = info;
                return info;
            }
            catch (Exception)
            {
                //MediaInfo = "读取失败";
                return "读取视频信息失败";
            }

        }

        public void Dispose()
        {
            try
            {
                ClosePlay();
                m_webPlayer?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Warn("播放器关闭失败，重试", ex);
                ClosePlay();
            }
            //try
            //{
            //    _vlcMediaPlayer?.Media?.Dispose();

            //    _vlcMediaPlayer?.Dispose();

            //    _vlcMediaPlayer=null;
            //    _libVLC?.Dispose();
            //    _libVLC = null;
            //}
            //catch (Exception)
            //{
            //}

        }

        public void SetVideoEnable(bool enable)
        {
            try
            {
                if (enable)
                {
                    m_playerVideo.TimelineController = m_mediaTimelineController;
                    mediaPlayerVideo.Visibility = Visibility.Visible;
                }
                else
                {
                    m_playerVideo.TimelineController = null;
                    m_playerVideo.Pause();
                    mediaPlayerVideo.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message, ex);
            }
        }

        public void OpenDevMode()
        {
            if (ShowShakaPlayer)
            {
                WebPlayer.OpenDevMode();
            }
        }

        #endregion
    }
}
