using BiliLite.Extensions;
using BiliLite.Models.Common;
using BiliLite.Services;

namespace BiliLite.Models.Requests.Api
{
    public class GitApi
    {
        /// <summary>
        /// 检查更新
        /// </summary>
        /// <returns></returns>
        public ApiModel CheckUpdate()
        {
            var updateJsonAddress = SettingService.GetValue(SettingConstants.Other.UPDATE_JSON_ADDRESS,
                                                              UpdateJsonAddressOptions.DEFAULT_UPDATE_JSON_ADDRESS);
            updateJsonAddress = updateJsonAddress.Replace("\"", ""); // 解决取出的值有奇怪的转义符
            var api = new ApiModel()
            {
                method = HttpMethods.Get,
                baseUrl = $"{updateJsonAddress}/document/new_version.json",
                parameter = $"ts={TimeExtensions.GetTimestampS()}"
            };
            return api;
        }
    }
}
