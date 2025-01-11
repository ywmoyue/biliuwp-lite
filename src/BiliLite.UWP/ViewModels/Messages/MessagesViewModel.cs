using System.Collections.ObjectModel;
using BiliLite.Models.Attributes;
using BiliLite.Models.Common.Msg;
using BiliLite.ViewModels.Common;

namespace BiliLite.ViewModels.Messages
{
    [RegisterTransientViewModel]
    public class MessagesViewModel : BaseViewModel
    {
        public ObservableCollection<ChatContextViewModel> ChatContexts { get; set; }

        public ChatContextViewModel SelectedChatContext { get; set; }

        public ObservableCollection<ChatMessage> ChatMessages { get; set; }

        public bool HasMore { get; set; }
    }
}
