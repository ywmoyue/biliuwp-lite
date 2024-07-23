using System;
using System.Threading.Tasks;
using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Models.Common.Account;
using BiliLite.Models.Requests.Api;

namespace BiliLite.Services
{
    public class WbiKeyService
    {
        private static async Task<WbiKey> GetNewWbiKeys()
        {
            // 获取最新的 img_key 和 sub_key
            var response = await new AccountApi().Nav().Request();
            var result = await response.GetData<WebInterfaceNav>();
            var imgUrl = result.data.WbiImg.ImgUrl;
            var subUrl = result.data.WbiImg.SubUrl;
            var imgKey = imgUrl.Substring(imgUrl.LastIndexOf('/') + 1).Split('.')[0];
            var subKey = subUrl.Substring(subUrl.LastIndexOf('/') + 1).Split('.')[0];

            var currentTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            SettingService.SetValue(SettingConstants.Account.WBI_IMG_KEY, imgKey);
            SettingService.SetValue(SettingConstants.Account.WBI_SUB_KEY, subKey);
            SettingService.SetValue(SettingConstants.Account.WBI_KEY_TIME, currentTime);

            return new WbiKey(imgKey, subKey);
        }

        private WbiKey GetCurrentWbiKeys()
        {
            var imgKey = SettingService.GetValue(SettingConstants.Account.WBI_IMG_KEY, string.Empty);
            var subKey = SettingService.GetValue(SettingConstants.Account.WBI_SUB_KEY, string.Empty);
            return new WbiKey(imgKey, subKey);
        }

        public async Task<WbiKey> GetWbiKeys()
        {
            var lastKeySaveTimestamp = SettingService.GetValue<long>(SettingConstants.Account.WBI_KEY_TIME, 0);
            if (lastKeySaveTimestamp <= 0) return await GetNewWbiKeys();
            var lastKeySaveTime = DateTimeOffset.FromUnixTimeSeconds(lastKeySaveTimestamp);
            if ((DateTimeOffset.Now - lastKeySaveTime) >=
                TimeSpan.FromMinutes(SettingConstants.Account.WBI_KEY_REFRESH_TIME))
            {
                var newKeys = await GetNewWbiKeys();
                return newKeys;
            }
            else
            {
                var wbiKey = GetCurrentWbiKeys();
                if (wbiKey.ImgKey != string.Empty && wbiKey.SubKey != string.Empty) return wbiKey;
                var newKeys = await GetNewWbiKeys();
                return newKeys;
            }
        }
    }
}
