using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using BiliLite.Models.Common.Msg;

namespace BiliLite.Controls.DataTemplateSelectors
{
    public class ChatMessageDataTemplateSelector : DataTemplateSelector
    {
        private static readonly Dictionary<ChatMsgType, Func<ChatMessageDataTemplateSelector, ChatMessage, DataTemplate>> _chatMsgTypeTemplateSelectFuncs;

        static ChatMessageDataTemplateSelector()
        {
            _chatMsgTypeTemplateSelectFuncs =
                new Dictionary<ChatMsgType, Func<ChatMessageDataTemplateSelector,
                    ChatMessage, DataTemplate>>()
                {
                    { ChatMsgType.Text, (selector, chatMessage) => selector.TextTemplate },
                    { ChatMsgType.Image, (selector, chatMessage) => selector.ImageTemplate },
                    { ChatMsgType.CustomEmote, (selector, chatMessage) => selector.ImageTemplate },
                    { ChatMsgType.Revoke, (selector, chatMessage) => selector.RevokeTemplate },
                    { ChatMsgType.Notification, (selector, chatMessage) => selector.NotificationTemplate },
                };
        }

        public DataTemplate TextTemplate { get; set; }

        public DataTemplate ImageTemplate { get; set; }

        public DataTemplate RevokeTemplate { get; set; }

        public DataTemplate NotificationTemplate { get; set; }

        public DataTemplate OtherTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            var chatMessage = item as ChatMessage;
            var success = _chatMsgTypeTemplateSelectFuncs.TryGetValue(chatMessage.MsgType, out var selectFunc);
            return success ? selectFunc(this, chatMessage) : OtherTemplate;
        }
    }
}
