using Windows.UI.Xaml.Controls;
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
                    return $"{Message}"
                        .ToRichTextBlock(Emote, lowProfilePrefix: $"{UserName}:  ");
                }

                return $"{Message.SubstringCommentText(50)}..."
                    .ToRichTextBlock(Emote, lowProfilePrefix: $"{UserName}:  ");
            }
        }
    }
}
