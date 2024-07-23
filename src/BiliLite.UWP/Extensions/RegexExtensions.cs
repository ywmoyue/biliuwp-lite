using System;
using System.Text.RegularExpressions;

namespace BiliLite.Extensions
{
    public static class RegexExtensions
    {
        public static bool IsValidRegex(this string regexString)
        {
            if (string.IsNullOrEmpty(regexString)) return false;

            try
            {
                var regex = new Regex(regexString);
                return true;
            }
            catch (Exception)
            {
                // 语法不正确的正则表达式会导致ArgumentException异常
                return false;
            }
        }
    }
}
