using BiliLite.Models.Common;
using BiliLite.Player.SubPlayers;
using BiliLite.Services;
using FFmpegInteropX;

namespace BiliLite.Extensions
{
    public static class PlayerExtensions
    {
        public static MediaSourceConfig ReloadFFmpegConfig(this MediaSourceConfig ffmpegConfig, string userAgent, string referer)
        {
            var passThrough = SettingService.GetValue<bool>(SettingConstants.Player.HARDWARE_DECODING, true);
            if (!string.IsNullOrEmpty(userAgent) && !ffmpegConfig.FFmpegOptions.ContainsKey("user_agent"))
            {
                ffmpegConfig.FFmpegOptions.Add("user_agent", userAgent);
            }
            if (!string.IsNullOrEmpty(referer) && !ffmpegConfig.FFmpegOptions.ContainsKey("referer"))
            {
                ffmpegConfig.FFmpegOptions.Add("referer", referer);
            }

            ffmpegConfig.VideoDecoderMode = passThrough ? VideoDecoderMode.ForceSystemDecoder : VideoDecoderMode.ForceFFmpegSoftwareDecoder;
            return ffmpegConfig;
        }
    }
}
