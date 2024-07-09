using System.Collections.Generic;

namespace BiliLite.Models.Common.Settings
{
    public class FilterRuleTypes
    {
        public static List<FilterTypeOption> FilterTypeOptions = new List<FilterTypeOption>()
        {
            new FilterTypeOption(FilterType.Word, "关键词"),
            new FilterTypeOption(FilterType.Regular, "正则"),
        };
        public static List<FilterTargetOption> FilterTargetOptions = new List<FilterTargetOption>()
        {
            new FilterTargetOption(FilterContentType.Title, "标题"),
            new FilterTargetOption(FilterContentType.User, "用户"),
        };
    }

    public class FilterTypeOption
    {
        public FilterTypeOption(){}

        public FilterTypeOption(FilterType type,string text)
        {
            Type = type;
            Text = text;
        }

        public FilterType Type { get; set; }

        public string Text { get; set; }
    }

    public class FilterTargetOption
    {
        public FilterTargetOption() { }

        public FilterTargetOption(FilterContentType type, string text)
        {
            Type = type;
            Text = text;
        }

        public FilterContentType Type { get; set; }

        public string Text { get; set; }
    }
}
