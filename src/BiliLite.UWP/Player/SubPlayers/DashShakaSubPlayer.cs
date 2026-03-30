using System;
using System.Threading.Tasks;
using BiliLite.Models.Common.Player;
using BiliLite.Player.MediaInfos;
using BiliLite.Player.WebPlayer;
using BiliLite.Player.WebPlayer.Models;
using Windows.UI.Xaml;

namespace BiliLite.Player.SubPlayers
{
    public class DashShakaSubPlayer : ISubPlayer, ISubWebPlayer
    {
        private readonly ShakaPlayerControl m_playerControl;
        private string m_url;
        private double m_position;
        private bool m_isBuffering;
        private double m_bufferCache = 1;
        private bool m_isMuted;

        public DashShakaSubPlayer(ShakaPlayerControl playerControl)
        {
            m_playerControl = playerControl;
            InitPlayerEvent();
        }

        public override RealPlayerType Type { get; } = RealPlayerType.ShakaPlayer;

        public BaseWebPlayer WebPlayer => m_playerControl;

        public override double Volume
        {
            get => m_playerControl.Volume;
            set => m_playerControl.SetVolume(value);
        }

        public override double Position => m_position;

        public override bool IsMuted
        {
            get => m_isMuted;
            set => m_isMuted = value;
        }

        public override bool IsBuffering => m_isBuffering;

        public override double BufferCache => m_bufferCache;

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
            if (m_realPlayInfo?.DashInfo?.Video?.Url == null)
            {
                EmitError(PlayerError.PlayerErrorCode.PlayUrlError, "Dash 视频地址为空", PlayerError.RetryStrategy.NoRetry);
                return;
            }

            m_url = m_realPlayInfo.DashInfo.Video.Url;
            var audioUrl = m_realPlayInfo.DashInfo.Audio?.Url ?? string.Empty;
            await m_playerControl.LoadUrl(m_url, audioUrl, m_realPlayInfo.IsLocal);
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
            await m_playerControl.Pause();
        }

        public override async Task Fault()
        {
            await m_playerControl.Pause();
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

        public override async Task SetMuted(bool muted)
        {
            await base.SetMuted(muted);
            if (muted)
            {
                await m_playerControl.SetVolume(0);
                return;
            }

            var volume = m_lastVolumeBeforeMuted <= 0 ? 1 : m_lastVolumeBeforeMuted;
            await m_playerControl.SetVolume(volume);
        }

        public override async Task SetVideoEnable(bool enable)
        {
            m_playerControl.Visibility = enable ? Visibility.Visible : Visibility.Collapsed;
            if (!enable)
            {
                await m_playerControl.Pause();
            }
        }

        public override async Task<byte[]> CaptureAsync()
        {
            return await m_playerControl.CaptureVideo();
        }

        private void InitPlayerEvent()
        {
            m_playerControl.PlayerLoaded += PlayerControlOnPlayerLoaded;
            m_playerControl.Ended += PlayerControlOnEnded;
            m_playerControl.PositionChanged += PlayerControlOnPositionChanged;
        }

        private void PlayerControlOnPositionChanged(object sender, double e)
        {
            m_position = e;
            PositionChanged?.Invoke(this, e);
        }

        private void PlayerControlOnEnded(object sender, EventArgs e)
        {
            MediaEnded?.Invoke(this, EventArgs.Empty);
        }

        private async void PlayerControlOnPlayerLoaded(object sender, ShakaPlayerLoadedData e)
        {
            MediaOpened?.Invoke(this, EventArgs.Empty);
            await Task.Delay(50);
            m_isBuffering = true;
            m_bufferCache = 0;
            EmitBufferCacheChanged(m_bufferCache);
            BufferingStarted?.Invoke(this, EventArgs.Empty);
            await Task.Delay(50);
            m_isBuffering = false;
            m_bufferCache = 1;
            EmitBufferCacheChanged(m_bufferCache);
            BufferingEnded?.Invoke(this, EventArgs.Empty);
        }
    }
}
