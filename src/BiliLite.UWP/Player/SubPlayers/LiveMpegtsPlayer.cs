using BiliLite.Models.Common.Player;
using BiliLite.Models.Common;
using BiliLite.Player.MediaInfos;
using BiliLite.Player.WebPlayer.Models;
using BiliLite.Player.WebPlayer;
using BiliLite.Services;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace BiliLite.Player.SubPlayers;

public class LiveMpegtsPlayer : ISubPlayer, ISubWebPlayer
{
    private MpegtsPlayerControl m_playerControl;
    private PlayerConfig m_playerConfig;
    private string m_url;
    private double m_position;

    public LiveMpegtsPlayer(PlayerConfig playerConfig, MpegtsPlayerControl playerControl)
    {
        m_playerConfig = playerConfig;
        m_playerControl = playerControl;
        InitPlayerEvent();
    }

    public override RealPlayerType Type { get; } = RealPlayerType.Mpegts;

    public BaseWebPlayer WebPlayer => m_playerControl;

    public override double Volume
    {
        get => m_playerControl.Volume;
        set => m_playerControl.SetVolume(value);
    }

    public override double Position => m_position;

    public override event EventHandler MediaOpened;
    public override event EventHandler MediaEnded;
    public override event EventHandler BufferingStarted;
    public override event EventHandler BufferingEnded;
    public override event EventHandler<double> PositionChanged;

    public override CollectInfo GetCollectInfo()
    {
        return new CollectInfo()
        {
            Data = new ShakaPlayerCollectInfoData()
            {
                WebPlayer = m_playerControl,
            },
            RealPlayInfo = m_realPlayInfo,
            Type = "ShakaPlayer",
            Url = m_url,
        };
    }

    public override async Task Load()
    {
        var urls = m_realPlayInfo.PlayUrls;
        // Mpegts 不支持 hls流
        if (urls.FlvUrls == null)
        {
            if (urls.HlsUrls != null)
            {
                EmitError(PlayerError.PlayerErrorCode.NeedUseOtherPlayerError, "需要切换其他播放器", PlayerError.RetryStrategy.Normal);
                return;
            }
            EmitError(PlayerError.PlayerErrorCode.PlayUrlError, "获取播放地址失败", PlayerError.RetryStrategy.NoRetry);
        }

        var defaultPlayerMode = m_playerConfig.PlayMode;
        var selectRouteLine = m_playerConfig.SelectedRouteLine;
        var url = "";
        var manualUrl = m_realPlayInfo.ManualPlayUrl;

        if (!string.IsNullOrEmpty(manualUrl) && SettingService.GetValue(SettingConstants.Live.LOW_DELAY_MODE,
                SettingConstants.Live.DEFAULT_LOW_DELAY_MODE))
        {
            url = manualUrl;
        }
        else
        {
            url = urls.FlvUrls[selectRouteLine].Url;
        }

        m_url = url;

        await m_playerControl.LoadLiveUrl(m_url, defaultPlayerMode.ToString());
        await SetRate(m_rate);
    }

    public override async Task Buff()
    {
    }

    public override async Task Play()
    {
        await m_playerControl.Resume();
    }

    public override async Task Stop()
    {
        await StopCore();
    }

    public override async Task Fault()
    {
        await StopCore();
    }

    public override async Task Pause()
    {
        await m_playerControl.Pause();
    }

    public override async Task Resume()
    {
        await m_playerControl.Resume();
    }

    public override async Task SetRate(double value)
    {
        m_rate = value;
        await m_playerControl.SetRate(value);
    }

    public override async Task SetPosition(double value)
    {
        m_position = value;
        await m_playerControl.Seek(value);
    }

    private async Task StopCore()
    {
        await m_playerControl.Pause();
    }

    private void InitPlayerEvent()
    {
        m_playerControl.PlayerLoaded += ShakaPlayerControl_PlayerLoaded;
        m_playerControl.Ended += ShakaPlayerControlOnEnded;
        m_playerControl.PositionChanged += ShakaPlayerControlOnPositionChanged;
    }

    private void ShakaPlayerControlOnPositionChanged(object sender, double e)
    {
        m_position = e;
        PositionChanged?.Invoke(this, e);
    }

    private void ShakaPlayerControlOnEnded(object sender, EventArgs e)
    {
        MediaEnded?.Invoke(this, EventArgs.Empty);
    }

    private async void ShakaPlayerControl_PlayerLoaded(object sender, ShakaPlayerLoadedData e)
    {
        MediaOpened?.Invoke(this, EventArgs.Empty);
        await Task.Delay(50);
        BufferingStarted?.Invoke(this, EventArgs.Empty);
        await Task.Delay(50);
        BufferingEnded?.Invoke(this, EventArgs.Empty);
    }
}
