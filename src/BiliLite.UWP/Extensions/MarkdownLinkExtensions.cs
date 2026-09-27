using System;
using System.Collections.Generic;
using BiliLite.Models.Common;
using BiliLite.Pages.Other;
using BiliLite.Services;
using Microsoft.Toolkit.Parsers.Markdown;
using Windows.UI.Xaml.Controls;

namespace BiliLite.Extensions
{
    /// <summary>
    /// Markdown 文本中的自定义链接处理。
    /// MarkdownTextBlock 使用的解析器只会把 MarkdownDocument.KnownSchemes 中的协议渲染成链接，
    /// 不在列表中的协议（如 help://）会被当成普通文本，所以自定义协议需要先注册。
    /// </summary>
    public static class MarkdownLinkExtensions
    {
        private const string HelpScheme = "help";

        /// <summary>
        /// 帮助详情页链接前缀，help://{名字} 对应 Assets/Text/help-{名字}.md
        /// </summary>
        public const string HelpLinkPrefix = HelpScheme + "://";

        private static readonly object m_schemeLocker = new object();

        private static bool m_schemeRegistered;

        private static readonly Dictionary<string, string> HelpPageTitles = new Dictionary<string, string>()
        {
            { "play-stutter", "播放视频掉帧或卡死" },
            { "network", "应用无法联网" },
            { "garbled-text", "中文乱码" },
        };

        /// <summary>
        /// 注册自定义协议，需要在设置 MarkdownTextBlock.Text 之前调用
        /// </summary>
        public static void RegisterCustomSchemes()
        {
            if (m_schemeRegistered)
            {
                return;
            }

            lock (m_schemeLocker)
            {
                if (m_schemeRegistered)
                {
                    return;
                }

                if (!MarkdownDocument.KnownSchemes.Contains(HelpScheme))
                {
                    MarkdownDocument.KnownSchemes.Add(HelpScheme);
                }

                m_schemeRegistered = true;
            }
        }

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
        public static bool TryHandleCustomLink(string link)
        {
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
