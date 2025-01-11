using System;
using BiliLite.Models.Common.Msg.MsgContent;
using Newtonsoft.Json;

namespace BiliLite.Models.Common.Msg;

public class ChatMessage
{
    private string m_contentStr;
    private IChatMsgContent m_chatMsgContent;

    public string ContentStr
    {
        get => m_contentStr;
        set
        {
            try
            {
                m_contentStr = value;

                switch (MsgType)
                {
                    case ChatMsgType.Text:
                    {
                        var msgContent = JsonConvert.DeserializeObject<TextChatMessageContent>(m_contentStr);
                        m_chatMsgContent = msgContent;
                        DisplayText = msgContent.Content;
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

    public IChatMsgContent Content => m_chatMsgContent;

    public TextChatMessageContent TextContent => m_chatMsgContent as TextChatMessageContent;

    public string UserId { get; set; }

    public string Face { get; set; }

    public string UserName { get; set; }

    public bool IsSelf { get; set; }

    public int IsSelfColumn => IsSelf ? 1 : 0;

    public ChatMsgType MsgType { get; set; }

    public string DisplayText { get; set; }
}