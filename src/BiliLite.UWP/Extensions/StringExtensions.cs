using Newtonsoft.Json;
using System.Text;
using System;
using BiliLite.Models.Common;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Markup;
using OpenCCNET;
using BiliLite.Services;
using Windows.UI;
using Windows.Security.Cryptography.Core;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml;
using BiliLite.Models.Exceptions;

namespace BiliLite.Extensions
{
    /// <summary>
    /// cc转为SRT
    /// </summary>
    /// <param name="json">CC字幕</param>
    /// <param name="toSimplified">转为简体</param>
    /// <returns></returns>
    public static class StringExtensions
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        public static string CcConvertToSrt(this string json, bool toSimplified = false)
        {
            var subtitle = JsonConvert.DeserializeObject<Subtitle>(json);
            var stringBuilder = new StringBuilder();
            var i = 1;
            foreach (var item in subtitle.body)
            {
                var start = TimeSpan.FromSeconds(item.from);
                var end = TimeSpan.FromSeconds(item.to);
                stringBuilder.AppendLine(i.ToString());
                stringBuilder.AppendLine($"{start:hh\\:mm\\:ss\\,fff} --> {end:hh\\:mm\\:ss\\,fff}");
                var content = item.content;
                if (toSimplified)
                {
                    content = content.ToHansFromTW(true);
                }

                stringBuilder.AppendLine(content);
                stringBuilder.AppendLine();
                i++;
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// 简体转繁体
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string SimplifiedToTraditional(this string input)
        {
            return input.ToHKFromHans();
        }

        /// <summary>
        /// 繁体转简体
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string TraditionalToSimplified(this string input)
        {
            return input.ToHansFromTW(true);
        }

        /// <summary>
        /// 文本转富文本控件
        /// </summary>
        /// <param name="txt"></param>
        /// <param name="emote"></param>
        /// <returns></returns>
        public static RichTextBlock ToRichTextBlock(this string txt, JObject emote, bool isLive = false,
            string fontColor = null, string fontWeight = "Normal", string lowProfilePrefix = "",
            string textAlignment = "Left", bool enableVideoSeekTime = false)
        {
            var input = txt;
            try
            {
                if (txt != null)
                {
                    //处理特殊字符
                    input = input.Replace("&", "&amp;");
                    input = input.Replace("<", "&lt;");
                    input = input.Replace(">", "&gt;");
                    input = input.Replace("\r\n", "<LineBreak/>");
                    input = input.Replace("\n", "<LineBreak/>");
                    //处理其他控制字符
                    input = Regex.Replace(input, @"[\p{Cc}\p{Cf}]", string.Empty);

                    //处理链接
                    if (!isLive) { input = HandelUrl(input); }

                    //处理时间坐标
                    if (enableVideoSeekTime)
                    {
                        input = HandleTimeSeek(input);
                    }
                    
                    //处理表情
                    input = !isLive ? HandelEmoji(input, emote) : HandleLiveEmoji(input, emote);

                    //处理av号/bv号
                    if (!isLive) { input = HandelVideoID(input); }

                    if (!string.IsNullOrEmpty(lowProfilePrefix))
                    {
                        lowProfilePrefix = $"<Run Foreground=\"{{ThemeResource LowProfileTextColor}}\" Text=\"{lowProfilePrefix}\"></Run>";
                    }

                    //生成xaml
                    var xaml = string.Format(@"<RichTextBlock HorizontalAlignment=""Stretch"" xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                                            xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" xmlns:d=""http://schemas.microsoft.com/expression/blend/2008""
                                            xmlns:mc = ""http://schemas.openxmlformats.org/markup-compatibility/2006"" LineHeight=""{1}"" {2} {3}>
                                            <Paragraph {5}>{4}{0}</Paragraph>
                                            </RichTextBlock>",  input, 
                                                                isLive ? 22 : 20,
                                                                fontColor == null ? "" : $"Foreground=\"{fontColor}\"",
                                                                $"FontWeight=\"{fontWeight}\"",
                                                                lowProfilePrefix,
                                                                $"TextAlignment=\"{textAlignment}\"");
                    if (!xaml.IsXmlString())
                    {
                        throw new CustomizedErrorException("不是有效的xml字符串");
                    }
                    var p = (RichTextBlock)XamlReader.Load(xaml);
                    return p;
                }
                else
                {
                    var tx = new RichTextBlock();
                    Paragraph paragraph = new Paragraph();
                    Run run = new Run() { Text = txt };
                    paragraph.Inlines.Add(run);
                    tx.Blocks.Add(paragraph);
                    return tx;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"富文本转换失败: {txt} || {input}", ex);
                var tx = new RichTextBlock();
                Paragraph paragraph = new Paragraph();
                Run run = new Run() { Text = txt };
                paragraph.Inlines.Add(run);
                tx.Blocks.Add(paragraph);
                return tx;
            }
        }

        public static int CalculateCommentTextLength(this string input)
        {
            var regex = new Regex(Constants.COMMENT_SPECIAL_TEXT_REGULAR);
            var newInput = regex.Replace(input, "");
            return newInput.Length;
        }

        public static string SubstringCommentText(this string input,int length)
        {
            var regex = new Regex(Constants.COMMENT_SPECIAL_TEXT_REGULAR);
            var matches = new Dictionary<int, string>();
            foreach (Match match in regex.Matches(input))
            {
                var index = match.Index;
                var value = match.Value;
                matches.Add(index, value);
            }
            var newInput = regex.Replace(input, "");
            if (newInput.Length < length) return input;
            var output = newInput.Substring(0, length);
            foreach (var pair in matches)
            {
                var index = pair.Key;
                var value = pair.Value;
                if (index < output.Length)
                {
                    output = output.Insert(index, value);
                }
                else
                {
                    break;
                }
            }
            return output;
        }

        public static string ProtectValues(this string url, params string[] keys)
        {
            foreach (string key in keys)
            {
                string pattern = $@"({key}=)([^&]*)";
                string replacement = $"$1{{hasValue}}";
                url = Regex.Replace(url, pattern, replacement);
            }
            return url;
        }

        public static string ChooseProxyServer(this string area)
        {
            var proxyUrl = SettingService.GetValue(SettingConstants.Roaming.CUSTOM_SERVER_URL, ApiHelper.ROMAING_PROXY_URL);
            var proxyUrlCN = SettingService.GetValue(SettingConstants.Roaming.CUSTOM_SERVER_URL_CN, "");
            var proxyUrlHK = SettingService.GetValue(SettingConstants.Roaming.CUSTOM_SERVER_URL_HK, "");
            var proxyUrlTW = SettingService.GetValue(SettingConstants.Roaming.CUSTOM_SERVER_URL_TW, "");
            switch (area)
            {
                case "cn":
                    return string.IsNullOrEmpty(proxyUrlCN) ? proxyUrl : proxyUrlCN;
                case "hk":
                    return string.IsNullOrEmpty(proxyUrlHK) ? proxyUrl : proxyUrlHK;
                case "tw":
                    return string.IsNullOrEmpty(proxyUrlTW) ? proxyUrl : proxyUrlTW;
                default:
                    return proxyUrl;
            }
        }

        public static string ParseArea(this string title, long mid)
        {
            if (Regex.IsMatch(title, @"僅.*港.*地區"))
            {
                return "hk";
            }
            else if (Regex.IsMatch(title, @"僅.*台.*地區"))
            {
                return "tw";
            }
            //如果是哔哩哔哩番剧出差这个账号上传的
            //且标题不含僅**地區，返回地区设置为港澳台
            if (mid == 11783021)
            {
                return "hk";
            }
            return "cn";
        }

        public static string ParseArea(this string title, string mid)
        {
            return title.ParseArea(mid.ToInt64());
        }

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToMD5(this string input)
        {
            var provider = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            IBuffer buffer = CryptographicBuffer.ConvertStringToBinary(input, BinaryStringEncoding.Utf8);
            var hashed = provider.HashData(buffer);
            var result = CryptographicBuffer.EncodeToHexString(hashed);
            return result;
        }

        public static string ToSimplifiedChinese(this string content)
        {
            content = content.TraditionalToSimplified();
            return content;
        }

        public static string RegexMatch(string input, string regular)
        {
            var data = Regex.Match(input, regular);
            if (data.Groups.Count >= 2 && data.Groups[1].Value != "")
            {
                return data.Groups[1].Value;
            }
            else
            {
                return "";
            }
        }
        public static async Task<T> DeserializeJson<T>(this string results)
        {
            return await Task.Run(() =>
            {
                return JsonConvert.DeserializeObject<T>(results);
            });
        }
        public static bool SetClipboard(this string content)
        {
            try
            {
                Windows.ApplicationModel.DataTransfer.DataPackage pack = new Windows.ApplicationModel.DataTransfer.DataPackage();
                pack.SetText(content);
                Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(pack);
                Windows.ApplicationModel.DataTransfer.Clipboard.Flush();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static Color StrToColor(this string obj)
        {
            obj = obj.Replace("#", "");
            if (int.TryParse(obj, out var c))
            {
                obj = c.ToString("X2");
            }
            Color color = new Color();
            if (obj.Length <= 6)
            {
                obj = obj.PadLeft(6, '0');
                color.R = byte.Parse(obj.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                color.G = byte.Parse(obj.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                color.B = byte.Parse(obj.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                color.A = 255;
            }
            else
            {
                obj = obj.PadLeft(8, '0');
                color.R = byte.Parse(obj.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                color.G = byte.Parse(obj.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                color.B = byte.Parse(obj.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                color.A = byte.Parse(obj.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return color;
        }

        /// <summary>
        /// 是否为正确格式的url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsUrl(this string url, UriKind kind = UriKind.Absolute)
        {
            return Uri.TryCreate(url, kind, out Uri _);
        }

        public static string UrlEncode(this string text)
        {
            return Uri.EscapeDataString(text);
        }

        public static bool IsXmlString(this string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            var detail = text.Trim();
            if (!detail.StartsWith("<") && !detail.EndsWith(">")) return false;
            var xml = new XmlDocument();
            try
            {
                xml.LoadXml($"<Root>{detail}</Root>");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static IBuffer StrToBuffer(this string text)
        {
            var data = System.Text.Encoding.UTF8.GetBytes(text);
            var buffer = data.AsBuffer();
            return buffer;
        }

        public static string GetImageTypeFromFileName(this string fileName)
        {
            // 获取文件扩展名并转换为小写
            string extension = Path.GetExtension(fileName)?.ToLower();

            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return "jpeg";
                case ".gif":
                    return "gif";
                case ".png":
                    return "png";
                default:
                    return "jpeg";
            }
        }

        #region Private methods

        private static string HandleTimeSeek(string input)
        {
            List<string> keyword = new List<string>();
            List<List<int>> haveHandledOffset = new List<List<int>>();
            // 正则表达式模式：匹配“mm:ss”和“hh:mm:ss”格式的时间
            var pattern = @"(?<!\d)(\d{1,}:\d{2}:\d{2}|\d{1,}:\d{2}|\d{1,}：\d{2}：\d{2}|\d{1,}：\d{2})(?!\d)";
            // 使用 Regex.Matches 获取所有匹配项
            MatchCollection matches = Regex.Matches(input, pattern);

            var offset = 0;
            foreach (Match item in matches)
            {
                if (keyword.Contains(item.Groups[0].Value) || haveHandledOffset
                        .Where(index => (item.Index + offset > index[0] && item.Index + offset < index[1])).ToList()
                        .Count > 0)
                {
                    continue;
                }

                keyword.Add(item.Groups[0].Value);
                var data =
                    @"<InlineUIContainer><HyperlinkButton Command=""{Binding SeekCommand}""  IsEnabled=""True"" Margin=""2 -3 2 -5"" Padding=""0 2 0 0"" " +
                    string.Format(
                        @" CommandParameter=""{0}"" ><TextBlock>⏩{0}</TextBlock></HyperlinkButton></InlineUIContainer>",
                        item.Groups[0].Value);
                input = input.Remove(item.Index + offset, item.Length);
                input = input.Insert(item.Index + offset, data);
                haveHandledOffset.Add(new List<int> { item.Index + offset, item.Index + offset + data.Length });
                offset += data.Length - item.Length;
            }
            return input;
        }

        /// <summary>
        /// 处理表情
        /// </summary>
        private static string HandelEmoji(string input, JObject emote)
        {
            if (emote == null) return input;
            //替换表情
            var mc = Regex.Matches(input, @"\[.*?\]");
            foreach (Match item in mc)
            {
                if (emote == null || !emote.ContainsKey(item.Groups[0].Value)) continue;
                var emoji = emote[item.Groups[0].Value];
                input = input.Replace(item.Groups[0].Value,
                    string.Format(
                        @"<InlineUIContainer><Border Margin=""2 0 2 -4""><Image Source=""{0}"" Width=""{1}"" Height=""{1}"" /></Border></InlineUIContainer>",
                        emoji["url"].ToString(), emoji["meta"]["size"].ToInt32() == 1 ? "20" : "36"));
            }

            return input;
        }

        /// <summary>
        /// 处理直播黄豆表情
        /// </summary>
        private static string HandleLiveEmoji(string input, JObject emotes)
        {
            if (emotes == null) return input;
            foreach (Match match in Regex.Matches(input, @"\[.*?\]"))
            {
                var emojiCode = match.Value;

                if (!emotes.TryGetValue(emojiCode, out var emote)) continue;
                var replacement = string.Format(
                    @"<InlineUIContainer><Border  Margin=""2 0 2 -4""><Image Source=""{0}"" Width=""{1}"" Height=""{1}"" /></Border></InlineUIContainer>",
                    emote["url"], emote["width"], emote["height"]);

                input = input.Replace(emojiCode, replacement);
            }

            return input;
        }

        /// <summary>
        /// 处理视频AVID,BVID,CVID
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string HandelVideoID(string input)
        {
            //处理AV号
            List<string> keyword = new List<string>();
            List<List<int>> haveHandledOffset = new List<List<int>>();
            //如果是链接就不处理了
            if (!Regex.IsMatch(input, @"(?<=://)[^\s]*[aAbBcC][vV]([a-zA-Z0-9]+)"))
            {
                var offset = 0;

                //处理BV号
                MatchCollection bv = Regex.Matches(input, @"[bB][vV]([a-zA-Z0-9]{8,})");
                offset = 0;
                foreach (Match item in bv)
                {
                    if (keyword.Contains(item.Groups[0].Value) || haveHandledOffset.Where(index => (item.Index + offset > index[0] && item.Index + offset < index[1])).ToList().Count > 0)
                    {
                        continue;
                    }

                    keyword.Add(item.Groups[0].Value);
                    var data =
                        @"<InlineUIContainer><HyperlinkButton Command=""{Binding LaunchUrlCommand}""  IsEnabled=""True"" Margin=""2 -3 2 -5"" Padding=""0 2 0 0"" " +
                        string.Format(
                            @" CommandParameter=""{1}"" ><TextBlock>🎞️{0}</TextBlock></HyperlinkButton></InlineUIContainer>",
                            item.Groups[0].Value, "bilibili://video/" + item.Groups[0].Value);
                    input = input.Remove(item.Index + offset, item.Length);
                    input = input.Insert(item.Index + offset, data);
                    haveHandledOffset.Add(new List<int> { item.Index + offset, item.Index + offset + data.Length });
                    offset += data.Length - item.Length;
                }

                //处理AV号
                MatchCollection av = Regex.Matches(input, @"[aA][vV](\d+)"); 
                foreach (Match item in av)
                {
                    if (keyword.Contains(item.Groups[0].Value) || haveHandledOffset.Where(index => (item.Index + offset > index[0] && item.Index + offset < index[1])).ToList().Count > 0 || item.Groups[1].Value.ToInt64() < 2)
                    {
                        continue;
                    }

                    keyword.Add(item.Groups[0].Value);
                    var data =
                        @"<InlineUIContainer><HyperlinkButton Command=""{Binding LaunchUrlCommand}""  IsEnabled=""True"" Margin=""2 -3 2 -5"" Padding=""0 2 0 0"" " +
                        string.Format(
                            @" CommandParameter=""{1}"" ><TextBlock>🎞️{0}</TextBlock></HyperlinkButton></InlineUIContainer>",
                            item.Groups[0].Value, "bilibili://video/" + item.Groups[0].Value);
                    input = input.Remove(item.Index + offset, item.Length);
                    input = input.Insert(item.Index + offset, data);
                    haveHandledOffset.Add(new List<int> { item.Index + offset, item.Index + offset + data.Length });
                    offset += data.Length - item.Length;
                }

                //处理CV号
                MatchCollection cv = Regex.Matches(input, @"[cC][vV](\d+)");
                offset = 0;
                foreach (Match item in cv)
                {
                    if (keyword.Contains(item.Groups[0].Value) || haveHandledOffset.Where(index => (item.Index > index[0] && item.Index < index[1])).ToList().Count > 0)
                    {
                        continue;
                    }

                    keyword.Add(item.Groups[0].Value);
                    var data =
                        @"<InlineUIContainer><HyperlinkButton Command=""{Binding LaunchUrlCommand}""  IsEnabled=""True"" Margin=""2 -3 2 -5"" Padding=""0 2 0 0"" " +
                        string.Format(
                            @" CommandParameter=""{1}"" ><TextBlock>📝{0}</TextBlock></HyperlinkButton></InlineUIContainer>",
                            item.Groups[0].Value, "bilibili://article/" + item.Groups[1].Value);
                    input = input.Remove(item.Index + offset, item.Length);
                    input = input.Insert(item.Index + offset, data);
                    haveHandledOffset.Add(new List<int> { item.Index + offset, item.Index + offset + data.Length });
                    offset += data.Length - item.Length;
                }
            }

            keyword.Clear();
            keyword = null;
            return input;
        }

        /// <summary>
        /// 处理URL链接
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string HandelUrl(string input)
        {
            List<string> keyword = new List<string>();
            MatchCollection url = Regex.Matches(input,
                @"(https?|ftp|file)://[-A-Za-z0-9+&@#/%?=~_|!:,.;]+[-A-Za-z0-9+&@#/%=~_|]");
            foreach (Match item in url)
            {
                if (keyword.Contains(item.Groups[0].Value))
                {
                    continue;
                }

                keyword.Add(item.Groups[0].Value);
                var data =
                    @"<InlineUIContainer><HyperlinkButton x:Name=""btn"" Command=""{Binding LaunchUrlCommand}""  IsEnabled=""True"" Margin=""2 -3 2 -5"" Padding=""0 2 0 0"" " +
                    string.Format(
                        @"ToolTipService.ToolTip=""{0}"" CommandParameter=""{0}"" ><TextBlock>🔗网页链接</TextBlock></HyperlinkButton></InlineUIContainer>",
                        item.Groups[0].Value.IsUrl() ? item.Groups[0].Value : ApiHelper.NOT_FOUND_URL);
                input = input.Replace(item.Groups[0].Value, data);
            }

            return input;
        }

        #endregion
    }
}
