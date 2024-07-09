using BiliLite.Models.Common;
using System;
using System.Linq;
using Windows.UI.Xaml.Data;
using BiliLite.Models.Common.Settings;

namespace BiliLite.Converters
{
    public class FilterRuleTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is FilterType filterType)
            {
                return FilterRuleTypes.FilterTypeOptions.FirstOrDefault(x => x.Type == filterType);
            }
            else if(value is FilterContentType contentType)
            {
                return FilterRuleTypes.FilterTargetOptions.FirstOrDefault(x => x.Type == contentType);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is FilterTypeOption filterType)
            {
                return filterType.Type;
            }
            else if (value is FilterTargetOption contentType)
            {
                return contentType.Type;
            }

            if (targetType == typeof(FilterType))
            {
                return FilterType.Word;
            }
            if (targetType == typeof(FilterContentType))
            {
                return FilterContentType.Title;
            }

            return null;
        }
    }
}
