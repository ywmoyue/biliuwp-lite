using System;
using System.Collections.Generic;
using BiliLite.Models.Common;
using BiliLite.Pages.Other;
using BiliLite.Services;
using Microsoft.UI.Xaml.Controls;

namespace BiliLite.Extensions
{
    /// <summary>
    /// Markdown 文本中的自定义链接处理。
    /// help://{名字} 这样的自定义协议会被解析成链接，点击后由这里跳转到对应的帮助详情页。
    /// </summary>
    public static class MarkdownLinkExtensions
    {
        private const string HelpScheme = "help";

        /// <summary>
        /// 帮助详情页链接前缀，help://{名字} 对应 Assets/Text/help-{名字}.md
        /// </summary>
        public const string HelpLinkPrefix = HelpScheme + "://";

        private static readonly Dictionary<string, string> HelpPageTitles = new Dictionary<string, string>()
        {
            { "play-stutter", "播放视频掉帧或卡死" },
            { "network", "应用无法联网" },
            { "garbled-text", "中文乱码" },
        };

        /// <summary>
        /// 当前链接是否为自定义协议链接
        /// </summary>
        public static bool IsCustomLink(this string link)
        {
            return !string.IsNullOrEmpty(link) &&
                   link.StartsWith(HelpLinkPrefix, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 处理自定义协议链接，返回是否已处理
        /// </summary>
        public static bool TryHandleCustomLink(Uri uri)
        {
            if (uri == null)
            {
                return false;
            }

            var link = uri.OriginalString;
            if (!link.IsCustomLink())
            {
                return false;
            }

            OpenHelpPage(link.Substring(HelpLinkPrefix.Length));

            return true;
        }

        /// <summary>
        /// 跳转到帮助详情页
        /// </summary>
        private static void OpenHelpPage(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            // 只允许帮助文件名，避免拼出预期之外的路径
            name = name.Trim().Trim('/');
            if (string.IsNullOrWhiteSpace(name) || !IsValidName(name))
            {
                return;
            }

            var title = HelpPageTitles.TryGetValue(name, out var pageTitle) ? pageTitle : "帮助";

            MessageCenter.NavigateToPage(null, new NavigationInfo()
            {
                icon = Symbol.Help,
                page = typeof(MarkdownViewerPage),
                title = title,
                parameters = new MarkdownViewerPagerParameter()
                {
                    Type = MarkdownViewerPagerParameterType.Link,
                    Value = $"ms-appx:///Assets/Text/help-{name}.md",
                },
                dontGoTo = false,
            });
        }

        private static bool IsValidName(string name)
        {
            foreach (var c in name)
            {
                if (!char.IsLetterOrDigit(c) && c != '-' && c != '_')
                {
                    return false;
                }
            }

            return true;
        }
    }
}
