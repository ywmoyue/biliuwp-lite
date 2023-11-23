using BiliLite.Models.Common;
using BiliLite.Services;

namespace BiliLite.Models.Requests.Api
{
    public class BaseApi
    {
        protected ApiKeyInfo AppKey => SettingService.Account.GetLoginAppKeySecret();
    }
}
