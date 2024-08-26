using System.Collections.Generic;
using System.Linq;
using BiliLite.Models.Common.Video.PlayUrlInfos;
using BiliLite.Services;

namespace BiliLite.Models.Common.Video
{
    public static class SoundQualityConstants
    {
        /// <summary>
        /// 音质Id,音质名称
        /// 根据音质大小从低到高排序
        /// </summary>
        private static Dictionary<int, string> m_dictionary = new Dictionary<int, string>()
        {
            {30216,"64K" },
            {30232,"132K" },
            {30280,"192K" },
            {30250,"杜比" },
            {30251,"无损" }
        };

        public static Dictionary<int, string> Dictionary
        {
            get
            {
                return m_dictionary;
            }
        }

        public static BiliDashAudioPlayUrlInfo GetDefaultAudio(List<BiliDashAudioPlayUrlInfo> audios)
        {
            var enableMaxQuality = SettingService.GetValue(
                SettingConstants.Player.ENABLE_DEFAULT_MAX_SOUND_QUALITY,
                SettingConstants.Player.DEFAULT_ENABLE_DEFAULT_MAX_SOUND_QUALITY
            );

            var preferredQualityIds = enableMaxQuality
                ? new[] { 30251, 30250, 30280, 30232, 30216 }
                : new[] { 30280, 30232, 30216, 30251, 30250 };

            foreach (var qualityId in preferredQualityIds)
            {
                var audio = audios.FirstOrDefault(x => x.QualityID == qualityId);
                if (audio != null)
                    return audio;
            }

            return audios.FirstOrDefault();
        }
    }
}
