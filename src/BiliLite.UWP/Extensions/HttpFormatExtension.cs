namespace BiliLite.Extensions
{
    static public class HttpFormatExtension
    {
        static public string EnsureHttpsPrefix(string url)
        {
            // 将中文句号替换为英文句号
            string correctedInput = url.Replace("。", ".");

            // 移除原有协议头（如果有）
            int protocolIndex = correctedInput.IndexOf("://");
            string domainPart = protocolIndex != -1 ?
                correctedInput.Substring(protocolIndex + 3) :
                correctedInput;

            // 强制添加 www 前缀（如果没有）
            if (!domainPart.StartsWith("www."))
            {
                domainPart = "www." + domainPart;
            }

            // 添加 HTTPS 协议并返回
            return $"https://{domainPart}";
        }

        static public readonly string BiliPattern = @"^https?://(?:www\.|m\.|t\.|h\.|vc\.|account\.|passport\.|bangumi\.|live\.|space\.|search\.|message\.)?bilibili\.com(/.*)?$";
        static public readonly string GeneralPattern = @"^(https?://)?([a-zA-Z0-9-]+[.|。])?([a-zA-Z0-9-]+[.|。])[a-zA-Z]{2,3}(\/.*)?$";
    }
}
