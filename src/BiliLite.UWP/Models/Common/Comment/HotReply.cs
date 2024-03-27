using System;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using BiliLite.Extensions;
using Newtonsoft.Json.Linq;

namespace BiliLite.Models.Common.Comment
{
    public class HotReply
    {
        public string UserName { get; set; }

        public string Message { get; set; }

        public JObject Emote { get; set; }

        public RichTextBlock Content
        {
            get
            {
                if (Message.Length <= 50)
                {
                    return $"{UserName}: {Message}"
                        .ToRichTextBlock(Emote);
                }

                return $"{UserName}: {Message.SubstringCommentText(50)}..."
                    .ToRichTextBlock(Emote);
            }
        }
    }
}
