using System.Collections.ObjectModel;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common.Msg;
using BiliLite.ViewModels.Common;
using PropertyChanged;

namespace BiliLite.ViewModels.Messages
{
    [RegisterSingletonViewModel]
    public class MessagesViewModel : BaseViewModel
    {
        public ObservableCollection<ChatContextViewModel> ChatContexts { get; set; }

        public ChatContextViewModel SelectedChatContext { get; set; }

        public ObservableCollection<ChatMessage> ChatMessages { get; set; }

        public bool HasMoreContexts { get; set; }

        public bool HasMoreMessages { get; set; }

        public bool ChatContextLoading { get; set; }

        public bool ChatMessagesLoading { get; set; }

        [DoNotNotify]
        public string LastMsgId { get; set; }

        [DoNotNotify]
        public string NewMsgId { get; set; }

        public string ChatMessageInput { get; set; }

        [DependsOn(nameof(ChatMessageInput))] public int ChatMessageInputCount => ChatMessageInput?.Length ?? 0;

        public ObservableCollection<ReplyMeMessageViewModel> ReplyMeMessages { get; set; }

        public bool ReplyMeLoading { get; set; }

        public bool HasMoreReplyMe { get; set; }

        [DoNotNotify]
        public long? ReplyMeCurseId { get; set; }

        [DoNotNotify]
        public long? ReplyMeCurseTime { get; set; }
    }
}
