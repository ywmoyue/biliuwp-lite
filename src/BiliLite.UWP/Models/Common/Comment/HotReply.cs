using BiliLite.Extensions;
using BiliLite.Services;
using Newtonsoft.Json.Linq;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BiliLite.Models.Common.Comment
{
    public class HotReply
    {
        private static readonly ILogger _logger = GlobalLogger.FromCurrentType();

        public string UserName { get; set; }

        public string Message { get; set; }

        public JObject Emote { get; set; }

        public RichTextBlock Content
        {
            get
            {
                try
                {
                    RichTextBlock result;

                    var substringMsg = $"{Message.SubstringCommentText(50)}...";
                    result = substringMsg.ToRichTextBlock(Emote, lowProfilePrefix: $"{UserName}:  ");

                    if (Message.Length <= 50)
                    {
                        result = $"{Message}"
                            .ToRichTextBlock(Emote, lowProfilePrefix: $"{UserName}:  ");
                    }

                    result.TextAlignment = TextAlignment.Justify;
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.Error("热门回复加载失败", ex);
                    return new RichTextBlock();
                }
            }
        }
    }
}
