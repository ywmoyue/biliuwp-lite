using BiliLite.Models.Common.Msg;
using BiliLite.ViewModels.Common;
using PropertyChanged;

namespace BiliLite.ViewModels.Messages;

public class ChatContextViewModel : BaseViewModel
{
    private ChatMessage m_chatMessage;

    [DoNotNotify]
    public string ChatContextId { get; set; }

    [DoNotNotify]
    public string FromUserId { get; set; }

    [DoNotNotify]
    public string Title { get; set; }

    [DoNotNotify]
    public string Cover { get; set; }

    public ChatMessage LastMsg
    {
        get => m_chatMessage;
        set
        {
            m_chatMessage = value;
            m_chatMessage.UpdateContent();
        }
    }

    [DoNotNotify]
    public string HasNotify { get; set; }

    public int UnreadMsgCount { get; set; }

    [DependsOn(nameof(UnreadMsgCount))]
    public bool HasUnreadMsg => UnreadMsgCount > 0;

    [DoNotNotify]
    public int Type { get; set; }

    [DoNotNotify]
    public long Time { get; set; }
}