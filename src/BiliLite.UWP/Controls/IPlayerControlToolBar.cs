using BiliLite.Models.Common.Video.PlayUrlInfos;
using BiliLite.Services;
using System;
using System.Collections.Generic;

namespace BiliLite.Controls
{
    public interface IPlayerControlToolBar
    {
        event EventHandler<BiliDashAudioPlayUrlInfo> SoundQualityChanged;
        event EventHandler<BiliPlayUrlInfo> QualityChanged;
        event EventHandler<double> PlaySpeedChanged;
        PlayerToastService PlayerToastService { set; }
        bool FirstMediaOpened { get; set; }
        void InitSoundQuality(List<BiliDashAudioPlayUrlInfo> audioQualites, BiliDashAudioPlayUrlInfo currentAudioQuality);
        void InitQuality(List<BiliPlayUrlInfo> videoQualites, BiliPlayUrlInfo currentQuality);
        void InitPlaySpeed();
        void SlowDown();
        void FastUp();
        double GetPlaySpeed();
        void SetPlaySpeed(double speed);
    }
}
