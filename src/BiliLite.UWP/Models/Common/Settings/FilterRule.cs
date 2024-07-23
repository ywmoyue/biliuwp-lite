namespace BiliLite.Models.Common.Settings
{
    public class FilterRule
    {
        public string Id { get; set; }

        public FilterRuleType FilterRuleType { get; set; }

        public FilterType FilterType { get; set; }

        public FilterContentType ContentType { get; set; }

        public string Rule { get; set; }

        public bool Enable { get; set; }
    }
}
