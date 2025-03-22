using BiliLite.Services;
using System.Linq;

namespace BiliLite.Models.Common
{
    public class UpdateJsonAddressOptions
    {
        public static UpdateJsonAddressOption[] Options = new[]
        {
            new UpdateJsonAddressOption() { Name = "github", Value = ApiHelper.GIT_RAW_URL },
            new UpdateJsonAddressOption() { Name = "[镜像] jsDelivr", Value = ApiHelper.JSDELIVR_GIT_RAW_URL},
            new UpdateJsonAddressOption() { Name = "[镜像] ghproxy", Value = ApiHelper.GHPROXY_GIT_RAW_URL },
            new UpdateJsonAddressOption() { Name = "[镜像] kgithub", Value = ApiHelper.KGITHUB_GIT_RAW_URL },
        };

        // 默认使用最稳定的jsdelivr
        public const string DEFAULT_UPDATE_JSON_ADDRESS = ApiHelper.JSDELIVR_GIT_RAW_URL;

        public static UpdateJsonAddressOption GetOption(string type)
        {
            return Options.FirstOrDefault(x => x.Value == type);
        }
    }

    public class UpdateJsonAddressOption
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }
}
