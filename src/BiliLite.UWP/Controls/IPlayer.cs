using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Player;
using BiliLite.Models.Common.Video.PlayUrlInfos;
using BiliLite.Services;

namespace BiliLite.Controls
{
    public interface IPlayer
    {
        PlayState PlayState { get; set; }
        PlayMediaType PlayMediaType { get; set; }
        VideoPlayHistoryHelper.ABPlayHistoryEntry ABPlay { get; set; }

        /// <summary>
        /// 进度
        /// </summary>
        double Position { get; set; }

        /// <summary>
        /// 时长
        /// </summary>
        double Duration { get; set; }

        /// <summary>
        /// 音量0-1
        /// </summary>
        double Volume { get; set; }

        /// <summary>
        /// 是否缓冲中
        /// </summary>
        bool Buffering { get; set; }

        /// <summary>
        /// 缓冲进度,0-100
        /// </summary>
        double BufferCache { get; set; }

        /// <summary>
        /// 播放速度
        /// </summary>
        double Rate { get; set; }

        /// <summary>
        /// 媒体信息
        /// </summary>
        string MediaInfo { get; set; }

        bool Opening { get; set; }
        event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 播放状态变更
        /// </summary>
        event EventHandler<PlayState> PlayStateChanged;

        /// <summary>
        /// 媒体加载完成
        /// </summary>
        event EventHandler PlayMediaOpened;

        /// <summary>
        /// 播放完成
        /// </summary>
        event EventHandler PlayMediaEnded;

        /// <summary>
        /// 播放错误
        /// </summary>
        event EventHandler<string> PlayMediaError;

        /// <summary>
        /// 更改播放引擎
        /// </summary>
        event EventHandler<ChangePlayerEngine> ChangeEngine;

        /// <summary>
        /// 使用AdaptiveMediaSource播放视频
        /// </summary>
        /// <returns></returns>
        Task<PlayerOpenResult> PlayerDashUseNative(BiliDashPlayUrlInfo dashInfo, string userAgent, string referer, double positon = 0);

        /// <summary>
        /// 使用eMediaSource播放视频
        /// </summary>
        /// <param name="videoUrl"></param>
        /// <param name="audioUrl"></param>
        /// <param name="positon"></param>
        /// <param name="needConfig"></param>
        /// <returns></returns>
        Task<PlayerOpenResult> PlayerSingleMp4UseNativeAsync(string url, double positon = 0, bool needConfig = true, bool isLocal = false);

        /// <summary>
        /// 使用FFmpegInterop解码播放音视频分离视频
        /// </summary>
        /// <param name="videoUrl"></param>
        /// <param name="audioUrl"></param>
        /// <param name="positon"></param>
        /// <param name="needConfig"></param>
        /// <returns></returns>
        Task<PlayerOpenResult> PlayDashUseFFmpegInterop(BiliDashPlayUrlInfo dashPlayUrlInfo, string userAgent, string referer, double positon = 0, bool needConfig = true, bool isLocal = false);

        /// <summary>
        /// 使用FFmpeg解码播放单Dash视频
        /// </summary>
        /// <param name="url"></param>
        /// <param name="positon"></param>
        /// <param name="needConfig"></param>
        /// <returns></returns>
        Task<PlayerOpenResult> PlayDashUrlUseFFmpegInterop(string url, string userAgent, string referer, double positon = 0, bool needConfig = true);

        /// <summary>
        /// 使用FFmpeg解码播放单FLV视频
        /// </summary>
        /// <param name="url"></param>
        /// <param name="positon"></param>
        /// <param name="needConfig"></param>
        /// <returns></returns>
        Task<PlayerOpenResult> PlaySingleFlvUseFFmpegInterop(string url, string userAgent, string referer, double positon = 0, bool needConfig = true);

        /// <summary>
        /// 使用SYEngine解码播放FLV视频
        /// </summary>
        /// <param name="url"></param>
        /// <param name="positon"></param>
        /// <param name="needConfig"></param>
        /// <param name="epId"></param>
        /// <returns></returns>
        Task<PlayerOpenResult> PlaySingleFlvUseSYEngine(string url, string userAgent, string referer, double positon = 0, bool needConfig = true, string epId = "");

        /// <summary>
        /// 使用SYEngine解码播放多段FLV视频
        /// </summary>
        /// <param name="url"></param>
        /// <param name="positon"></param>
        /// <param name="needConfig"></param>
        /// <param name="epId"></param>
        /// <returns></returns>
        Task<PlayerOpenResult> PlayVideoUseSYEngine(List<BiliFlvPlayUrlInfo> urls, string userAgent, string referer, double positon = 0, bool needConfig = true, string epId = "", bool isLocal = false);

        void SetRatioMode(int mode);

        /// <summary>
        /// 设置进度
        /// </summary>
        void SetPosition(double position);

        Task Load(BasePlayInfo basePlayInfo);

        /// <summary>
        /// 暂停
        /// </summary>
        void Pause();

        /// <summary>
        /// 播放
        /// </summary>
        void Play();

        /// <summary>
        /// 设置播放速度
        /// </summary>
        /// <param name="value"></param>
        void SetRate(double value);

        /// <summary>
        /// 停止播放
        /// </summary>
        void ClosePlay();

        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="volume"></param>
        void SetVolume(double volume);

        string GetMediaInfo();
        void Dispose();
    }
}