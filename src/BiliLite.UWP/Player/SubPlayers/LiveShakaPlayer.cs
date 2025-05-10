using System;
using System.Threading.Tasks;
using BiliLite.Controls;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Player;
using BiliLite.Player.MediaInfos;
using BiliLite.Player.WebPlayer;
using BiliLite.Player.WebPlayer.Models;
using BiliLite.Services;

namespace BiliLite.Player.SubPlayers
{
    public class LiveShakaPlayer : ISubPlayer, ISubWebPlayer
    {
        private ShakaPlayerControl m_ShakaPlayerControl;
        private PlayerConfig m_playerConfig;
        private string m_url;

        public LiveShakaPlayer(PlayerConfig playerConfig, ShakaPlayerControl shakaPlayerControl)
        {
            m_playerConfig = playerConfig;
            m_ShakaPlayerControl = shakaPlayerControl;
            InitPlayerEvent();
        }

        public override RealPlayerType Type { get; } = RealPlayerType.ShakaPlayer;

        public BaseWebPlayer WebPlayer => m_ShakaPlayerControl;

        public override double Volume
        {
            get => m_ShakaPlayerControl.Volume;
            set => m_ShakaPlayerControl.SetVolume(value);
        }

        public override event EventHandler MediaOpened;
        public override event EventHandler MediaEnded;
        public override event EventHandler BufferingStarted;
        public override event EventHandler BufferingEnded;

        public override CollectInfo GetCollectInfo()
        {
            return new CollectInfo()
            {
                Data = new ShakaPlayerCollectInfoData()
                {
                    WebPlayer = m_ShakaPlayerControl,
                },
                RealPlayInfo = m_realPlayInfo,
                Type = "ShakaPlayer",
                Url = m_url,
            };
        }

        public override async Task Load()
        {
            var urls = m_realPlayInfo.PlayUrls;
            // shakaPlayer 不支持 flv流
            if (urls.HlsUrls == null)
            {
                if (urls.FlvUrls != null)
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
                url = urls.HlsUrls[selectRouteLine].Url;
            }

            m_url = url;

            await m_ShakaPlayerControl.LoadLiveUrl(m_url, defaultPlayerMode.ToString());
        }

        public override async Task Buff()
        {
        }

        public override async Task Play()
        {
            await m_ShakaPlayerControl.Resume();
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
            await m_ShakaPlayerControl.Pause();
        }

        public override async Task Resume()
        {
            await m_ShakaPlayerControl.Resume();
        }

        private async Task StopCore()
        {
            await m_ShakaPlayerControl.Pause();
        }

        private void InitPlayerEvent()
        {
            m_ShakaPlayerControl.PlayerLoaded += ShakaPlayerControl_PlayerLoaded;
            m_ShakaPlayerControl.Ended += ShakaPlayerControlOnEnded;
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
}