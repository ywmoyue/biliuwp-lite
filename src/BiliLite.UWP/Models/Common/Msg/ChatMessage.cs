using System;
using BiliLite.Models.Common.Msg.MsgContent;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Msg;

public class ChatMessage
{
    private string m_contentStr;
    private IChatMsgContent m_chatMsgContent;

    public string ChatMessageId { get; set; }

    public string ContentStr
    {
        get => m_contentStr;
        set
        {
            m_contentStr = value;
            UpdateContent();
        }
    }

    public IChatMsgContent Content => m_chatMsgContent;

    public TextChatMessageContent TextContent => m_chatMsgContent as TextChatMessageContent;

    public NotificationChatMessageContent NotificationContent => m_chatMsgContent as NotificationChatMessageContent;

    public ImageChatMessageContent ImageContent => m_chatMsgContent as ImageChatMessageContent;

    public string UserId { get; set; }

    public string Face { get; set; }

    public string UserName { get; set; }

    public bool IsSelf { get; set; }

    public ChatMsgType MsgType { get; set; }

    public string DisplayText { get; set; }

    public void UpdateContent()
    {
        try
        {
            switch (MsgType)
            {
                case ChatMsgType.Text:
                {
                    var msgContent = JsonConvert.DeserializeObject<TextChatMessageContent>(m_contentStr);
                    m_chatMsgContent = msgContent;
                    DisplayText = msgContent.Content;
                    break;
                }
                case ChatMsgType.Image:
                case ChatMsgType.CustomEmote:
                {
                    var msgContent = JsonConvert.DeserializeObject<ImageChatMessageContent>(m_contentStr);
                    m_chatMsgContent = msgContent;
                    DisplayText = "[图片]";
                    break;
                }
                case ChatMsgType.Notification:
                {
                    var msgContent = JsonConvert.DeserializeObject<NotificationChatMessageContent>(m_contentStr);
                    m_chatMsgContent = msgContent;
                    DisplayText = msgContent.Title;
                    break;
                }
                default:
                {
                    var msgContent = new TextChatMessageContent()
                    {
                        Content = "暂不支持的消息格式"
                    };
                    m_chatMsgContent = msgContent;
                    DisplayText = msgContent.Content;
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            // content不一定是json对象
        }
    }
}