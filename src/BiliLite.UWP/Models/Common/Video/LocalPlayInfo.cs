﻿using System.Collections.Generic;
using BiliLite.Models.Common.Video.PlayUrlInfos;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace BiliLite.Models.Common.Video
{
    public class LocalPlayInfo
    {
        public List<BiliPlayUrlInfo> VideoTrackInfos { get; set; }

        public List<BiliDashAudioPlayUrlInfo> AudioTrackInfos { get; set; }

        public BiliDashAudioPlayUrlInfo CurrentAudioTrack { get; set; }

        public BiliPlayUrlInfo CurrentVideoTrack { get; set; }

        public BiliPlayUrlInfo Info { get; set; }
        public IDictionary<string, string> Subtitles { get; set; }
        public string DanmakuPath { get; set; }
        public string Quality { get; set; }
    }
}
