namespace BiliLite.Models.Common.Live
{
    public static class DefaultPlayUrlSourceOptions
    {
        public static LivePlayUrlSourceOption[] DefaultLivePlayUrlSourceOption = new LivePlayUrlSourceOption[]
        {
            new LivePlayUrlSourceOption() { Name = "京东云MCDN(PCDN?)", Value = "mcdn" },
            new LivePlayUrlSourceOption() { Name = "B站自建地区CDN", Value = "cn-"},
            new LivePlayUrlSourceOption() { Name = "备用及第三方CDN", Value = "-gotcha"},
        };

        public const string DEFAULT_PLAY_URL_SOURCE = "cn-"; //默认使用B站自建CDN
    }

    public class LivePlayUrlSourceOption
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }
}
