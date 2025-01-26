using System;
using System.Windows.Input;
using BiliLite.Models.Common.Msg.MsgContent;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Msg;

public class ChatMessage
{
    private IChatMsgContent m_chatMsgContent;

    public ICommand RevokeCommand { get; set; }

    public ICommand ImageCommand { get; set; }

    public ICommand NotificationActionCommand { get; set; }

    public string ChatMessageId { get; set; }

    public DateTimeOffset Time { get; set; }

    public string ContentStr { get; set; }

    public IChatMsgContent Content => m_chatMsgContent;

    public TextChatMessageContent TextContent => m_chatMsgContent as TextChatMessageContent;

    public NotificationChatMessageContent NotificationContent => m_chatMsgContent as NotificationChatMessageContent;

    public ImageChatMessageContent ImageContent => m_chatMsgContent as ImageChatMessageContent;

    public RevokeChatMessageContent RevokeContent => m_chatMsgContent as RevokeChatMessageContent;

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
                        var msgContent = JsonConvert.DeserializeObject<TextChatMessageContent>(ContentStr);
                        m_chatMsgContent = msgContent;
                        DisplayText = msgContent.Content;
                        break;
                    }
                case ChatMsgType.Image:
                case ChatMsgType.CustomEmote:
                    {
                        var msgContent = JsonConvert.DeserializeObject<ImageChatMessageContent>(ContentStr);
                        m_chatMsgContent = msgContent;
                        DisplayText = "[图片]";
                        break;
                    }
                case ChatMsgType.Revoke:
                    {
                        var msgContent = ContentStr;
                        var text = "对方撤回了一条消息";
                        if (IsSelf)
                        {
                            text = "你撤回了一条消息";
                        }
                        m_chatMsgContent = new RevokeChatMessageContent()
                        {
                            RevokeId = msgContent,
                            Text = text,
                        };
                        DisplayText = text;
                        break;
                    }
                case ChatMsgType.Notification:
                    {
                        var msgContent = JsonConvert.DeserializeObject<NotificationChatMessageContent>(ContentStr);
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