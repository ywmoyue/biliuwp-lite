using BiliLite.Models.Common;
using BiliLite.ViewModels.Common;
using PropertyChanged;

namespace BiliLite.ViewModels.Settings
{
    public class FilterRuleViewModel : BaseViewModel
    {
        public string Id { get; set; }

        public FilterRuleType FilterRuleType { get; set; }

        public FilterType FilterType { get; set; }

        public FilterContentType ContentType { get; set; }

        public string Rule { get; set; }

        public bool Enable { get; set; } = true;

        [DependsOn(nameof(FilterType))]
        public string RuleDesc
        {
            get
            {
                return FilterType switch
                {
                    FilterType.Word => "输入关键词",
                    FilterType.Regular => "输入正则表达式",
                    _ => ""
                };
            }
        }

        [DoNotNotify]
        public string FilterTypeDesc => "选择使用关键词过滤或正则表达式过滤";

        [DoNotNotify]
        public string FilterContentTypeDesc
        {
            get
            {
                var desc = "选择过滤目标对象属性";
                if (FilterRuleType == FilterRuleType.Dynamic)
                {
                    return desc + "(动态项过滤标题指过滤投稿标题)";
                }
                if (FilterRuleType == FilterRuleType.Recommend)
                {
                    return desc + "(推荐项不支持过滤详情)";
                }

                return desc;
            }
        }
    }
}
