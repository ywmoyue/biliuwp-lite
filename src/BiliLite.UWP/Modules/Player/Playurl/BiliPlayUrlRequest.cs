﻿using Bilibili.App.Playurl.V1;
using BiliLite.Extensions;
using BiliLite.gRPC.Api;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Video;
using BiliLite.Models.Common.Video.PlayUrlInfos;
using BiliLite.Models.Requests.Api;
using BiliLite.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PlayURL = BiliLite.gRPC.Api.PlayURL;

namespace BiliLite.Modules.Player.Playurl
{
    class BiliPlayUrlRequest
    {
        protected bool IsDownload { get; set; } = false;
        protected readonly PlayerAPI playerAPI = new PlayerAPI();
        protected readonly gRPC.Api.PlayURL playUrlApi = new PlayURL();
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        /// <summary>
        /// 是否开启替换CDN选项
        /// </summary>
        protected int ReplaceCDNMode { get; }

        /// <summary>
        /// 替换的CDN
        /// </summary>
        protected string CDN { get; }
        /// <summary>
        /// 是否大会员，非大会员最高只能看1080P
        /// </summary>
        protected bool IsVIP { get; }
        /// <summary>
        /// 选择的编码模式
        /// </summary>
        protected PlayUrlCodecMode CodecMode { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Message { get; set; } = "";

        public const string WebUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36";
        public const string WebReferer = "https://www.bilibili.com";
        public const string AndroidUserAgent = "Bilibili Freedoooooom/MarkII";


        public BiliPlayUrlRequest(bool isDownload)
        {
            IsDownload = isDownload;
            //PriorityAkamaiCDN = SettingService.GetValue<bool>(SettingConstants.Roaming.AKAMAI_CDN, false);
            ReplaceCDNMode = SettingService.GetValue<int>(SettingConstants.Roaming.REPLACE_CDN, 3);
            CDN = SettingService.GetValue<string>(SettingConstants.Roaming.CDN_SERVER, "upos-sz-mirrorhwo1.bilivideo.com");
            IsVIP = (SettingService.Account.Logined && SettingService.Account.Profile.vip != null && SettingService.Account.Profile.vip.status != 0);
            CodecMode = (PlayUrlCodecMode)SettingService.GetValue(IsDownload ? SettingConstants.Download.DEFAULT_VIDEO_TYPE : SettingConstants.Player.DEFAULT_VIDEO_TYPE, (int)DefaultVideoTypeOptions.DEFAULT_VIDEO_TYPE);
        }
        protected void AddMessage(string type, string msg)
        {
            Message += $"{type}：{msg}\r\n";
        }
        protected async Task<bool> CheckUrlAvailable(string url, string userAgent, string referer)
        {
            using (HttpClient client = new HttpClient() { Timeout = TimeSpan.FromSeconds(2) })
            {
                try
                {
                    if (userAgent != null && userAgent.Length > 0)
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", userAgent);
                    }
                    if (referer != null && referer.Length > 0)
                    {
                        client.DefaultRequestHeaders.Add("Referer", referer);
                    }
                    client.DefaultRequestHeaders.Add("Range", "bytes=0-9");
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return false;
                }
            }
        }
        protected async Task<string> ReplaceCDN(string url, string userAgent, string referer)
        {

            Regex regex = new Regex(@"http://|https://?([^/]*)");
            var host = regex.Match(url).Groups[1].Value;
            var replaceUrl = url.Replace(host, CDN);
            if (await CheckUrlAvailable(replaceUrl, userAgent, referer))
            {
                return replaceUrl;
            }
            else
            {
                return url;
            }
        }

        private void ParseBiliPlayUrlInfoSupportFormats(BiliPlayUrlQualitesInfo info, JObject playUrlInfoResult, string userAgent, string referer)
        {
            var timeLength = playUrlInfoResult["timelength"].ToInt32();
            foreach (var item in playUrlInfoResult["support_formats"])
            {
                info.Qualites.Add(new BiliPlayUrlInfo()
                {
                    UserAgent = userAgent,
                    Referer = referer,
                    QualityID = item["quality"].ToInt32(),
                    QualityName = item["new_description"].ToString(),
                    Timelength = timeLength,
                    HasPlayUrl = false,
                });
            }
            info.Qualites = [.. info.Qualites.OrderBy(x => x.QualityID)];
        }

        private async Task<BiliDashItem> ParseBiliPlayUrlInfoAudioDash(BiliPlayUrlQualitesInfo info, JObject playUrlInfoResult,
            int quality, List<int> qualites, string userAgent, string referer, bool isProxy, int soundQualityId = 0)
        {
            if (!playUrlInfoResult.TryGetValue("dash", out var dashData))
            {
                _logger.Warn("获取Dash数据失败");
                return null;
            }

            if (!((JObject)dashData).TryGetValue("audio", out var audiosToken))
            {
                _logger.Warn("获取音频播放地址失败");
                return null;
            }

            var audios = JsonConvert.DeserializeObject<List<DashItemModel>>(audiosToken.ToString());

            if (audios == null)
            {
                _logger.Warn("获取音频播放地址失败");
                return null;
            }

            BiliFlacItem flacAudio;

            var flacJToken = playUrlInfoResult["dash"]["flac"];
            flacAudio = flacJToken == null ? null : JsonConvert.DeserializeObject<BiliFlacItem>(flacJToken.ToString());

            BiliDolbyItem dolbyAudio;

            var dolbyJToken = playUrlInfoResult["dash"]["dolby"];
            dolbyAudio = dolbyJToken == null ? null : JsonConvert.DeserializeObject<BiliDolbyItem>(dolbyJToken.ToString());

            var qn = quality;

            var timeLength = playUrlInfoResult["timelength"].ToInt32();
            info.AudioQualites = new List<BiliDashAudioPlayUrlInfo>();
            // 处理普通音质列表
            foreach (var audio in audios)
            {
                audio.BaseUrl = await HandleUrl(audio.BaseUrl, audio.BackupUrl, userAgent, referer, isProxy);
                info.AudioQualites.Add(new BiliDashAudioPlayUrlInfo()
                {
                    HasPlayUrl = true,
                    QualityID = audio.Id,
                    QualityName = SoundQualityConstants.Dictionary[audio.Id],
                    Referer = referer,
                    UserAgent = userAgent,
                    Timelength = timeLength,
                    Audio = audio.ToBiliDashItem(),
                });
            }

            info.AudioQualites = [.. info.AudioQualites.OrderBy(x => x.QualityID)];

            // 处理无损音质
            if (flacAudio is { Display: true, Audio: { } })
            {
                var audio = flacAudio.Audio;
                audio.BaseUrl = await HandleUrl(audio.BaseUrl, audio.BackupUrl, userAgent, referer, isProxy);
                info.AudioQualites.Add(new BiliDashAudioPlayUrlInfo()
                {
                    HasPlayUrl = true,
                    QualityID = audio.Id,
                    QualityName = SoundQualityConstants.Dictionary[audio.Id],
                    Referer = referer,
                    UserAgent = userAgent,
                    Timelength = timeLength,
                    Audio = audio.ToBiliDashItem(),
                });
            }

            // 处理杜比音效
            if (dolbyAudio is { Audio: { } } && dolbyAudio.Audio.Count > 0)
            {
                var audio = dolbyAudio.Audio[0];
                audio.BaseUrl = await HandleUrl(audio.BaseUrl, audio.BackupUrl, userAgent, referer, isProxy);
                info.AudioQualites.Add(new BiliDashAudioPlayUrlInfo()
                {
                    HasPlayUrl = true,
                    QualityID = audio.Id,
                    QualityName = SoundQualityConstants.Dictionary[audio.Id],
                    Referer = referer,
                    UserAgent = userAgent,
                    Timelength = timeLength,
                    Audio = audio.ToBiliDashItem(),
                });
            }

            BiliDashItem currentAudio = null;

            //部分视频没有音频文件
            if (audios.Any())
            {
                var audioQuality = info.AudioQualites.FirstOrDefault(x => x.QualityID == soundQualityId);
                var defaultAudio = SoundQualityConstants.GetDefaultAudio(info.AudioQualites);
                info.CurrentAudioQuality = audioQuality ?? defaultAudio;
                currentAudio = info.CurrentAudioQuality.Audio;
            }
            return currentAudio;
        }

        private async Task<BiliPlayUrlQualitesInfo> ParseBiliPlayUrlInfoDash(BiliPlayUrlQualitesInfo info, JObject playUrlInfoResult,
            int quality, List<int> qualites, string userAgent, string referer, bool isProxy, int soundQualityId = 0)
        {
            var videos = JsonConvert.DeserializeObject<List<DashItemModel>>(playUrlInfoResult["dash"]["video"].ToString());
            var currentAudio = await ParseBiliPlayUrlInfoAudioDash(info, playUrlInfoResult, quality, qualites, userAgent, referer, isProxy, soundQualityId);
            var qn = quality;
            var h264Videos = videos.Where(x => x.Codecid == (int)BiliPlayUrlVideoCodec.AVC);
            var h265Videos = videos.Where(x => x.Codecid == (int)BiliPlayUrlVideoCodec.HEVC);
            var av01Videos = videos.Where(x => x.Codecid == (int)BiliPlayUrlVideoCodec.AV1);

            var duration = playUrlInfoResult["dash"]["duration"].ToInt32();
            var minBufferTime = playUrlInfoResult["dash"]["minBufferTime"].ToString();

            if (qn > qualites.Max())
            {
                qn = qualites.Max();
            }
            if (!qualites.Contains(qn))
            {
                qn = qualites.Max();
            }
            foreach (var item in info.Qualites)
            {
                item.PlayUrlType = BiliPlayUrlType.DASH;
                var video = h264Videos.FirstOrDefault(x => x.Id == item.QualityID);
                var h265_video = h265Videos.FirstOrDefault(x => x.Id == item.QualityID);
                var av1_video = av01Videos.FirstOrDefault(x => x.Id == item.QualityID);
                //h265处理
                switch (CodecMode)
                {
                    case PlayUrlCodecMode.DASH_H265 when h265_video != null:
                        video = h265_video;
                        break;
                    case PlayUrlCodecMode.DASH_AV1:
                        //部分清晰度可能没有av1编码，切换至hevc
                        video = av1_video ?? h265_video ?? video;
                        break;
                }
                //没有视频，跳过此清晰度
                if (video == null)
                {
                    //info.Qualites.Remove(item);
                    continue;
                }
                //替换链接
                video.BaseUrl = await HandleUrl(video.BaseUrl, video.BackupUrl, userAgent, referer, isProxy);

                item.Codec = (BiliPlayUrlVideoCodec)video.Codecid;
                item.HasPlayUrl = true;

                item.DashInfo = new BiliDashPlayUrlInfo()
                {
                    Audio = currentAudio,
                    Video = video.ToBiliDashItem(),
                };
            }
            //移除没有链接的视频
            info.Qualites = info.Qualites.Where(x => x.HasPlayUrl).ToList();
            if (!IsVIP)
            {
                //非大会员，去除大会员专享清晰度
                info.Qualites = info.Qualites.Where(x => x.QualityID != 74 && x.QualityID <= 80).ToList();
            }

            var current = info.Qualites.FirstOrDefault(x => x.QualityID == qn);
            if (current == null)
            {
                current = info.Qualites.OrderByDescending(x => x.QualityID).FirstOrDefault(x => x.HasPlayUrl);
            }
            info.CurrentQuality = current;
            return info;
        }

        private async Task<BiliPlayUrlQualitesInfo> ParseBiliPlayUrlDurl(BiliPlayUrlQualitesInfo info, JObject playUrlInfoResult,
            int quality, List<int> qualites, string userAgent, string referer, bool isProxy)
        {
            var durl = JsonConvert.DeserializeObject<List<FlvDurlModel>>(playUrlInfoResult["durl"].ToString());
            var index = qualites.IndexOf(quality);
            if (index == -1)
            {
                index = 0;
            }
            //替换链接
            foreach (var item in durl)
            {
                item.url = await HandleUrl(item.url, item.backup_url, userAgent, referer, isProxy);
            }
            info.Qualites[index].Codec = BiliPlayUrlVideoCodec.AVC;
            info.Qualites[index].PlayUrlType = durl.Count == 1 ? BiliPlayUrlType.SingleFLV : BiliPlayUrlType.MultiFLV;
            info.Qualites[index].FlvInfo = durl.Select(x => x.ToBiliFlvItem()).ToList();
            info.Qualites[index].HasPlayUrl = true;


            info.CurrentQuality = info.Qualites[index];
            return info;
        }

        protected async Task<BiliPlayUrlQualitesInfo> ParseJson(int quality, JObject obj, string userAgent, string referer, bool isProxy, int soundQualityId = 0)
        {
            try
            {
                var info = new BiliPlayUrlQualitesInfo();
                info.Qualites = new List<BiliPlayUrlInfo>();

                ParseBiliPlayUrlInfoSupportFormats(info, obj, userAgent, referer);
                var qualites = info.Qualites.Select(x => x.QualityID).ToList();
                if (obj.ContainsKey("dash") && obj["dash"]["video"] != null)
                {
                    return await ParseBiliPlayUrlInfoDash(info, obj, quality, qualites, userAgent, referer, isProxy, soundQualityId);
                }
                else if (obj.ContainsKey("durl"))
                {
                    return await ParseBiliPlayUrlDurl(info, obj, quality, qualites, userAgent, referer, isProxy);
                }
                else
                {
                    return BiliPlayUrlQualitesInfo.Failure("无法读取播放链接");
                }
            }
            catch (Exception ex)
            {
                return BiliPlayUrlQualitesInfo.Failure(ex.Message);
            }

        }

        protected async Task<BiliPlayUrlQualitesInfo> ParseGrpc(int quality, PlayViewReply obj, string userAgent, string referer)
        {
            try
            {
                BiliPlayUrlQualitesInfo info = new BiliPlayUrlQualitesInfo();
                info.Qualites = new List<BiliPlayUrlInfo>();

                var timeLength = (long)obj.VideoInfo.Timelength;

                foreach (var item in obj.VideoInfo.StreamList)
                {
                    info.Qualites.Add(new BiliPlayUrlInfo()
                    {
                        UserAgent = userAgent,
                        Referer = referer,
                        QualityID = (int)item.StreamInfo.Quality,
                        QualityName = item.StreamInfo.NewDescription,
                        Timelength = timeLength,
                        HasPlayUrl = false,
                    });
                }
                var qualites = info.Qualites.Select(x => x.QualityID).ToList();

                if (obj.VideoInfo.DashAudio != null && obj.VideoInfo.DashAudio.Count > 0)
                {
                    List<DashItemModel> videos = new List<DashItemModel>();
                    List<DashItemModel> audios = new List<DashItemModel>();

                    foreach (var item in obj.VideoInfo.StreamList)
                    {
                        if (item.DashVideo == null)
                        {
                            continue;
                        }
                        var codecs = "avc1.640032";
                        var initialization = "0-995";
                        var indexRange = "996-4639";
                        if (item.DashVideo.Codecid == 12)
                        {
                            codecs = "hev1.1.6.L150.90";
                            initialization = "0-105";
                            indexRange = "1060-4703";
                        }
                        if (item.DashVideo.Codecid == 13)
                        {
                            codecs = "av01.0.08M.08.0.110.01.01.01.0";
                            initialization = "0-939";
                            indexRange = "1328-4983";
                        }
                        videos.Add(new DashItemModel()
                        {
                            BackupUrl = item.DashVideo.BackupUrl.ToList(),
                            BaseUrl = item.DashVideo.BaseUrl,
                            Bandwidth = (int)item.DashVideo.Bandwidth,
                            Codecid = (int)item.DashVideo.Codecid,
                            MimeType = "video/mp4",
                            Id = (int)item.StreamInfo.Quality,
                            StartWithSap = 1,
                            Sar = "",
                            Codecs = codecs,
                            FrameRate = "",
                            SegmentBase = new SegmentBase()
                            {
                                IndexRange = indexRange,
                                Initialization = initialization
                            }
                        });
                    }
                    foreach (var item in obj.VideoInfo.DashAudio)
                    {

                        audios.Add(new DashItemModel()
                        {
                            BackupUrl = item.BackupUrl.ToList(),
                            BaseUrl = item.BaseUrl,
                            Bandwidth = (int)item.Bandwidth,
                            Codecid = (int)item.Codecid,
                            MimeType = "audio/mp4",
                            Id = (int)item.Id,
                            Codecs = "mp4a.40.2",
                            StartWithSap = 0,
                            SegmentBase = new SegmentBase()
                            {
                                IndexRange = "0-907",
                                Initialization = "908-4575"
                            }
                        });
                    }


                    var h264Videos = videos.Where(x => x.Codecid == (int)BiliPlayUrlVideoCodec.AVC);
                    var h265Videos = videos.Where(x => x.Codecid == (int)BiliPlayUrlVideoCodec.HEVC);
                    var av01Videos = videos.Where(x => x.Codecid == (int)BiliPlayUrlVideoCodec.AV1);

                    var duration = (timeLength / 1000).ToInt32();


                    var qn = quality;
                    if (qn > qualites.Max())
                    {
                        qn = qualites.Max();
                    }
                    if (!qualites.Contains(qn))
                    {
                        qn = qualites.Max();
                    }
                    for (int i = 0; i < info.Qualites.Count; i++)
                    {
                        var item = info.Qualites[i];
                        item.PlayUrlType = BiliPlayUrlType.DASH;
                        var video = h264Videos.FirstOrDefault(x => x.Id == item.QualityID);
                        var h265_video = h265Videos.FirstOrDefault(x => x.Id == item.QualityID);
                        var av1_video = av01Videos.FirstOrDefault(x => x.Id == item.QualityID);
                        //h265处理
                        if (CodecMode == PlayUrlCodecMode.DASH_H265 && h265_video != null)
                        {
                            video = h265_video;
                        }
                        //av1处理
                        if (CodecMode == PlayUrlCodecMode.DASH_AV1)
                        {
                            //部分清晰度可能没有av1编码，切换至hevc
                            if (av1_video != null)
                            {

                                video = av1_video;
                            }
                            else if (h265_video != null)
                            {
                                video = h265_video;
                            }
                        }
                        //没有视频，跳过此清晰度
                        if (video == null)
                        {

                            //info.Qualites.Remove(item);
                            continue;
                        }
                        DashItemModel audio = null;
                        //部分视频没有音频文件
                        if (audios != null && audios.Count > 0)
                        {
                            if (qn > 64)
                            {
                                audio = audios.LastOrDefault();
                            }
                            else
                            {
                                audio = audios.FirstOrDefault();
                            }
                        }
                        //替换链接
                        video.BaseUrl = await HandleUrl(video.BaseUrl, video.BackupUrl, userAgent, referer, false);
                        if (audio != null)
                        {
                            audio.BaseUrl = await HandleUrl(audio.BaseUrl, audio.BackupUrl, userAgent, referer, false);
                        }

                        item.Codec = (BiliPlayUrlVideoCodec)video.Codecid;
                        item.HasPlayUrl = true;
                        item.DashInfo = new BiliDashPlayUrlInfo()
                        {
                            Audio = audio?.ToBiliDashItem(),
                            Video = video.ToBiliDashItem(),
                        };
                    }
                    //移除没有链接的视频
                    info.Qualites = info.Qualites.Where(x => x.HasPlayUrl).ToList();
                    if (!IsVIP)
                    {
                        //非大会员，去除大会员专享清晰度
                        info.Qualites = info.Qualites.Where(x => x.QualityID != 74 && x.QualityID <= 80).ToList();
                    }
                    var current = info.Qualites.FirstOrDefault(x => x.QualityID == qn);
                    if (current == null)
                    {
                        current = info.Qualites.OrderByDescending(x => x.QualityID).FirstOrDefault(x => x.HasPlayUrl);
                    }
                    info.CurrentQuality = current;
                    return info;
                }
                else if (obj.VideoInfo.StreamList.FirstOrDefault(x => x.SegmentVideo != null) != null)
                {
                    var video = obj.VideoInfo.StreamList.FirstOrDefault(x => x.SegmentVideo != null);

                    List<FlvDurlModel> durl = new List<FlvDurlModel>();
                    foreach (var item in video.SegmentVideo.Segment)
                    {
                        durl.Add(new FlvDurlModel()
                        {
                            backup_url = item.BackupUrl.ToList(),
                            length = (int)item.Length,
                            order = (int)item.Order,
                            size = (int)item.Size,
                            url = item.Url
                        });

                    }
                    //替换链接
                    foreach (var item in durl)
                    {
                        item.url = await HandleUrl(item.url, item.backup_url, userAgent, referer, false);
                    }
                    var index = qualites.IndexOf(quality);
                    if (index == -1)
                    {
                        index = 0;
                    }

                    info.Qualites[index].Codec = BiliPlayUrlVideoCodec.AVC;
                    info.Qualites[index].PlayUrlType = durl.Count == 1 ? BiliPlayUrlType.SingleFLV : BiliPlayUrlType.MultiFLV;
                    info.Qualites[index].FlvInfo = durl.Select(x => x.ToBiliFlvItem()).ToList();
                    info.Qualites[index].HasPlayUrl = true;


                    info.CurrentQuality = info.Qualites[index];
                    return info;
                }
                else
                {
                    return BiliPlayUrlQualitesInfo.Failure("无法读取播放链接");
                }
            }
            catch (Exception ex)
            {
                return BiliPlayUrlQualitesInfo.Failure(ex.Message);
            }

        }
        public virtual Task<BiliPlayUrlQualitesInfo> GetPlayUrlInfo(PlayInfo playInfo, int qualityID, int soundQualityId = 0)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 处理链接
        /// 获取Akamai\替换CDN\替换PCDN
        /// </summary>
        /// <returns></returns>
        private async Task<string> HandleUrl(string url, List<string> backupUrl, string userAgent, string referer, bool isProxy)
        {
            var isPCDN = false;
            var optimizationUrl = url;
            var uri = new Uri(url);
            if (!uri.Host.Contains("bilivideo.com") && !uri.Host.Contains("akamaized.net"))
            {
                isPCDN = true;
                //设置优选链接（非PCDN）
                foreach (var item in backupUrl)
                {
                    var _host = new Uri(item).Host;
                    if (_host.Contains("bilivideo.com") || _host.Contains("akamaized.net"))
                    {
                        optimizationUrl = item;
                        break;
                    }
                }
            }
            //未开启CDN替换
            if (ReplaceCDNMode == 0)
            {
                return optimizationUrl;
            }
            //全部替换
            if (ReplaceCDNMode == 1)
            {
                return await ReplaceCDN(optimizationUrl, userAgent, referer);
            }
            //只替换PCDN
            if (ReplaceCDNMode == 2 && isPCDN)
            {
                return await ReplaceCDN(optimizationUrl, userAgent, referer);
            }
            //只替换代理
            if (ReplaceCDNMode == 3 && isProxy)
            {
                return await ReplaceCDN(optimizationUrl, userAgent, referer);
            }
            return optimizationUrl;
        }
    }
    class BiliVideoPlayUrlRequest : BiliPlayUrlRequest
    {
        public BiliVideoPlayUrlRequest(bool isDownload) : base(isDownload)
        {
        }

        public override async Task<BiliPlayUrlQualitesInfo> GetPlayUrlInfo(PlayInfo playInfo, int qualityID, int soundQualityId = 0)
        {
            //尝试WEB API读取播放地址
            var webResult = await GetPlayUrlUseWebApi(playInfo, qualityID, soundQualityId);
            if (webResult.Success)
            {
                return webResult;
            }
            AddMessage("[/x/player/playurl]", webResult.Message);
            //尝试GRPC API读取地址
            var grpcResult = await GetPlayUrlUseGrpc(playInfo, qualityID);
            if (grpcResult.Success)
            {
                return grpcResult;
            }
            AddMessage("[/v1.PlayURL/PlayView]", grpcResult.Message);
            return BiliPlayUrlQualitesInfo.Failure(Message);
        }

        private async Task<BiliPlayUrlQualitesInfo> GetPlayUrlUseWebApi(PlayInfo playInfo, int qualityID, int soundQualityId = 0)
        {
            try
            {
                var webApiResult = await (playerAPI.VideoPlayUrl(aid: playInfo.avid, cid: playInfo.cid, qn: qualityID, dash: CodecMode != PlayUrlCodecMode.FLV, false, playInfo.area)).Request();
                if (!webApiResult.status)
                {
                    return BiliPlayUrlQualitesInfo.Failure(webApiResult.message);
                }
                var data = await webApiResult.GetData<JObject>();
                if (data.code != 0)
                {
                    return BiliPlayUrlQualitesInfo.Failure(data.message);
                }
                var jsonResult = await ParseJson(qualityID, data.data, WebUserAgent, WebReferer, false, soundQualityId);
                return jsonResult;
            }
            catch (Exception ex)
            {
                return BiliPlayUrlQualitesInfo.Failure(ex.ToString());
            }

        }
        private async Task<BiliPlayUrlQualitesInfo> GetPlayUrlUseGrpc(PlayInfo playInfo, int qualityID)
        {
            try
            {
                Bilibili.App.Playurl.V1.CodeType codec = CodecMode == PlayUrlCodecMode.DASH_H265 ? Bilibili.App.Playurl.V1.CodeType.Code265 : Bilibili.App.Playurl.V1.CodeType.Code264;

                var requestUserInfo = new GrpcBiliUserInfo(
                    SettingService.Account.AccessKey,
                    SettingService.Account.UserID,
                    SettingService.Account.GetLoginAppKeySecret().Appkey);
                var playViewReply = await playUrlApi.VideoPlayView(Convert.ToInt64(playInfo.avid), Convert.ToInt64(playInfo.cid), qualityID, 16, codec, requestUserInfo);

                var grpcResult = await ParseGrpc(qualityID, playViewReply, AndroidUserAgent, "");
                return grpcResult;
            }
            catch (Exception ex)
            {
                return BiliPlayUrlQualitesInfo.Failure(ex.ToString());
            }

        }
    }
    class BiliSeasonPlayUrlRequest : BiliPlayUrlRequest
    {
        public BiliSeasonPlayUrlRequest(bool isDownload) : base(isDownload)
        {
        }

        public override async Task<BiliPlayUrlQualitesInfo> GetPlayUrlInfo(PlayInfo playInfo, int qualityID, int soundQualityId = 0)
        {
            //尝试WEB API读取播放地址
            // 按此顺序，访问代理
            string[] proxyAreas = new string[] { "", "cn", "hk", "tw" };
            if (playInfo.area == "hk")
            {
                proxyAreas = new string[] { "", "hk", "tw", "cn" };
            }
            else if (playInfo.area == "tw")
            {
                proxyAreas = new string[] { "", "tw", "hk", "cn" };
            }
            foreach (var item in proxyAreas)
            {
                var webResult = await GetPlayUrlUseWebApi(playInfo, qualityID, area: item, soundQualityId: soundQualityId);
                if (webResult.Success)
                {
                    return webResult;
                }
                var areaName = "无代理";
                switch (item)
                {
                    case "hk":
                        areaName = "香港代理";
                        break;
                    case "cn":
                        areaName = "大陆代理";
                        break;
                    case "tw":
                        areaName = "台湾代理";
                        break;
                    default:
                        break;
                }
                AddMessage($"WebAPI-{areaName}", webResult.Message);
            }


            //尝试GRPC API读取地址
            var grpcResult = await GetPlayUrlUseGrpc(playInfo, qualityID);
            if (grpcResult.Success)
            {
                return grpcResult;
            }
            AddMessage("[/v1.PlayURL/PlayView]", grpcResult.Message);
            return BiliPlayUrlQualitesInfo.Failure(Message);

        }

        private async Task<BiliPlayUrlQualitesInfo> GetPlayUrlUseWebApi(PlayInfo playInfo, int qualityID, string area = "", int soundQualityId = 0)
        {
            try
            {
                var webApiResult = await (playerAPI.SeasonPlayUrl(aid: playInfo.avid, cid: playInfo.cid, ep_id: playInfo.ep_id, qn: qualityID, season_type: playInfo.season_type, dash: CodecMode != PlayUrlCodecMode.FLV, area != "", area)).Request();
                if (!webApiResult.status)
                {
                    return BiliPlayUrlQualitesInfo.Failure(webApiResult.message);
                }
                var data = await webApiResult.GetResult<JObject>();
                if (data.code != 0)
                {
                    return BiliPlayUrlQualitesInfo.Failure(data.message);
                }
                var jsonResult = await ParseJson(qualityID, data.result, WebUserAgent, WebReferer, area != "", soundQualityId);
                return jsonResult;
            }
            catch (Exception ex)
            {
                return BiliPlayUrlQualitesInfo.Failure(ex.Message);
            }

        }
        private async Task<BiliPlayUrlQualitesInfo> GetPlayUrlUseGrpc(PlayInfo playInfo, int qualityID)
        {
            try
            {

                CodeType codec = CodecMode == PlayUrlCodecMode.DASH_H265 ? CodeType.Code265 : CodeType.Code264;

                var requestUserInfo = new GrpcBiliUserInfo(
                    SettingService.Account.AccessKey,
                    SettingService.Account.UserID,
                    SettingService.Account.GetLoginAppKeySecret().Appkey);
                var playViewReply = await playUrlApi.BangumiPlayView(Convert.ToInt64(playInfo.ep_id), Convert.ToInt64(playInfo.cid), qualityID, 0, codec, requestUserInfo);

                var grpcResult = await ParseGrpc(qualityID, playViewReply, AndroidUserAgent, "");
                return grpcResult;
            }
            catch (Exception ex)
            {

                return BiliPlayUrlQualitesInfo.Failure(ex.Message);
            }
        }
    }

    public class DashItemModel
    {
        private string m_baseUrl;
        private List<string> m_backupUrl;
        private string m_mimeType;
        private string m_frameRate;
        private SegmentBase m_segmentBase;
        private int? m_startWithSap;

        public int Id { get; set; }
        public int Bandwidth { get; set; }

        public string BaseUrl
        {
            get => m_baseUrl ?? BaseUrlV2;
            set => m_baseUrl = value;
        }
        [JsonProperty("base_url")]
        public string BaseUrlV2 { get; set; }

        public List<string> BackupUrl
        {
            get => m_backupUrl ?? BackupUrlV2;
            set => m_backupUrl = value;
        }
        [JsonProperty("backup_url")]
        public List<string> BackupUrlV2 { get; set; }
        public string MimeType
        {
            get => m_mimeType ?? MimeTypeV2;
            set => m_mimeType = value;
        }

        [JsonProperty("mime_type")]
        public string MimeTypeV2 { get; set; }

        public string Codecs { get; set; }
        public int Codecid { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public string FrameRate
        {
            get => m_frameRate ?? FrameRateV2;
            set => m_frameRate = value;
        }

        [JsonProperty("frame_rate")]
        public string FrameRateV2 { get; set; }

        public int StartWithSap
        {
            get => m_startWithSap ?? StartWithSapV2;
            set => m_startWithSap = value;
        }

        [JsonProperty("start_with_sap")]
        public int StartWithSapV2 { get; set; }

        public string Sar { get; set; }
        /// <summary>
        /// 计算平均帧数
        /// </summary>
        public string Fps
        {
            get
            {
                try
                {
                    if (!string.IsNullOrEmpty(FrameRate) && FrameRate.Contains("/"))
                    {
                        var values = FrameRate.Split('/');
                        if (values.Length == 1)
                        {
                            return FrameRate;
                        }
                        double r = Convert.ToDouble(values[0]);
                        double d = Convert.ToDouble(values[1]);
                        return (r / d).ToString("0.0");
                    }
                    else
                    {
                        return FrameRate;
                    }
                }
                catch (Exception)
                {
                    return FrameRate;
                }

            }
        }

        public SegmentBase SegmentBase
        {
            get => m_segmentBase ?? SegmentBaseV2;
            set => m_segmentBase = value;
        }

        [JsonProperty("segment_base")]
        public SegmentBase SegmentBaseV2 { get; set; }

        public BiliDashItem ToBiliDashItem()
        {
            return new BiliDashItem()
            {
                BandWidth = Bandwidth,
                CodecID = Codecid,
                Codecs = Codecs,
                FrameRate = FrameRate,
                Height = Height,
                Width = Width,
                ID = Id,
                IsVideo = Width != 0,
                MimeType = MimeType,
                Sar = Sar,
                StartWithSap = StartWithSap,
                SegmentBaseIndexRange = SegmentBase.IndexRange,
                SegmentBaseInitialization = SegmentBase.Initialization,
                Url = BaseUrl,
            };
        }
    }

    public class SegmentBase
    {
        private string m_indexRange;

        public string Initialization { get; set; }

        public string IndexRange
        {
            get => m_indexRange ?? IndexRangeV2;
            set => m_indexRange = value;
        }

        [JsonProperty("index_range")]
        public string IndexRangeV2 { get; set; }
    }

    public class FlvDurlModel
    {
        public List<string> backup_url { get; set; }
        public string url { get; set; }
        public int order { get; set; }
        public long size { get; set; }
        public long length { get; set; }
        public BiliFlvPlayUrlInfo ToBiliFlvItem()
        {
            return new BiliFlvPlayUrlInfo()
            {
                Url = url,
                Length = length,
                Size = size,
                Order = order,
            };
        }
    }
}
