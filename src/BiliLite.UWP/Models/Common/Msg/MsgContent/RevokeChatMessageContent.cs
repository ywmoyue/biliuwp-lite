namespace BiliLite.Models.Common.Msg.MsgContent
{
    public class RevokeChatMessageContent : IChatMsgContent
    {
        public string RevokeId { get; set; }

        public string Text { get; set; }
    }
}
