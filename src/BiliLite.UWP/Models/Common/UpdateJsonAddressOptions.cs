using BiliLite.Services;
using System.Linq;

namespace BiliLite.Models.Common
{
    public class UpdateJsonAddressOptions
    {
        public static UpdateJsonAddressOption[] Options = new[]
        {
            new UpdateJsonAddressOption() { Name = "github", Value = ApiHelper.GIT_RAW_URL },
            new UpdateJsonAddressOption() { Name = "[镜像]ghproxy", Value = ApiHelper.GHPROXY_GIT_RAW_URL },
            new UpdateJsonAddressOption() { Name = "[镜像]kgithub", Value = ApiHelper.KGITHUB_GIT_RAW_URL },
        };

        // 优先使用更稳定的ghproxy
        public const string DEFAULT_UPDATE_JSON_ADDRESS = ApiHelper.GHPROXY_GIT_RAW_URL;

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
